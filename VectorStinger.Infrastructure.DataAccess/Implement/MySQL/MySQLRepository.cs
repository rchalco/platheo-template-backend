using VectorStinger.Infrastructure.DataAccess.Interface;
using VectorStinger.Infrastructure.DataAccess.Wrapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace VectorStinger.Infrastructure.DataAccess.Implement.MySQL
{
    public class MySQLRepository<TDbContext> : IRepository
       where TDbContext : DbContext
    {
        MySQLDataInterface mysqlDataInterface = new MySQLDataInterface();
        TDbContext _dbContext;
        public string _connectionString;
        private readonly ILogger<MySQLRepository<TDbContext>> _logger;
        private static readonly ActivitySource ActivitySource = new("MySQLRepository");

        public MySQLRepository(TDbContext dbContext, string connectionString, ILogger<MySQLRepository<TDbContext>> logger = null)
        {
            _dbContext = dbContext;
            _connectionString = connectionString;
            _logger = logger;
            mysqlDataInterface.ConnectionString = _dbContext.Database.GetDbConnection().ConnectionString;            
        }

        public bool CallProcedure<T>(string nameProcedure, params object[] parameters) where T : class, new()
        {
            using var activity = ActivitySource.StartActivity($"MySQL.CallProcedure.{nameProcedure}");
            activity?.SetTag("db.operation", "call_procedure");
            activity?.SetTag("db.name", nameProcedure);
            
            try
            {
                _logger?.LogInformation("Ejecutando procedimiento almacenado: {ProcedureName} con {ParameterCount} parámetros", 
                    nameProcedure, parameters?.Length ?? 0);
                
                mysqlDataInterface.CallProcedure<T>(nameProcedure, parameters);
                
                activity?.SetStatus(ActivityStatusCode.Ok);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error ejecutando procedimiento almacenado: {ProcedureName}", nameProcedure);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw;
            }
        }

        public bool Commit()
        {
            using var activity = ActivitySource.StartActivity("MySQL.Commit");
            activity?.SetTag("db.operation", "commit");
            
            try
            {
                _logger?.LogInformation("Ejecutando Commit en MySQL");
                var changes = _dbContext.SaveChanges();
                
                activity?.SetTag("db.changes", changes);
                activity?.SetStatus(ActivityStatusCode.Ok);
                
                _logger?.LogInformation("Commit completado. {Changes} cambios guardados", changes);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error durante Commit en MySQL");
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw;
            }
        }

        public List<T> GetDataByProcedure<T>(string nameProcedure, params object[] parameters) where T : class, new()
        {
            using var activity = ActivitySource.StartActivity($"MySQL.GetDataByProcedure.{nameProcedure}");
            activity?.SetTag("db.operation", "select_procedure");
            activity?.SetTag("db.name", nameProcedure);
            
            try
            {
                _logger?.LogInformation("Obteniendo datos por procedimiento: {ProcedureName} con {ParameterCount} parámetros", 
                    nameProcedure, parameters?.Length ?? 0);
                
                var result = mysqlDataInterface.GetListByProcedure<T>(nameProcedure, parameters);
                
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

        public bool Rollback()
        {
            using var activity = ActivitySource.StartActivity("MySQL.Rollback");
            activity?.SetTag("db.operation", "rollback");
            
            try
            {
                _logger?.LogInformation("Ejecutando Rollback en MySQL");
                activity?.SetStatus(ActivityStatusCode.Ok);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error durante Rollback en MySQL");
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw;
            }
        }

        public List<T> SimpleSelect<T>(Expression<Func<T, bool>> predicate) where T : class, new()
        {
            using var activity = ActivitySource.StartActivity($"MySQL.SimpleSelect.{typeof(T).Name}");
            activity?.SetTag("db.operation", "select");
            activity?.SetTag("db.table", typeof(T).Name);
            
            try
            {
                _logger?.LogInformation("Ejecutando SimpleSelect en tabla: {TableName}", typeof(T).Name);
                
                var result = _dbContext.Set<T>().Where(predicate).ToList();
                
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

        public List<T> GetAll<T>() where T : class, new()
        {
            using var activity = ActivitySource.StartActivity($"MySQL.GetAll.{typeof(T).Name}");
            activity?.SetTag("db.operation", "select_all");
            activity?.SetTag("db.table", typeof(T).Name);
            
            try
            {
                _logger?.LogInformation("Obteniendo todos los registros de tabla: {TableName}", typeof(T).Name);
                
                var result = _dbContext.Set<T>().ToList();
                
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

        public bool SaveObject<T>(Entity<T> entity) where T : class, new()
        {
            using var activity = ActivitySource.StartActivity($"MySQL.SaveObject.{typeof(T).Name}");
            activity?.SetTag("db.table", typeof(T).Name);
            
            try
            {
                if (entity == null)
                {
                    throw new ArgumentException("el parametro a operar no puede ser nulo");
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

                if (entity.stateEntity == StateEntity.add)
                {
                    _dbContext.Add(entity.EntityDB);
                    var changes = _dbContext.SaveChanges();
                    activity?.SetTag("db.changes", changes);
                }
                else if (entity.stateEntity == StateEntity.modify)
                {
                    _dbContext.Update(entity.EntityDB);
                    var changes = _dbContext.SaveChanges();
                    activity?.SetTag("db.changes", changes);
                }
                else if (entity.stateEntity == StateEntity.remove)
                {
                    _dbContext.Remove(entity.EntityDB);
                    var changes = _dbContext.SaveChanges();
                    activity?.SetTag("db.changes", changes);
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
    }
}
