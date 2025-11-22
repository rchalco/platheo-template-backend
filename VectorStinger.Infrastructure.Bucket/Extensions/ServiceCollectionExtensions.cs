using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VectorStinger.Core.Interfaces.Infrastructure.Bucket;
using VectorStinger.Infrastructure.Bucket.Adapters;
using VectorStinger.Infrastructure.Bucket.Configuration;
using VectorStinger.Infrastructure.Bucket.Interfaces;
using VectorStinger.Infrastructure.Bucket.Services;

namespace VectorStinger.Infrastructure.Bucket.Extensions
{
    /// <summary>
    /// Extensiones para registrar servicios de S3 en el contenedor de DI
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registra los servicios de AWS S3
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuración de la aplicación</param>
        /// <returns>Service collection para encadenamiento</returns>
        public static IServiceCollection AddAwsS3Services(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Registrar configuración de AWS S3
            services.AddOptions<AwsS3Settings>()
                .Bind(configuration.GetSection("AWS"));

            // Registrar servicio S3
            services.AddSingleton<IS3Service, AwsS3Service>();

            // Registrar adaptador agnóstico IBucketService
            services.AddSingleton<IBucketService, S3BucketAdapter>();

            return services;
        }

        /// <summary>
        /// Registra los servicios de AWS S3 con configuración personalizada
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configureOptions">Action para configurar AWS S3 settings</param>
        /// <returns>Service collection para encadenamiento</returns>
        public static IServiceCollection AddAwsS3Services(
            this IServiceCollection services,
            Action<AwsS3Settings> configureOptions)
        {
            // Registrar configuración con action
            services.Configure(configureOptions);

            // Registrar servicio S3
            services.AddSingleton<IS3Service, AwsS3Service>();

            // Registrar adaptador agnóstico IBucketService
            services.AddSingleton<IBucketService, S3BucketAdapter>();

            return services;
        }
    }
}
