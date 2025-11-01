using FluentResults;
using VectorStinger.Infrastructure.DataAccess.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VectorStinger.Foundation.Utilities;
using VectorStinger.Foundation.Utilities.Config;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace VectorStinger.Foundation.Abstractions.UserCase
{
    public interface IUserCase
    {
    }

    public abstract class BaseUseCase<T, V, W> : IUserCase
        where T : class, IUseCaseInput
        where V : class, IUseCaseOutput
        where W : UserCaseValidation<T>
    {
        #region Properties
        public string Description { get; set; } = "User Case";
        public string Summary { get; set; } = "User Case";
        public StateUserCase State => _state;
        #endregion

        #region Private Fields
        private readonly T _userCaseInput;
        private readonly UserCaseValidation<T> _validationRules;
        private readonly IRepository _repository;
        private readonly ILogger _logger;
        private static readonly ActivitySource ActivitySource = new("BaseUseCase");

        // Cambiar de estático a instancia
        private readonly bool _enableDetailedTelemetry = true;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private StateUserCase _state = StateUserCase.None;
        #endregion

        #region Constructor
        public BaseUseCase(T userCaseInput, W validationRules, IRepository repository, ILogger logger)
        {
            _validationRules = validationRules ?? throw new ArgumentNullException(nameof(validationRules));
            _userCaseInput = userCaseInput ?? throw new ArgumentNullException(nameof(userCaseInput));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger;

            // Leer configuración con manejo seguro de valores nulos/inexistentes
            var configValue = ConfigManager.GetConfiguration().GetSection("UseCase:EnableDetailedTelemetry").Value;
            _enableDetailedTelemetry = !string.IsNullOrEmpty(configValue) && Convert.ToBoolean(configValue);

            Summary = GenerateSummary();
        }
        #endregion

        #region Public Methods
        public async Task<Result<V>> ExecuteAsync(T input)
        {
            var useCaseName = GetType().Name;
            using var activity = ActivitySource.StartActivity($"UseCase.{useCaseName}");

            // Solo registrar si está habilitado
            if (_enableDetailedTelemetry)
            {
                var serializedInput = SerializeObject(input, "Input");
                _logger?.LogInformation("=== Ejecutando {UseCaseName} === Input: {SerializedInput}", useCaseName, serializedInput);
                activity?.SetTag("usecase.input_data", SerializeObjectForTelemetry(input, "Input"));
            }

            ConfigureActivityTags(activity, useCaseName);

            try
            {
                // Inicialización
                await SetStateAsync(StateUserCase.Initialized, activity, useCaseName);

                // Validación
                var validationResult = await ValidateInputAsync(input, activity, useCaseName);
                if (validationResult.IsFailed)
                    return validationResult;

                // Ejecución de lógica de negocio
                var businessResult = await ExecuteBusinessLogicAsync(input, activity, useCaseName);

                // Finalización
                await FinalizeExecutionAsync(businessResult, activity, useCaseName);

                // Log serialized output and add to activity
                if (businessResult.IsSuccess && _enableDetailedTelemetry)
                {
                    var serializedOutput = SerializeObject(businessResult.Value, "Output");
                    _logger?.LogInformation("=== {UseCaseName} completado exitosamente === Output: {SerializedOutput}", useCaseName, serializedOutput);
                    activity?.SetTag("usecase.output_data", SerializeObjectForTelemetry(businessResult.Value, "Output"));
                }

                return businessResult;
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync(ex, input, activity, useCaseName);
            }
            finally
            {
                LogExecutionEnd(useCaseName);
            }
        }

        public abstract Task<Result<V>> ExecuteBusinessAsync(T input);
        #endregion

        #region Private Methods - State Management
        private async Task SetStateAsync(StateUserCase newState, Activity activity, string useCaseName)
        {
            _state = newState;
            activity?.SetTag("usecase.state", _state.ToString());
            _logger?.LogDebug("Estado del caso de uso {UseCaseName} cambiado a: {State}", useCaseName, _state);
            await Task.CompletedTask;
        }
        #endregion

        #region Private Methods - Activity Configuration
        private void ConfigureActivityTags(Activity activity, string useCaseName)
        {
            if (activity == null) return;

            activity.SetTag("usecase.name", useCaseName);
            activity.SetTag("usecase.input_type", typeof(T).Name);
            activity.SetTag("usecase.output_type", typeof(V).Name);
            activity.SetTag("usecase.description", Description);
            activity.SetTag("usecase.assembly", GetType().Assembly.GetName().Name);
        }
        #endregion

        #region Private Methods - Logging
        private void LogExecutionStart(string useCaseName, T input)
        {
            _logger?.LogInformation("=== Iniciando ejecución del caso de uso: {UseCaseName} ===", useCaseName);

            if (_logger?.IsEnabled(LogLevel.Debug) == true)
            {
                var serializedInput = SerializeObject(input, "Input");
                _logger.LogDebug("Input serializado para {UseCaseName}: {SerializedInput}", useCaseName, serializedInput);
            }
        }

        private void LogExecutionEnd(string useCaseName)
        {
            _logger?.LogDebug("=== Finalizando ejecución del caso de uso {UseCaseName} con estado: {FinalState} ===",
                useCaseName, _state);
        }
        #endregion

        #region Private Methods - Validation
        private async Task<Result> ValidateInputAsync(T input, Activity activity, string useCaseName)
        {
            _logger?.LogDebug("Iniciando validación para caso de uso: {UseCaseName}", useCaseName);

            var validationResult = await _validationRules.ValidateAsync(input);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                await SetStateAsync(StateUserCase.ValidationError, activity, useCaseName);

                ConfigureValidationErrorTelemetry(activity, errors);
                LogValidationErrors(useCaseName, errors);

                return Result.Fail(errors);
            }

            _logger?.LogDebug("Validación exitosa para caso de uso: {UseCaseName}", useCaseName);
            return Result.Ok(); // Placeholder for continuing execution
        }

        private void ConfigureValidationErrorTelemetry(Activity activity, List<string> errors)
        {
            if (activity == null) return;

            activity.SetTag("usecase.validation_errors", string.Join("; ", errors));
            activity.SetTag("usecase.validation_error_count", errors.Count);
            activity.SetStatus(ActivityStatusCode.Error, "Validation failed");
        }

        private void LogValidationErrors(string useCaseName, List<string> errors)
        {
            _logger?.LogWarning("Errores de validación en caso de uso {UseCaseName}: {ErrorCount} errores encontrados",
                useCaseName, errors.Count);

            foreach (var (error, index) in errors.Select((error, index) => (error, index)))
            {
                _logger?.LogWarning("Error de validación #{ErrorIndex} en {UseCaseName}: {ErrorMessage}",
                    index + 1, useCaseName, error);
            }
        }
        #endregion

        #region Private Methods - Business Logic
        private async Task<Result<V>> ExecuteBusinessLogicAsync(T input, Activity activity, string useCaseName)
        {
            _logger?.LogDebug("Iniciando ejecución de lógica de negocio para: {UseCaseName}", useCaseName);

            var stopwatch = Stopwatch.StartNew();
            var result = await ExecuteBusinessAsync(input);
            stopwatch.Stop();

            activity?.SetTag("usecase.business_execution_time_ms", stopwatch.ElapsedMilliseconds);

            if (_logger?.IsEnabled(LogLevel.Debug) == true && result.IsSuccess)
            {
                var serializedOutput = SerializeObject(result.Value, "Output");
                _logger.LogDebug("Output serializado para {UseCaseName}: {SerializedOutput}", useCaseName, serializedOutput);
            }

            _logger?.LogInformation("Lógica de negocio ejecutada para {UseCaseName} en {ExecutionTime}ms - Estado: {ResultStatus}",
                useCaseName, stopwatch.ElapsedMilliseconds, result.IsSuccess ? "Éxito" : "Fallo");

            return result;
        }
        #endregion

        #region Private Methods - Finalization
        private async Task FinalizeExecutionAsync(Result<V> result, Activity activity, string useCaseName)
        {
            if (result.IsSuccess)
            {
                await FinalizeSuccessfulExecutionAsync(activity, useCaseName);
            }
            else
            {
                await FinalizeFailedExecutionAsync(result, activity, useCaseName);
            }
        }

        private async Task FinalizeSuccessfulExecutionAsync(Activity activity, string useCaseName)
        {
            await SetStateAsync(StateUserCase.Completed, activity, useCaseName);
            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger?.LogDebug("Ejecutando Commit para caso de uso: {UseCaseName}", useCaseName);

            var commitSuccess = _repository.Commit();
            activity?.SetTag("usecase.commit_success", commitSuccess);

            _logger?.LogInformation("✅ Caso de uso {UseCaseName} ejecutado exitosamente", useCaseName);
        }

        private async Task FinalizeFailedExecutionAsync(Result<V> result, Activity activity, string useCaseName)
        {
            await SetStateAsync(StateUserCase.Crushed, activity, useCaseName);

            var businessErrors = result.Errors.Select(e => e.Message).ToList();
            activity?.SetTag("usecase.business_errors", string.Join("; ", businessErrors));
            activity?.SetTag("usecase.business_error_count", businessErrors.Count);
            activity?.SetStatus(ActivityStatusCode.Error, "Business logic failed");

            _logger?.LogWarning("❌ Errores en lógica de negocio del caso de uso {UseCaseName}: {ErrorCount} errores",
                useCaseName, businessErrors.Count);

            foreach (var (error, index) in businessErrors.Select((error, index) => (error, index)))
            {
                _logger?.LogWarning("Error de negocio #{ErrorIndex} en {UseCaseName}: {ErrorMessage}",
                    index + 1, useCaseName, error);
            }

            var rollbackSuccess = _repository.Rollback();
            activity?.SetTag("usecase.rollback_success", rollbackSuccess);
        }
        #endregion

        #region Private Methods - Exception Handling
        private async Task<Result<V>> HandleExceptionAsync(Exception ex, T input, Activity activity, string useCaseName)
        {
            await SetStateAsync(StateUserCase.Crushed, activity, useCaseName);

            ConfigureExceptionTelemetry(ex, activity);
            LogException(ex, input, useCaseName);
            LogInnerException(ex, useCaseName);

            await ExecuteRollbackAsync(activity, useCaseName);

            return Result.Fail<V>(ex.Message);
        }

        private void ConfigureExceptionTelemetry(Exception ex, Activity activity)
        {
            if (activity == null) return;

            activity.SetTag("usecase.exception_type", ex.GetType().Name);
            activity.SetTag("usecase.exception_message", ex.Message);
            activity.SetTag("usecase.has_inner_exception", ex.InnerException != null);
            activity.SetStatus(ActivityStatusCode.Error, ex.Message);

            if (ex.InnerException != null)
            {
                activity.SetTag("usecase.inner_exception_type", ex.InnerException.GetType().Name);
                activity.SetTag("usecase.inner_exception_message", ex.InnerException.Message);
            }
        }

        private void LogException(Exception ex, T input, string useCaseName)
        {
            var serializedInput = SerializeObject(input, "Input (en excepción)");

            _logger?.LogError(ex,
                "🔥 Error crítico en caso de uso {UseCaseName}. " +
                "Tipo de excepción: {ExceptionType}, " +
                "Mensaje: {ExceptionMessage}, " +
                "Input: {SerializedInput}",
                useCaseName,
                ex.GetType().Name,
                ex.Message,
                serializedInput);
        }

        private void LogInnerException(Exception ex, string useCaseName)
        {
            if (ex.InnerException == null) return;

            _logger?.LogError("🔗 Inner Exception en {UseCaseName}: {InnerExceptionType} - {InnerExceptionMessage}",
                useCaseName, ex.InnerException.GetType().Name, ex.InnerException.Message);
        }

        private async Task ExecuteRollbackAsync(Activity activity, string useCaseName)
        {
            try
            {
                _logger?.LogDebug("Ejecutando Rollback debido a excepción en caso de uso: {UseCaseName}", useCaseName);

                var rollbackSuccess = _repository.Rollback();
                activity?.SetTag("usecase.rollback_success", rollbackSuccess);

                _logger?.LogDebug("Rollback ejecutado {Status} para caso de uso: {UseCaseName}",
                    rollbackSuccess ? "exitosamente" : "con errores", useCaseName);
            }
            catch (Exception rollbackEx)
            {
                _logger?.LogError(rollbackEx,
                    "💥 Error crítico durante Rollback en caso de uso {UseCaseName}: {RollbackError}",
                    useCaseName, rollbackEx.Message);

                activity?.SetTag("usecase.rollback_error", rollbackEx.Message);
                activity?.SetTag("usecase.rollback_success", false);
            }

            await Task.CompletedTask;
        }
        #endregion

        #region Private Methods - Serialization
        private string SerializeObject<TObject>(TObject obj, string context = "Object")
        {
            if (obj == null)
                return $"[{context}: null]";

            try
            {
                return JsonSerializer.Serialize(obj, JsonOptions);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning("No se pudo serializar {Context} del tipo {ObjectType}: {Error}",
                    context, typeof(TObject).Name, ex.Message);
                return $"[{context}: Error de serialización - {ex.Message}]";
            }
        }

        private string SerializeObjectForTelemetry<TObject>(TObject obj, string context = "Object", int maxLength = 8000)
        {
            var serialized = SerializeObject(obj, context);

            if (serialized.Length > maxLength)
            {
                return serialized.Substring(0, maxLength) + "... [TRUNCATED]";
            }

            return serialized;
        }

        private string GenerateSummary()
        {
            var inputSchema = GenerateJsonFromClass<T>();
            var outputSchema = GenerateJsonFromClass<V>();

            return $"Input: {inputSchema} <br> <hr> Output: {outputSchema}";
        }

        public string GenerateJsonFromClass<H>() where H : class
        {
            var type = typeof(H);

            if (type == null)
                throw new ArgumentException("Clase no encontrada");

            try
            {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var schema = properties.ToDictionary(
                    prop => prop.Name,
                    prop => GetDefaultValueForType(prop.PropertyType));

                return JsonSerializer.Serialize(schema, JsonOptions);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning("Error generando esquema JSON para {TypeName}: {Error}", type.Name, ex.Message);
                return $"{{\"error\": \"No se pudo generar esquema para {type.Name}\"}}";
            }
        }

        private static object? GetDefaultValueForType(Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            if (type == typeof(string))
                return "string";

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return null;

            return $"<{type.Name}>";
        }
        #endregion
    }
}
