using VectorStinger.Core.Configurations;
using VectorStinger.Core.Domain.DataBase.Data;
using VectorStinger.Core.Interfaces.Infrastructure.Oauth;
using VectorStinger.Infrastructure.DataAccess.Interface;
using VectorStinger.Infrastructure.DataAccess.Manager;
using VectorStinger.Infrastructure.OAuth.Implement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VectorStinger.Modules.Security.Configuration;
using VectorStinger.Foundation.Abstractions.Infrastructure;
using VectorStinger.Foundation.Abstractions.Manager;
using VectorStinger.Foundation.Abstractions.UserCase;
using System;
using System.Reflection;

namespace VectorStinger.Application.Configurations
{
    public static class VectorStingerMain
    {
        public static IServiceCollection RegisterUserCases(this IServiceCollection services, List<Type> serviceUserCase, DatabaseSettings databaseSettings)
        {
            ///Configuration of the repository
            services.AddScoped<IRepository>((serviceProvider) =>
            {
                // Obtener logger del contenedor de servicios si está disponible
                var repositoryLogger = serviceProvider.GetService<ILogger<IRepository>>();
                
                repositoryLogger?.LogDebug("Creando repositorio con proveedor {Provider} y cadena de conexión configurada", databaseSettings.Provider);
                
                return FactoryDataInterface<DbearthBnbContext>.CreateRepository(
                    databaseSettings.Provider, 
                    databaseSettings.DefaultConnection,
                    repositoryLogger);
            });

            Assembly assemblyApplication = typeof(VectorStingerMain).Assembly;
            Assembly assemblySecurity = typeof(SecurityMain).Assembly;
            Assembly assemblyKernel = typeof(VectorStingerCoreMain).Assembly;

            // register the configuration for oauth
            services.AddTransient<IProviderAuthentication, FirebaseAuthentication>();

            //Register infraestructure components
            services.AddTransient<HttpClient>();

            //Register of the Managers
            assemblySecurity.GetTypes()
                .Where(t => t.IsClass && typeof(IManager).IsAssignableFrom(t))
                .ToList().ForEach(manager =>
                {
                    var typeManagerAbastraction = assemblyKernel.GetTypes()
                                                        .Where(t => t.IsInterface && t.IsAssignableFrom(manager))
                                                        .FirstOrDefault();
                    if (typeManagerAbastraction != null)
                    {
                        services.AddTransient(typeManagerAbastraction, manager);
                    }
                });

            // Register User Case Input
            assemblyApplication.GetTypes()
                .Where(t => t.IsClass && typeof(IUseCaseInput).IsAssignableFrom(t))
                .ToList().ForEach(input =>
                {
                    services.AddTransient(input);
                });

            // Register User Case OutPut
            assemblyApplication.GetTypes()
                .Where(t => t.IsClass && typeof(IUseCaseOutput).IsAssignableFrom(t))
                .ToList().ForEach(output =>
                {
                    services.AddTransient(output);
                });

            // Register User Case Validation
            assemblyApplication.GetTypes()
                .Where(t => t.IsClass && typeof(IUserCaseValidation).IsAssignableFrom(t))
                .ToList().ForEach(validation =>
                {
                    services.AddTransient(validation);
                });

            // Register User Case - Con inyección de logger
            assemblyApplication.GetTypes()
                .Where(t => t.IsClass && typeof(IUserCase).IsAssignableFrom(t))
                .ToList().ForEach(userCase  =>
                {
                    services.AddTransient(userCase);
                    serviceUserCase.Add(userCase);
                });

            return services;
        }

        public static void RegisterFoldersConfiguration(this IServiceCollection services, List<VectorStingerFolder> roomsyFolders)
        {
            if (roomsyFolders == null || roomsyFolders.Count == 0 || !roomsyFolders.Any(x => x.target == "Images"))
            {
                throw new ArgumentException("No se tiene configurado la ruta de folder para las imagenes");
            }

            ///Configuration of the repository
            services.AddScoped<IImageManager, ImageManager>((serviceProvider) =>
            {
                string pathDefault = roomsyFolders.FirstOrDefault(x => x.target == "Images")?.path ??
                    throw new ArgumentException("No se tiene configurado la ruta de folder para las imagenes");
                var logger = serviceProvider.GetService<ILogger<IImageManager>>();
                return new ImageManager(pathDefault, logger);
            });
        }

    }
}
