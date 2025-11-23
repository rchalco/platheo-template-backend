using VectorStinger.Infrastructure.DataAccess.Interface;
using VectorStinger.Infrastructure.DataAccess.Wrapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using VectorStinger.Foundation.Utilities.CrossUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VectorStinger.Infrastructure.DataAccess.Implement.SQLServer
{
    public class MSSQLRepository<TDbContext> : IRepository where TDbContext : DbContext
    {
        #region  variables        
        private const string PREFIX_PARAMETER_NAME = "parameter";
        private string _connectionString = string.Empty;
        private SqlConnection sqlConnection = null;
        private IDbContextTransaction _transaction = null;
        private TDbContext _dbContext;
        private readonly ILogger<MSSQLRepository<TDbContext>> _logger;
        private static readonly ActivitySource ActivitySource = new("MSSQLRepository");
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        #endregion

        public MSSQLRepository(TDbContext dbContext, string connectionString, ILogger<MSSQLRepository<TDbContext>> logger = null)
        {
            _dbContext = dbContext;
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task<bool> CallProcedureAsync<T>(string nameProcedure, params object[] parameters) where T : class, new()
        {
            using var activity = ActivitySource.StartActivity($"SQLServer.CallProcedure.{nameProcedure}");
            activity?.SetTag("db.system", "sqlserver");
            activity?.SetTag("db.operation", "call_procedure");
            activity?.SetTag("db.name", nameProcedure);
            activity?.SetTag("db.connection_string", _connectionString);

            try
            {
                _logger?.LogInformation("Ejecutando procedimiento almacenado: {ProcedureName} con {ParameterCount} parámetros", 
                    nameProcedure, parameters?.Length ?? 0);

                Type type = _dbContext.GetType();
                MethodInfo methodInfo = type.GetMethod(nameProcedure);
                if (methodInfo != null)
                {
                    activity?.SetTag("db.execution_type", "ef_core");
                    methodInfo.Invoke(_dbContext, parameters);
                }
                else
                {
                    activity?.SetTag("db.execution_type", "ado_net");
                    await CallProcedureADOAsync(nameProcedure, parameters);
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

                if (_transaction != null)
                {
                    await _semaphore.WaitAsync(cancellationToken);
                    try
                    {
                        await _transaction.CommitAsync(cancellationToken);
                        await _transaction.DisposeAsync();
                        _transaction = null;
                        await _dbContext.Database.CloseConnectionAsync();
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

                List<T> resul = await _dbContext.Set<T>().ToListAsync(cancellationToken);

                activity?.SetTag("db.rows_affected", resul.Count);
                activity?.SetStatus(ActivityStatusCode.Ok);
                
                _logger?.LogInformation("GetAll de {TableName} retornó {RowCount} registros", 
                    typeof(T).Name, resul.Count);

                return resul;
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
            using var activity = ActivitySource.StartActivity($"SQLServer.GetDataByProcedure.{nameProcedure}");
            activity?.SetTag("db.system", "sqlserver");
            activity?.SetTag("db.operation", "select_procedure");
            activity?.SetTag("db.name", nameProcedure);

            try
            {
                _logger?.LogInformation("Obteniendo datos por procedimiento: {ProcedureName} con {ParameterCount} parámetros", 
                    nameProcedure, parameters?.Length ?? 0);

                List<T> lResul = new List<T>();
                Type type = _dbContext.GetType();
                object obj = Activator.CreateInstance(type);
                MethodInfo methodInfo = type.GetMethod(nameProcedure);
                
                if (methodInfo == null)
                {
                    activity?.SetTag("db.execution_type", "ado_net");
                    lResul = await GetListByProcedureADOAsync<T>(nameProcedure, parameters);
                }
                else
                {
                    activity?.SetTag("db.execution_type", "ef_core");
                    object resul = methodInfo.Invoke(obj, parameters);
                    methodInfo = typeof(Enumerable).GetMethod("ToList");
                    MethodInfo genericMethod = methodInfo.MakeGenericMethod(typeof(T));
                    object[] args = { resul };
                    lResul = genericMethod.Invoke(resul, args) as List<T>;
                }

                activity?.SetTag("db.rows_affected", lResul.Count);
                activity?.SetStatus(ActivityStatusCode.Ok);
                
                _logger?.LogInformation("Procedimiento {ProcedureName} retornó {RowCount} registros", 
                    nameProcedure, lResul.Count);

                return lResul;
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

                if (_transaction != null)
                {
                    await _semaphore.WaitAsync(cancellationToken);
                    try
                    {
                        await _transaction.RollbackAsync(cancellationToken);
                        await _transaction.DisposeAsync();
                        _transaction = null;
                        await _dbContext.Database.CloseConnectionAsync();
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
            using var activity = ActivitySource.StartActivity($"SQLServer.SaveObject.{typeof(T).Name}");
            activity?.SetTag("db.system", "sqlserver");
            activity?.SetTag("db.table", typeof(T).Name);

            try
            {
                if (entity == null)
                {
                    throw new Exception("La entidad ingresada para el registro no puede se nula");
                }
                else if (entity.stateEntity == StateEntity.none)
                {
                    throw new ArgumentException("no se definio un estado para la entidad");
                }
                else if (entity.EntityDB == null)
                {
                    throw new ArgumentException("no se tiene una entidad valida!, entidad interna nula");
                }

                activity?.SetTag("db.operation", entity.stateEntity.ToString());
                _logger?.LogInformation("Guardando objeto {TableName} con operación: {Operation}", 
                    typeof(T).Name, entity.stateEntity);

                StateEntity _operation = entity.stateEntity;
                
                await _semaphore.WaitAsync(cancellationToken);
                try
                {
                    if (_transaction == null)
                    {
                        _transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
                        activity?.SetTag("db.transaction_started", "true");
                    }

                    switch (_operation)
                    {
                        case StateEntity.add:
                            _dbContext.Add(entity.EntityDB);
                            break;

                        case StateEntity.modify:
                            _dbContext.Update(entity.EntityDB);
                            break;

                        case StateEntity.remove:
                            _dbContext.Remove(entity.EntityDB);
                            break;

                        default:
                            break;
                    }
                    var changes = await _dbContext.SaveChangesAsync(cancellationToken);
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
            using var activity = ActivitySource.StartActivity($"SQLServer.SimpleSelect.{typeof(T).Name}");
            activity?.SetTag("db.system", "sqlserver");
            activity?.SetTag("db.operation", "select");
            activity?.SetTag("db.table", typeof(T).Name);

            try
            {
                _logger?.LogInformation("Ejecutando SimpleSelect en tabla: {TableName}", typeof(T).Name);

                var result = await _dbContext.Set<T>().Where(predicate).AsNoTracking().ToListAsync(cancellationToken);

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
            using var activity = ActivitySource.StartActivity($"SQLServer.GetById.{typeof(T).Name}");
            activity?.SetTag("db.system", "sqlserver");
            activity?.SetTag("db.operation", "select_by_id");
            activity?.SetTag("db.table", typeof(T).Name);

            try
            {
                _logger?.LogInformation("Obteniendo entidad {TableName} por ID", typeof(T).Name);

                var result = await _dbContext.Set<T>().FindAsync(keyValues);

                activity?.SetTag("db.found", result != null);
                activity?.SetStatus(ActivityStatusCode.Ok);
                
                _logger?.LogInformation("GetById en {TableName} {Result}", 
                    typeof(T).Name, result != null ? "encontró la entidad" : "no encontró la entidad");

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error en GetById para tabla: {TableName}", typeof(T).Name);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw;
            }
        }

        private object[] GetKeysFromEntity<T>(T entity)
        {
            List<object> resul = new List<object>();
            _dbContext.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties
                .Select(x => x.Name).ToList().ForEach(keyName =>
                {
                    resul.Add(entity.GetType().GetProperty(keyName).GetValue(entity, null));
                });

            return resul.ToArray();
        }

        private async Task CallProcedureADOAsync(string nameProcedure, params object[] param)
        {
            using var activity = ActivitySource.StartActivity($"SQLServer.CallProcedureADO.{nameProcedure}");
            activity?.SetTag("db.system", "sqlserver");
            activity?.SetTag("db.operation", "call_procedure_ado");
            activity?.SetTag("db.name", nameProcedure);

            string sqlLog = string.Empty;
            try
            {
                _logger?.LogInformation("Ejecutando procedimiento ADO: {ProcedureName}", nameProcedure);

                if (_dbContext.Database.GetDbConnection().State != ConnectionState.Open)
                {
                    await _dbContext.Database.OpenConnectionAsync();
                }

                await _semaphore.WaitAsync();
                try
                {
                    if (_transaction == null)
                    {
                        _transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
                    }

                    string commandText = "exec " + nameProcedure + " ";
                    List<SqlParameter> parameters = new List<SqlParameter>();
                    Dictionary<object, SqlParameter> parametersOutput = new Dictionary<object, SqlParameter>();
                    commandText += ProcessParameters(param.ToList(), parameters, parametersOutput);
                    sqlLog = commandText = commandText.Substring(0, commandText.Length - 1);
                    
                    activity?.SetTag("db.statement", sqlLog);
                    await _dbContext.Database.ExecuteSqlRawAsync(commandText, parameters.ToArray());

                    foreach (var item in parametersOutput)
                    {
                        item.Key.GetType().GetProperty("Value").SetValue(item.Key, item.Value.Value == DBNull.Value ? null : item.Value.Value);
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
                Exception excepcion = new Exception("Error en la ejecucion del procedimiento almacenado: " + nameProcedure + "( " + sqlLog + " )", ex);
                throw excepcion;
            }
        }

        private async Task<List<T>> GetListByProcedureADOAsync<T>(string nameProcedure, params object[] param) where T : class, new()
        {
            using var activity = ActivitySource.StartActivity($"SQLServer.GetListByProcedureADO.{nameProcedure}");
            activity?.SetTag("db.system", "sqlserver");
            activity?.SetTag("db.operation", "select_procedure_ado");
            activity?.SetTag("db.name", nameProcedure);

            List<T> vListado = new List<T>();
            string sqlLog = string.Empty;
            try
            {
                _logger?.LogInformation("Obteniendo lista por procedimiento ADO: {ProcedureName}", nameProcedure);

                SqlCommand sqlCommand = new SqlCommand();
                sqlCommand.CommandText = nameProcedure;

                SqlConnection sqlConnection = (SqlConnection)_dbContext.Database.GetDbConnection();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = "exec " + nameProcedure + " ";
                List<SqlParameter> parameters = new List<SqlParameter>();
                Dictionary<object, SqlParameter> parametersOutput = new Dictionary<object, SqlParameter>();
                sqlCommand.CommandText += ProcessParameters(param.ToList(), parameters, parametersOutput);
                parameters.ForEach(parameter =>
                {
                    sqlCommand.Parameters.Add(parameter);
                });
                sqlLog = sqlCommand.CommandText = sqlCommand.CommandText.Substring(0, sqlCommand.CommandText.Length - 1);
                
                activity?.SetTag("db.statement", sqlLog);
                
                DataTable dtResul = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(sqlCommand);
                dtResul.BeginLoadData();
                await Task.Run(() => da.Fill(dtResul));
                dtResul.EndLoadData();

                foreach (DataRow item in dtResul.Rows)
                {
                    vListado.Add(MapDataRowToObject<T>(item));
                }

                DisposeConnection();
                foreach (var item in parametersOutput)
                {
                    item.Key.GetType().GetProperty("Value").SetValue(item.Key, item.Value.Value == DBNull.Value ? null : item.Value.Value);
                }

                activity?.SetTag("db.rows_affected", vListado.Count);
                activity?.SetStatus(ActivityStatusCode.Ok);
                
                _logger?.LogInformation("Procedimiento ADO {ProcedureName} retornó {RowCount} registros", 
                    nameProcedure, vListado.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error en procedimiento ADO: {ProcedureName} - {SqlLog}", nameProcedure, sqlLog);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                Exception excepcion = new Exception("Error en la ejecucion del procedimiento almacenado: " + nameProcedure + "( " + sqlLog + " )", ex);
                throw excepcion;
            }
            return vListado;
        }

        private void DisposeConnection()
        {
            try
            {
                if (sqlConnection != null)
                {
                    if (sqlConnection.State != ConnectionState.Closed)
                        sqlConnection.Close();
                }
            }
            catch
            {
                throw;
            }
        }

        private T MapDataRowToObject<T>(DataRow dr) where T : class, new()
        {
            T vEntity = new T();
            
            try
            {
                var properties = typeof(T).GetProperties();
                var tableColumns = dr.Table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToHashSet();

                foreach (PropertyInfo property in properties)
                {
                    if (!tableColumns.Contains(property.Name))
                    {
                        continue;
                    }

                    var value = dr[property.Name];
                    
                    if (value == null || value == DBNull.Value)
                    {
                        property.SetValue(vEntity, null);
                        continue;
                    }

                    // Use direct type checking instead of string matching for better performance
                    var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    
                    if (propertyType == typeof(int))
                    {
                        property.SetValue(vEntity, Convert.ToInt32(value));
                    }
                    else if (propertyType == typeof(long))
                    {
                        property.SetValue(vEntity, Convert.ToInt64(value));
                    }
                    else if (propertyType == typeof(decimal))
                    {
                        property.SetValue(vEntity, Convert.ToDecimal(value));
                    }
                    else if (propertyType == typeof(float))
                    {
                        property.SetValue(vEntity, Convert.ToSingle(value));
                    }
                    else if (propertyType == typeof(double))
                    {
                        property.SetValue(vEntity, Convert.ToDouble(value));
                    }
                    else if (propertyType == typeof(StringBuilder))
                    {
                        property.SetValue(vEntity, new StringBuilder(value.ToString()));
                    }
                    else if (propertyType == typeof(string))
                    {
                        property.SetValue(vEntity, value.ToString());
                    }
                    else if (propertyType == typeof(bool))
                    {
                        property.SetValue(vEntity, Convert.ToBoolean(value));
                    }
                    else if (propertyType == typeof(DateTime))
                    {
                        property.SetValue(vEntity, Convert.ToDateTime(value));
                    }
                    else if (propertyType == typeof(byte[]))
                    {
                        property.SetValue(vEntity, (byte[])value);
                    }
                    else
                    {
                        // For any other type, try direct assignment
                        property.SetValue(vEntity, value);
                    }
                }
            }
            catch
            {
                throw;
            }

            return vEntity;
        }

        private string ProcessParameters(List<object> parameters, List<SqlParameter> sqlParamters, Dictionary<object, SqlParameter> sqlParamtersOut)
        {
            string commandTextParameters = string.Empty;

            foreach (var (parameter, index) in parameters.Select((value, index) => (value, index)))
            {
                string nameParameter = "@" + PREFIX_PARAMETER_NAME + index.ToString();
                commandTextParameters += nameParameter;
                SqlParameter sqlParameter = new SqlParameter
                {
                    ParameterName = nameParameter
                };

                ///si el parametro es nulo
                if (parameter == null)
                {
                    sqlParameter.Value = DBNull.Value;
                    sqlParamters.Add(sqlParameter);
                    commandTextParameters += ", ";
                    continue;
                }

                ///si el parametro es un valor struct: string , double, etc
                if (Parameter<object>._typeMap.ContainsKey(parameter.GetType()) && parameter.GetType() != typeof(byte[]))
                {
                    sqlParameter.Direction = ParameterDirection.Input;
                    sqlParameter.DbType = Parameter<object>._typeMap[parameter.GetType()];
                    sqlParameter.Value = parameter;
                    if (parameter.GetType() == typeof(decimal))
                    {
                        sqlParameter.Precision = 10;
                        sqlParameter.Scale = 2;
                    }
                    sqlParamters.Add(sqlParameter);
                    commandTextParameters += ", ";
                    continue;
                }

                ///si el parametro es un parametro
                if (parameter.GetType().IsGenericType && parameter.GetType().GetGenericTypeDefinition() == typeof(Parameter<>))
                {
                    // Obtener el tipo real de T en Parameter<T>
                    Type parameterType = parameter.GetType().GetGenericArguments()[0];

                    // Usar Reflection para invocar el método GetDbParameterDirection() dinámicamente
                    MethodInfo getDirectionMethod = parameter.GetType().GetMethod("GetDbParameterDirection");
                    object direction = getDirectionMethod.Invoke(parameter, null);

                    // Obtener el valor de 'Value' usando Reflection
                    PropertyInfo valueProperty = parameter.GetType().GetProperty("Value");
                    PropertyInfo sizeProperty = parameter.GetType().GetProperty("Size");
                    object value = valueProperty.GetValue(parameter);

                    // Crear el parámetro SQL
                    sqlParameter.Direction = (ParameterDirection)direction;
                    sqlParameter.DbType = Parameter<object>._typeMap[parameterType];
                    sqlParameter.Value = value;
                    sqlParameter.Size = (int)sizeProperty.GetValue(parameter); ;

                    sqlParamters.Add(sqlParameter);
                    sqlParamtersOut.Add(parameter, sqlParameter);

                    commandTextParameters += sqlParameter.Direction == ParameterDirection.Output ? " out," : ",";
                    continue;
                }

                if (parameter.GetType().IsGenericType && parameter.GetType().GetGenericTypeDefinition() == typeof(List<>))
                {
                    sqlParameter.SqlDbType = SqlDbType.Structured;
                    sqlParameter.TypeName = parameter.GetType().GetGenericArguments()[0].Name;
                    sqlParameter.Value = CustomListExtension.ToDataTable(parameter as IList, parameter.GetType().GetGenericArguments()[0].UnderlyingSystemType);
                    sqlParamters.Add(sqlParameter);
                    commandTextParameters += " ,";
                    continue;
                }

                throw new ArgumentException("El parametro no es valido");
            }
            return commandTextParameters;
        }

    }
}


