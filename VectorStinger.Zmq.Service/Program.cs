using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using MessagePack;
using System.Reflection;
using System.Threading.Channels;
using VectorStinger.Application.Configurations;
using VectorStinger.Core.Configurations;
using VectorStinger.Foundation.Abstractions.UserCase;
using VectorStinger.Host.ServiceDefaults;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        // Add service defaults for telemetry and configuration
        builder.AddServiceDefaults();

        // Configure logging
        builder.Logging.AddConsole();

        // Register configuration options
        builder.Services.AddOptions<DatabaseSettings>().Bind(builder.Configuration.GetSection("DatabaseSettings"));
        builder.Services.AddOptions<PaymentBridgeSettings>().Bind(builder.Configuration.GetSection("PaymentBridgeSettings"));
        builder.Services.AddOptions<List<VectorStingerFolder>>().Bind(builder.Configuration.GetSection("Folders"));

        // Get configuration instances
        var databaseSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>();
        List<VectorStingerFolder> roomsyFolders = builder.Configuration.GetSection("Folders").Get<List<VectorStingerFolder>>()!;
        
        if (roomsyFolders != null)
        {
            builder.Services.RegisterFoldersConfiguration(roomsyFolders);
        }

        List<Type> userCaseTypes = new List<Type>();

        // Register UserCases if database connection is configured
        if (!string.IsNullOrEmpty(databaseSettings?.DefaultConnection))
        {
            var serviceUserCase = builder.Services.RegisterUserCases(userCaseTypes, databaseSettings!, builder.Configuration);
        }

        builder.Services.AddMemoryCache();

        // Add hosted service for ZeroMQ
        builder.Services.AddHostedService<ZmqHostedService>(sp => 
            new ZmqHostedService(
                sp,
                sp.GetRequiredService<ILogger<ZmqHostedService>>(),
                builder.Configuration,
                userCaseTypes
            )
        );

        var host = builder.Build();
        await host.RunAsync();
    }
}

