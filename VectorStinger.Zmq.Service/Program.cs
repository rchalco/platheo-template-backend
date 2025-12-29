using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using MessagePack;
using System.Reflection;
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

        // Create a pool of worker tasks
        var workerTasks = new List<Task>();
        for (int i = 0; i < workerThreads; i++)
        {
            int workerId = i;
            workerTasks.Add(Task.Run(async () => await ProcessMessagesAsync(router, workerId, stoppingToken), stoppingToken));
        }

        try
        {
            await Task.WhenAll(workerTasks);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("ZeroMQ service is shutting down");
        }
    }

    private async Task ProcessMessagesAsync(RouterSocket router, int workerId, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker {WorkerId} started", workerId);

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

                _logger.LogDebug("Worker {WorkerId} received request for use case: {UseCaseName}", 
                    workerId, useCaseNameFrame);

                byte[] responseData;
                try
                {
                    responseData = await ProcessRequestAsync(useCaseNameFrame, requestDataFrame);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Worker {WorkerId} error processing request for {UseCaseName}", 
                        workerId, useCaseNameFrame);
                    
                    var errorResponse = new { IsSuccess = false, Error = ex.Message };
                    responseData = MessagePackSerializer.Serialize(errorResponse);
                }

                // Send response back to client
                router.SendMoreFrame(clientIdFrame)
                      .SendMoreFrame(Array.Empty<byte>())
                      .SendFrame(responseData);

                _logger.LogDebug("Worker {WorkerId} sent response for use case: {UseCaseName}", 
                    workerId, useCaseNameFrame);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Worker {WorkerId} encountered an error", workerId);
                await Task.Delay(100, stoppingToken); // Small delay to prevent tight loop on error
            }
        }

        _logger.LogInformation("Worker {WorkerId} stopped", workerId);
    }

    private async Task<byte[]> ProcessRequestAsync(string useCaseName, byte[] requestData)
    {
        // Find the use case type
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
        var userCaseInstance = serviceProvider.GetRequiredService(useCaseType);

        try
        {
            // Deserialize input using MessagePack
            var input = MessagePackSerializer.Deserialize(inputParameterType, requestData);
            
            // Invoke the method
            var resultTask = (Task)handleMethod.Invoke(userCaseInstance, new object[] { input! })!;
            await resultTask;

            // Get the result
            var resultProperty = resultTask.GetType().GetProperty("Result");
            var result = resultProperty?.GetValue(resultTask);

            // Extract success, errors, and value
            var resultType = resultTask.GetType().GetProperty("Result")?.PropertyType;
            var successProperty = resultType?.GetProperty("IsSuccess");
            var errorsProperty = resultType?.GetProperty("Errors");
            var valueProperty = resultType?.GetProperty("Value");

            var isSuccess = (bool)successProperty?.GetValue(result)!;
            var errors = isSuccess ? null : errorsProperty?.GetValue(result);
            var value = isSuccess ? valueProperty?.GetValue(result) : null;

            // Serialize response using MessagePack
            var response = new
            {
                IsSuccess = isSuccess,
                Value = value,
                Errors = errors
            };

            return MessagePackSerializer.Serialize(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing use case: {UseCaseName}", useCaseName);
            var errorResponse = new { IsSuccess = false, Error = ex.Message };
            return MessagePackSerializer.Serialize(errorResponse);
        }
    }
}
