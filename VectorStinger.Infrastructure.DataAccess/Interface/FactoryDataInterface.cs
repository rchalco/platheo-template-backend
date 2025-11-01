using VectorStinger.Infrastructure.DataAccess.Implement.MySQL;
using VectorStinger.Infrastructure.DataAccess.Implement.SQLServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace VectorStinger.Infrastructure.DataAccess.Interface
{
    public static class FactoryDataInterface<TDbContext> where TDbContext : DbContext
    {
        /// <summary>
        /// Crea una instancia del repositorio según el proveedor especificado
        /// </summary>
        /// <param name="provider">Tipo de proveedor de base de datos (MySql, SqlServer)</param>
        /// <param name="connectionString">Cadena de conexión a la base de datos</param>
        /// <param name="logger">Logger para telemetría y seguimiento</param>
        /// <returns>Instancia del repositorio configurado</returns>
        /// <exception cref="ArgumentException">Cuando el proveedor no es compatible</exception>
        /// <exception cref="InvalidOperationException">Cuando no se puede crear el contexto</exception>
        public static IRepository CreateRepository(string provider, string connectionString, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(provider))
                throw new ArgumentException("El proveedor no puede ser nulo o vacío", nameof(provider));
            
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("La cadena de conexión no puede ser nula o vacía", nameof(connectionString));

            logger?.LogInformation("Creando repositorio para proveedor: {Provider}", provider);

            try
            {
                var dbContext = CreateContext(provider, connectionString, logger);
                
                IRepository repository = provider.ToLowerInvariant() switch
                {
                    "mysql" => CreateMySqlRepository(dbContext, connectionString, logger),
                    "sqlserver" => CreateSqlServerRepository(dbContext, connectionString, logger),
                    _ => throw new ArgumentException($"Proveedor '{provider}' no compatible. Proveedores soportados: MySql, SqlServer", nameof(provider))
                };

                logger?.LogInformation("Repositorio {ProviderType} creado exitosamente", repository.GetType().Name);
                return repository;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error creando repositorio para proveedor: {Provider}", provider);
                throw;
            }
        }

        /// <summary>
        /// Crea el contexto de base de datos según el proveedor especificado
        /// </summary>
        /// <param name="provider">Tipo de proveedor de base de datos</param>
        /// <param name="connectionString">Cadena de conexión</param>
        /// <param name="logger">Logger para seguimiento</param>
        /// <returns>Contexto de base de datos configurado</returns>
        private static TDbContext CreateContext(string provider, string connectionString, ILogger logger)
        {
            logger?.LogDebug("Configurando contexto para proveedor: {Provider}", provider);

            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
                
                // Configurar el proveedor según el tipo
                ConfigureProvider(optionsBuilder, provider, connectionString, logger);
                
                // Configuraciones comunes para telemetría y rendimiento
                ConfigureCommonOptions(optionsBuilder, logger);

                var dbContext = (TDbContext)Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options);
                
                // Configuraciones específicas del contexto
                ConfigureContextSettings(dbContext, logger);

                logger?.LogDebug("Contexto {ContextType} configurado exitosamente", typeof(TDbContext).Name);
                return dbContext;
            }
            catch (Exception ex)
            {
                var contextException = new InvalidOperationException(
                    $"No se puede instanciar el contexto {typeof(TDbContext).Name}. " +
                    "Verifique que tenga un constructor que reciba DbContextOptions<T> como parámetro.", ex);
                
                logger?.LogError(contextException, "Error creando contexto para proveedor: {Provider}", provider);
                throw contextException;
            }
        }

        /// <summary>
        /// Configura el proveedor de base de datos específico
        /// </summary>
        private static void ConfigureProvider(DbContextOptionsBuilder<TDbContext> optionsBuilder, string provider, string connectionString, ILogger logger)
        {
            switch (provider.ToLowerInvariant())
            {
                case "mysql":
                    logger?.LogDebug("Configurando proveedor MySQL");
                    optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), options =>
                    {
                        options.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
                        options.CommandTimeout(7200);
                    });
                    break;

                case "sqlserver":
                    logger?.LogDebug("Configurando proveedor SQL Server");
                    optionsBuilder.UseSqlServer(connectionString, options =>
                    {
                        //options.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
                        options.CommandTimeout(7200);
                    });
                    break;

                default:
                    throw new ArgumentException($"Proveedor '{provider}' no soportado en CreateContext");
            }
        }

        /// <summary>
        /// Configura opciones comunes del contexto para telemetría y rendimiento
        /// </summary>
        private static void ConfigureCommonOptions(DbContextOptionsBuilder<TDbContext> optionsBuilder, ILogger logger)
        {
            logger?.LogDebug("Aplicando configuraciones comunes del contexto");

            // Configuraciones para Application Insights y telemetría
            optionsBuilder.EnableSensitiveDataLogging(false); // Por seguridad en producción
            optionsBuilder.EnableServiceProviderCaching(true);
            optionsBuilder.EnableDetailedErrors(true);
            
            // Configuración de logging para EF Core (solo en desarrollo)
#if DEBUG
            optionsBuilder.LogTo(message => logger?.LogDebug("[EF Core] {Message}", message))
                          .EnableSensitiveDataLogging(true);
#endif
        }

        /// <summary>
        /// Configura ajustes específicos del contexto después de la creación
        /// </summary>
        private static void ConfigureContextSettings(TDbContext dbContext, ILogger logger)
        {
            logger?.LogDebug("Configurando ajustes del contexto de base de datos");

            // Deshabilitar seguimiento automático de cambios para mejor rendimiento
            dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            
            // Configurar timeout de comando (2 horas)
            dbContext.Database.SetCommandTimeout(7200);
            
            // Configuraciones adicionales para rendimiento
            dbContext.ChangeTracker.LazyLoadingEnabled = false;
            dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            logger?.LogDebug("Contexto configurado con AutoDetectChanges: {AutoDetect}, CommandTimeout: {Timeout}s", 
                false, 7200);
        }

        /// <summary>
        /// Crea una instancia específica del repositorio MySQL
        /// </summary>
        private static IRepository CreateMySqlRepository(TDbContext dbContext, string connectionString, ILogger logger)
        {
            logger?.LogDebug("Creando repositorio MySQL");
            
            // Crear logger tipado si es posible
            var typedLogger = logger as ILogger<MySQLRepository<TDbContext>>;
            return new MySQLRepository<TDbContext>(dbContext, connectionString, typedLogger);
        }

        /// <summary>
        /// Crea una instancia específica del repositorio SQL Server
        /// </summary>
        private static IRepository CreateSqlServerRepository(TDbContext dbContext, string connectionString, ILogger logger)
        {
            logger?.LogDebug("Creando repositorio SQL Server");
            
            // Crear logger tipado si es posible
            var typedLogger = logger as ILogger<MSSQLRepository<TDbContext>>;
            return new MSSQLRepository<TDbContext>(dbContext, connectionString, typedLogger);
        }
    }
}
