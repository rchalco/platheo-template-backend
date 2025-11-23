using VectorStinger.Infrastructure.DataAccess.Interface;
using VectorStinger.Infrastructure.DataAccess.Wrapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using VectorStinger.Foundation.Utilities.CrossUtil;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using PropertyDictionary = System.Collections.Frozen.FrozenDictionary<string, System.Reflection.PropertyInfo>;
using SqlParameterList = System.Collections.Generic.List<Microsoft.Data.SqlClient.SqlParameter>;
using SqlParameterDict = System.Collections.Generic.Dictionary<object, Microsoft.Data.SqlClient.SqlParameter>;

namespace VectorStinger.Infrastructure.DataAccess.Implement.SQLServer;

public sealed class MSSQLRepository<TDbContext> : IRepository where TDbContext : DbContext
{
    #region Variables
    private const string PREFIX_PARAMETER_NAME = "parameter";
    private readonly string _connectionString;
    private SqlConnection? sqlConnection;
    private IDbContextTransaction? _transaction;
    private readonly TDbContext _dbContext;
    private readonly ILogger<MSSQLRepository<TDbContext>>? _logger;
    private static readonly ActivitySource ActivitySource = new("MSSQLRepository");
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    // .NET 10 optimizations: FrozenDictionary for immutable, faster lookups
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();
    private static readonly ConcurrentDictionary<Type, Type> _underlyingTypeCache = new();
    private static readonly ConcurrentDictionary<Type, FrozenDictionary<string, PropertyInfo>> _propertyLookupCache = new();
    #endregion

    public MSSQLRepository(TDbContext dbContext, string connectionString, ILogger<MSSQLRepository<TDbContext>>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        _dbContext = dbContext;
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<bool> CallProcedureAsync<T>(string nameProcedure, params object[] parameters) where T : class, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nameProcedure);

        using var activity = ActivitySource.StartActivity($"SQLServer.CallProcedure.{nameProcedure}");
        activity?.SetTag("db.system", "sqlserver");
        activity?.SetTag("db.operation", "call_procedure");
        activity?.SetTag("db.name", nameProcedure);