public class ZmqHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ZmqHostedService> _logger;
    private readonly IConfiguration _configuration;
    private readonly List<Type> _userCaseTypes;

    public ZmqHostedService(
        IServiceProvider serviceProvider,
        ILogger<ZmqHostedService> logger,
        IConfiguration configuration,
        List<Type> userCaseTypes)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
        _userCaseTypes = userCaseTypes;
    }

    private record WorkItem(byte[] ClientId, string UseCaseName, byte[] RequestData);
    private record WorkResult(byte[] ClientId, byte[] ResponseData);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bindAddress = _configuration.GetValue<string>("ZmqSettings:BindAddress") ?? "tcp://*:5555";
        var workerThreads = _configuration.GetValue<int>("ZmqSettings:WorkerThreads");
        if (workerThreads <= 0) workerThreads = 4;

        _logger.LogInformation("Starting ZeroMQ service on {BindAddress} with {WorkerThreads} worker threads", 
            bindAddress, workerThreads);

        using var router = new RouterSocket();
        router.Bind(bindAddress);

        _logger.LogInformation("ZeroMQ service is listening on {BindAddress}", bindAddress);
        _logger.LogInformation("Registered {Count} use cases", _userCaseTypes.Count);

        // Create channels for work distribution
        var workChannel = Channel.CreateUnbounded<WorkItem>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = true
        });

        var resultChannel = Channel.CreateUnbounded<WorkResult>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        // Start the single reader task (reads from RouterSocket)
        var readerTask = Task.Run(async () => await ReadMessagesAsync(router, workChannel.Writer, stoppingToken), stoppingToken);

        // Start the single writer task (writes to RouterSocket)
        var writerTask = Task.Run(async () => await WriteMessagesAsync(router, resultChannel.Reader, stoppingToken), stoppingToken);

        // Start worker tasks (process requests)
        var workerTasks = new List<Task>();
        for (int i = 0; i < workerThreads; i++)
        {
            int workerId = i;
            workerTasks.Add(Task.Run(async () => 
                await ProcessWorkerAsync(workerId, workChannel.Reader, resultChannel.Writer, stoppingToken), stoppingToken));
        }

        try
        {
            await Task.WhenAll(readerTask, writerTask, Task.WhenAll(workerTasks));
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("ZeroMQ service is shutting down");
        }
        finally
        {
            workChannel.Writer.TryComplete();
            resultChannel.Writer.TryComplete();
        }
    }

    private async Task ReadMessagesAsync(RouterSocket router, ChannelWriter<WorkItem> workWriter, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reader started");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Use polling with timeout to check cancellation
                    if (!router.TryReceiveFrameBytes(TimeSpan.FromMilliseconds(100), out var clientIdFrame))
                    {
                        continue;
                    }

                    var emptyFrame = router.ReceiveFrameBytes();
                    var useCaseNameFrame = router.ReceiveFrameString();
                    var requestDataFrame = router.ReceiveFrameBytes();

                    _logger.LogDebug("Reader received request for use case: {UseCaseName}", useCaseNameFrame);

                    // Write to work channel
                    var workItem = new WorkItem(clientIdFrame, useCaseNameFrame, requestDataFrame);
                    await workWriter.WriteAsync(workItem, stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Reader encountered an error");
                    await Task.Delay(100, stoppingToken);
                }
            }
        }
        finally
        {
            workWriter.TryComplete();
            _logger.LogInformation("Reader stopped");
        }
    }

    private async Task WriteMessagesAsync(RouterSocket router, ChannelReader<WorkResult> resultReader, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Writer started");

        try
        {
            await foreach (var result in resultReader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    // Send response back to client
                    router.SendMoreFrame(result.ClientId)
                          .SendMoreFrame(Array.Empty<byte>())
                          .SendFrame(result.ResponseData);

                    _logger.LogDebug("Writer sent response to client");
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Writer encountered an error sending response");
                }
            }
        }
        finally
        {
            _logger.LogInformation("Writer stopped");
        }
    }

    private async Task ProcessWorkerAsync(int workerId, ChannelReader<WorkItem> workReader, 
        ChannelWriter<WorkResult> resultWriter, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker {WorkerId} started", workerId);

        try
        {
            await foreach (var workItem in workReader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    _logger.LogDebug("Worker {WorkerId} processing request for use case: {UseCaseName}", 
                        workerId, workItem.UseCaseName);

                    byte[] responseData;
                    try
                    {
                        responseData = await ProcessRequestAsync(workItem.UseCaseName, workItem.RequestData);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Worker {WorkerId} error processing request for {UseCaseName}", 
                            workerId, workItem.UseCaseName);
                        
                        var errorResponse = new { IsSuccess = false, Error = ex.Message };
                        responseData = MessagePackSerializer.Serialize(errorResponse);
                    }

                    // Write result to result channel
                    var result = new WorkResult(workItem.ClientId, responseData);
                    await resultWriter.WriteAsync(result, stoppingToken);

                    _logger.LogDebug("Worker {WorkerId} completed request for use case: {UseCaseName}", 
                        workerId, workItem.UseCaseName);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Worker {WorkerId} encountered an error", workerId);
                }
            }
        }
        finally
        {
            _logger.LogInformation("Worker {WorkerId} stopped", workerId);
        }
    }

    private async Task<byte[]> ProcessRequestAsync(string useCaseName, byte[] requestData)
    {
        var useCaseType = _userCaseTypes.FirstOrDefault(t => t.Name == useCaseName);
        if (useCaseType == null)
        {
            _logger.LogWarning("Use case not found: {UseCaseName}", useCaseName);
            var errorResponse = new { IsSuccess = false, Error = $"Use case '{useCaseName}' not found" };
            return MessagePackSerializer.Serialize(errorResponse);
        }

        var handleMethod = useCaseType.GetMethod("ExecuteAsync");
        if (handleMethod == null)
        {
            _logger.LogWarning("ExecuteAsync method not found for use case: {UseCaseName}", useCaseName);
            var errorResponse = new { IsSuccess = false, Error = $"ExecuteAsync method not found for '{useCaseName}'" };
            return MessagePackSerializer.Serialize(errorResponse);
        }

        var inputParameterType = handleMethod.GetParameters().FirstOrDefault()?.ParameterType;
        if (inputParameterType == null || !typeof(IUseCaseInput).IsAssignableFrom(inputParameterType))
        {
            _logger.LogWarning("Invalid input parameter type for use case: {UseCaseName}", useCaseName);
            var errorResponse = new { IsSuccess = false, Error = $"Invalid input parameter for '{useCaseName}'" };
            return MessagePackSerializer.Serialize(errorResponse);
        }

        using var scope = _serviceProvider.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        try
        {
            var input = MessagePackSerializer.Deserialize(inputParameterType, requestData);
            var useCaseInstance = serviceProvider.GetRequiredService(useCaseType);

            var resultTask = (Task)handleMethod.Invoke(useCaseInstance, new object[] { input! })!;
            await resultTask.ConfigureAwait(false);

            var resultProperty = resultTask.GetType().GetProperty("Result");
            var result = resultProperty?.GetValue(resultTask);

            // Extraer el valor del Result<T> usando reflection
            if (result != null)
            {
                var resultType = result.GetType();
                
                // Verificar si es un Result<T> de FluentResults
                if (resultType.IsGenericType && resultType.GetGenericTypeDefinition().Name == "Result`1")
                {
                    var isSuccessProperty = resultType.GetProperty("IsSuccess");
                    var isSuccess = (bool)(isSuccessProperty?.GetValue(result) ?? false);
                    
                    if (isSuccess)
                    {
                        // Extraer el valor del resultado exitoso
                        var valueProperty = resultType.GetProperty("Value");
                        var value = valueProperty?.GetValue(result);
                        
                        // Serializar solo el valor, no el Result<T>
                        return MessagePackSerializer.Serialize(value);
                    }
                    else
                    {
                        // Extraer los errores
                        var errorsProperty = resultType.GetProperty("Errors");
                        var errors = errorsProperty?.GetValue(result) as IEnumerable<object>;
                        var errorMessages = errors?.Select(e => 
                        {
                            var msgProp = e.GetType().GetProperty("Message");
                            return msgProp?.GetValue(e)?.ToString() ?? "Unknown error";
                        }).ToList() ?? new List<string>();
                        
                        var errorResponse = new { IsSuccess = false, Errors = errorMessages };
                        return MessagePackSerializer.Serialize(errorResponse);
                    }
                }
            }

            return MessagePackSerializer.Serialize(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing use case: {UseCaseName}", useCaseName);
            var errorResponse = new { IsSuccess = false, Error = ex.Message };
            return MessagePackSerializer.Serialize(errorResponse);
        }
    }
}