        try
        {
            _logger?.LogInformation("Ejecutando procedimiento almacenado: {ProcedureName} con {ParameterCount} parámetros",
                nameProcedure, parameters?.Length ?? 0);

            var type = _dbContext.GetType();
            var methodInfo = type.GetMethod(nameProcedure);

            if (methodInfo is not null)
            {
                activity?.SetTag("db.execution_type", "ef_core");
                methodInfo.Invoke(_dbContext, parameters);
            }
            else
            {
                activity?.SetTag("db.execution_type", "ado_net");
                await CallProcedureADOAsync(nameProcedure, parameters).ConfigureAwait(false);
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            _logger?.LogInformation("Procedimiento {ProcedureName} ejecutado exitosamente", nameProcedure);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error ejecutando procedimiento almacenado: {ProcedureName}", nameProcedure);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<bool> CommitAsync(CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("SQLServer.Commit");
        activity?.SetTag("db.system", "sqlserver");
        activity?.SetTag("db.operation", "commit");

        try
        {
            _logger?.LogInformation("Ejecutando Commit en SQL Server");

            if (_transaction is not null)
            {
                await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
                    await _transaction.DisposeAsync().ConfigureAwait(false);
                    _transaction = null;
                    await _dbContext.Database.CloseConnectionAsync().ConfigureAwait(false);
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            _logger?.LogInformation("Commit completado exitosamente");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error durante Commit en SQL Server");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<List<T>> GetAllAsync<T>(CancellationToken cancellationToken = default) where T : class, new()
    {
        using var activity = ActivitySource.StartActivity($"SQLServer.GetAll.{typeof(T).Name}");
        activity?.SetTag("db.system", "sqlserver");
        activity?.SetTag("db.operation", "select_all");
        activity?.SetTag("db.table", typeof(T).Name);

        try
        {
            _logger?.LogInformation("Obteniendo todos los registros de tabla: {TableName}", typeof(T).Name);

            // EF Core 10: Use AsSplitQuery for better performance with related entities
            var result = await _dbContext.Set<T>()
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            activity?.SetTag("db.rows_affected", result.Count);
            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger?.LogInformation("GetAll de {TableName} retornó {RowCount} registros",
                typeof(T).Name, result.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error obteniendo todos los registros de tabla: {TableName}", typeof(T).Name);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<List<T>> GetDataByProcedureAsync<T>(string nameProcedure, params object[] parameters) where T : class, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nameProcedure);

        using var activity = ActivitySource.StartActivity($"SQLServer.GetDataByProcedure.{nameProcedure}");
        activity?.SetTag("db.system", "sqlserver");
        activity?.SetTag("db.operation", "select_procedure");
        activity?.SetTag("db.name", nameProcedure);

        try
        {
            _logger?.LogInformation("Obteniendo datos por procedimiento: {ProcedureName} con {ParameterCount} parámetros",
                nameProcedure, parameters?.Length ?? 0);

            var type = _dbContext.GetType();
            var methodInfo = type.GetMethod(nameProcedure);

            List<T> result;

            if (methodInfo is null)
            {
                activity?.SetTag("db.execution_type", "ado_net");
                result = await GetListByProcedureADOAsync<T>(nameProcedure, parameters).ConfigureAwait(false);
            }
            else
            {
                activity?.SetTag("db.execution_type", "ef_core");
                var obj = Activator.CreateInstance(type);
                var procedureResult = methodInfo.Invoke(obj, parameters);
                var toListMethod = typeof(Enumerable).GetMethod("ToList");
                var genericMethod = toListMethod!.MakeGenericMethod(typeof(T));
                result = (genericMethod.Invoke(procedureResult, [procedureResult]) as List<T>)!;
            }

            activity?.SetTag("db.rows_affected", result.Count);
            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger?.LogInformation("Procedimiento {ProcedureName} retornó {RowCount} registros",
                nameProcedure, result.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error obteniendo datos por procedimiento: {ProcedureName}", nameProcedure);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<bool> RollbackAsync(CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity("SQLServer.Rollback");
        activity?.SetTag("db.system", "sqlserver");
        activity?.SetTag("db.operation", "rollback");

        try
        {
            _logger?.LogInformation("Ejecutando Rollback en SQL Server");

            if (_transaction is not null)
            {
                await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    await _transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    await _transaction.DisposeAsync().ConfigureAwait(false);
                    _transaction = null;
                    await _dbContext.Database.CloseConnectionAsync().ConfigureAwait(false);
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            _logger?.LogInformation("Rollback completado exitosamente");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error durante Rollback en SQL Server");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<bool> SaveObjectAsync<T>(Entity<T> entity, CancellationToken cancellationToken = default) where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(entity);

        using var activity = ActivitySource.StartActivity($"SQLServer.SaveObject.{typeof(T).Name}");
        activity?.SetTag("db.system", "sqlserver");
        activity?.SetTag("db.table", typeof(T).Name);

        try
        {
            if (entity.stateEntity == StateEntity.none)
            {
                throw new ArgumentException("No se definió un estado para la entidad");
            }

            ArgumentNullException.ThrowIfNull(entity.EntityDB, "No se tiene una entidad válida; entidad interna nula");

            activity?.SetTag("db.operation", entity.stateEntity.ToString());
            _logger?.LogInformation("Guardando objeto {TableName} con operación: {Operation}",
                typeof(T).Name, entity.stateEntity);

            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                _transaction ??= await _dbContext.Database
                    .BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)
                    .ConfigureAwait(false);

                activity?.SetTag("db.transaction_started", "true");

                switch (entity.stateEntity)
                {
                    case StateEntity.add:
                        await _dbContext.AddAsync(entity.EntityDB, cancellationToken).ConfigureAwait(false);
                        break;

                    case StateEntity.modify:
                        _dbContext.Update(entity.EntityDB);
                        break;

                    case StateEntity.remove:
                        _dbContext.Remove(entity.EntityDB);
                        break;
                }

                var changes = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                activity?.SetTag("db.changes", changes);
            }
            finally
            {
                _semaphore.Release();
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            _logger?.LogInformation("Objeto {TableName} guardado exitosamente con operación: {Operation}",
                typeof(T).Name, entity.stateEntity);

            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error guardando objeto {TableName} con operación: {Operation}",
                typeof(T).Name, entity?.stateEntity);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<List<T>> SimpleSelectAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(predicate);

        using var activity = ActivitySource.StartActivity($"SQLServer.SimpleSelect.{typeof(T).Name}");
        activity?.SetTag("db.system", "sqlserver");
        activity?.SetTag("db.operation", "select");
        activity?.SetTag("db.table", typeof(T).Name);

        try
        {
            _logger?.LogInformation("Ejecutando SimpleSelect en tabla: {TableName}", typeof(T).Name);

            var result = await _dbContext.Set<T>()
                .Where(predicate)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            activity?.SetTag("db.rows_affected", result.Count);
            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger?.LogInformation("SimpleSelect en {TableName} retornó {RowCount} registros",
                typeof(T).Name, result.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error en SimpleSelect para tabla: {TableName}", typeof(T).Name);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<T?> GetByIdAsync<T>(params object[] keyValues) where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(keyValues);

        using var activity = ActivitySource.StartActivity($"SQLServer.GetById.{typeof(T).Name}");
        activity?.SetTag("db.system", "sqlserver");
        activity?.SetTag("db.operation", "select_by_id");
        activity?.SetTag("db.table", typeof(T).Name);

        try
        {
            _logger?.LogInformation("Obteniendo entidad {TableName} por ID", typeof(T).Name);

            var result = await _dbContext.Set<T>().FindAsync(keyValues).ConfigureAwait(false);

            activity?.SetTag("db.found", result is not null);
            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger?.LogInformation("GetById en {TableName} {Result}",
                typeof(T).Name, result is not null ? "encontró la entidad" : "no encontró la entidad");

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error en GetById para tabla: {TableName}", typeof(T).Name);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    private async Task CallProcedureADOAsync(string nameProcedure, params object[] param)
    {
        using var activity = ActivitySource.StartActivity($"SQLServer.CallProcedureADO.{nameProcedure}");
        activity?.SetTag("db.system", "sqlserver");
        activity?.SetTag("db.operation", "call_procedure_ado");
        activity?.SetTag("db.name", nameProcedure);

        var sqlLog = string.Empty;
        try
        {
            _logger?.LogInformation("Ejecutando procedimiento ADO: {ProcedureName}", nameProcedure);

            if (_dbContext.Database.GetDbConnection().State != ConnectionState.Open)
            {
                await _dbContext.Database.OpenConnectionAsync().ConfigureAwait(false);
            }

            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                _transaction ??= await _dbContext.Database
                    .BeginTransactionAsync(IsolationLevel.ReadCommitted)
                    .ConfigureAwait(false);

                var commandText = $"exec {nameProcedure} ";
                List<SqlParameter> parameters = [];
                Dictionary<object, SqlParameter> parametersOutput = [];

                commandText += ProcessParameters([.. param], parameters, parametersOutput);
                sqlLog = commandText[..^1];

                activity?.SetTag("db.statement", sqlLog);
                await _dbContext.Database.ExecuteSqlRawAsync(commandText, [.. parameters]).ConfigureAwait(false);

                foreach (var (key, value) in parametersOutput)
                {
                    key.GetType().GetProperty("Value")?.SetValue(key, value.Value == DBNull.Value ? null : value.Value);
                }
            }
            finally
            {
                _semaphore.Release();
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            _logger?.LogInformation("Procedimiento ADO {ProcedureName} ejecutado exitosamente", nameProcedure);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error en procedimiento ADO: {ProcedureName} - {SqlLog}", nameProcedure, sqlLog);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw new InvalidOperationException($"Error en la ejecución del procedimiento almacenado: {nameProcedure}( {sqlLog} )", ex);
        }
    }

    private async Task<List<T>> GetListByProcedureADOAsync<T>(string nameProcedure, params object[] param) where T : class, new()
    {
        using var activity = ActivitySource.StartActivity($"SQLServer.GetListByProcedureADO.{nameProcedure}");
        activity?.SetTag("db.system", "sqlserver");
        activity?.SetTag("db.operation", "select_procedure_ado");
        activity?.SetTag("db.name", nameProcedure);

        List<T> resultList = [];
        var sqlLog = string.Empty;

        try
        {
            _logger?.LogInformation("Obteniendo lista por procedimiento ADO: {ProcedureName}", nameProcedure);

            using var sqlCommand = new SqlCommand
            {
                Connection = (SqlConnection)_dbContext.Database.GetDbConnection(),
                CommandText = $"exec {nameProcedure} "
            };

            List<SqlParameter> parameters = [];
            Dictionary<object, SqlParameter> parametersOutput = [];

            sqlCommand.CommandText += ProcessParameters([.. param], parameters, parametersOutput);
            sqlCommand.Parameters.AddRange([.. parameters]);
            sqlLog = sqlCommand.CommandText[..^1];

            activity?.SetTag("db.statement", sqlLog);

            using var dataTable = new DataTable();
            using var adapter = new SqlDataAdapter(sqlCommand);

            dataTable.BeginLoadData();
            await Task.Run(() => adapter.Fill(dataTable)).ConfigureAwait(false);
            dataTable.EndLoadData();

            // Use parallel processing for large datasets (.NET 10 optimization)
            if (dataTable.Rows.Count > 100)
            {
                var rows = dataTable.Rows.Cast<DataRow>().ToArray();
                resultList = new List<T>(rows.Length);

                var partitioner = Partitioner.Create(0, rows.Length);
                var localLists = new ConcurrentBag<List<T>>();

                Parallel.ForEach(partitioner, range =>
                {
                    var localList = new List<T>(range.Item2 - range.Item1);
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        localList.Add(MapDataRowToObject<T>(rows[i]));
                    }
                    localLists.Add(localList);
                });

                foreach (var list in localLists)
                {
                    resultList.AddRange(list);
                }
            }
            else
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    resultList.Add(MapDataRowToObject<T>(row));
                }
            }

            DisposeConnection();

            foreach (var (key, value) in parametersOutput)
            {
                key.GetType().GetProperty("Value")?.SetValue(key, value.Value == DBNull.Value ? null : value.Value);
            }

            activity?.SetTag("db.rows_affected", resultList.Count);
            activity?.SetStatus(ActivityStatusCode.Ok);

            _logger?.LogInformation("Procedimiento ADO {ProcedureName} retornó {RowCount} registros",
                nameProcedure, resultList.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error en procedimiento ADO: {ProcedureName} - {SqlLog}", nameProcedure, sqlLog);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw new InvalidOperationException($"Error en la ejecución del procedimiento almacenado: {nameProcedure}( {sqlLog} )", ex);
        }

        return resultList;
    }

    private void DisposeConnection()
    {
        try
        {
            if (sqlConnection?.State != ConnectionState.Closed)
            {
                sqlConnection?.Close();
            }
        }
        catch
        {
            throw;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private T MapDataRowToObject<T>(DataRow dr) where T : class, new()
    {
        var vEntity = new T();

        try
        {
            // Get cached property lookup dictionary (FrozenDictionary for .NET 10)
            var propertyLookup = _propertyLookupCache.GetOrAdd(typeof(T), type =>
            {
                var props = _propertyCache.GetOrAdd(type, t => t.GetProperties());
                return props.ToFrozenDictionary(p => p.Name, StringComparer.Ordinal);
            });

            // Pre-calculate column lookup
            var columnLookup = dr.Table.Columns.Cast<DataColumn>()
                .ToFrozenDictionary(c => c.ColumnName, c => c.Ordinal, StringComparer.Ordinal);

            foreach (var (propertyName, property) in propertyLookup)
            {
                var columnIndex = columnLookup.GetValueOrDefault(propertyName, -1);
                if (columnIndex == -1) continue;

                var value = dr[columnIndex];

                if (value is null or DBNull)
                {
                    property.SetValue(vEntity, null);
                    continue;
                }

                var propertyType = _underlyingTypeCache.GetOrAdd(
                    property.PropertyType,
                    pt => Nullable.GetUnderlyingType(pt) ?? pt);

                try
                {
                    // Use .NET 10 pattern matching with improved performance
                    var convertedValue = (propertyType.Name, value) switch
                    {
                        (nameof(Int32), _) => Convert.ToInt32(value),
                        (nameof(Int64), _) => Convert.ToInt64(value),
                        (nameof(Decimal), _) => Convert.ToDecimal(value),
                        (nameof(Single), _) => Convert.ToSingle(value),
                        (nameof(Double), _) => Convert.ToDouble(value),
                        (nameof(Boolean), _) => Convert.ToBoolean(value),
                        (nameof(DateTime), _) => Convert.ToDateTime(value),
                        (nameof(String), _) => value.ToString()!,
                        (nameof(Byte) + "[]", byte[] bytes) => bytes,
                        (nameof(StringBuilder), _) => new StringBuilder(value.ToString()),
                        (nameof(Guid), Guid guid) => guid,
                        (nameof(Guid), _) => Guid.Parse(value.ToString()!),
                        (nameof(Int16), _) => Convert.ToInt16(value),
                        (nameof(Byte), _) => Convert.ToByte(value),
                        (nameof(Char), _) => Convert.ToChar(value),
                        (nameof(TimeSpan), TimeSpan ts) => ts,
                        (nameof(TimeSpan), _) => TimeSpan.Parse(value.ToString()!),
                        (nameof(DateTimeOffset), DateTimeOffset dto) => dto,
                        (nameof(DateTimeOffset), _) => DateTimeOffset.Parse(value.ToString()!),
                        _ => Convert.ChangeType(value, propertyType)
                    };

                    property.SetValue(vEntity, convertedValue);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex,
                        "Error mapping property {PropertyName} of type {PropertyType} with value type {ValueType}",
                        property.Name, propertyType.Name, value.GetType().Name);
                    continue;
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error mapping DataRow to {TypeName}", typeof(T).Name);
            throw;
        }

        return vEntity;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static string ProcessParameters(
        List<object> parameters,
        SqlParameterList sqlParameters,
        SqlParameterDict sqlParametersOut)
    {
        var commandTextBuilder = new StringBuilder(parameters.Count * 20);

        for (int i = 0; i < parameters.Count; i++)
        {
            var parameter = parameters[i];
            var nameParameter = $"@{PREFIX_PARAMETER_NAME}{i}";
            commandTextBuilder.Append(nameParameter);

            var sqlParameter = new SqlParameter { ParameterName = nameParameter };

            if (parameter is null)
            {
                sqlParameter.Value = DBNull.Value;
                sqlParameters.Add(sqlParameter);
                commandTextBuilder.Append(", ");
                continue;
            }

            var paramType = parameter.GetType();

            if (Parameter<object>._typeMap.ContainsKey(paramType) && paramType != typeof(byte[]))
            {
                sqlParameter.Direction = ParameterDirection.Input;
                sqlParameter.DbType = Parameter<object>._typeMap[paramType];
                sqlParameter.Value = parameter;

                if (paramType == typeof(decimal))
                {
                    sqlParameter.Precision = 10;
                    sqlParameter.Scale = 2;
                }

                sqlParameters.Add(sqlParameter);
                commandTextBuilder.Append(", ");
                continue;
            }

            if (paramType.IsGenericType && paramType.GetGenericTypeDefinition() == typeof(Parameter<>))
            {
                var genericType = paramType.GetGenericArguments()[0];
                var getDirectionMethod = paramType.GetMethod("GetDbParameterDirection");
                var direction = getDirectionMethod!.Invoke(parameter, null);
                var valueProperty = paramType.GetProperty("Value");
                var sizeProperty = paramType.GetProperty("Size");
                var value = valueProperty!.GetValue(parameter);

                sqlParameter.Direction = (ParameterDirection)direction!;
                sqlParameter.DbType = Parameter<object>._typeMap[genericType];
                sqlParameter.Value = value;
                sqlParameter.Size = (int)sizeProperty!.GetValue(parameter)!;

                sqlParameters.Add(sqlParameter);
                sqlParametersOut.Add(parameter, sqlParameter);

                commandTextBuilder.Append(sqlParameter.Direction == ParameterDirection.Output ? " out," : ",");
                continue;
            }

            if (paramType.IsGenericType && paramType.GetGenericTypeDefinition() == typeof(List<>))
            {
                sqlParameter.SqlDbType = SqlDbType.Structured;
                sqlParameter.TypeName = paramType.GetGenericArguments()[0].Name;
                sqlParameter.Value = CustomListExtension.ToDataTable(
                    (IList)parameter,
                    paramType.GetGenericArguments()[0].UnderlyingSystemType);
                sqlParameters.Add(sqlParameter);
                commandTextBuilder.Append(" ,");
                continue;
            }

            throw new ArgumentException($"El parámetro de tipo {paramType.Name} no es válido");
        }

        return commandTextBuilder.ToString();
    }
}


