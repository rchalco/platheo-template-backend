using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using VectorSinger.Modules.WebTemplate.Configuration;
using VectorStinger.Core.Configurations;
using VectorStinger.Core.Domain.DataBase.DataContext;
using VectorStinger.Core.Interfaces.Infrastructure.Bucket;
using VectorStinger.Core.Interfaces.Infrastructure.Oauth;
using VectorStinger.Foundation.Abstractions.Manager;
using VectorStinger.Foundation.Abstractions.UserCase;
using VectorStinger.Infrastructure.Bucket.Adapters;
using VectorStinger.Infrastructure.Bucket.Extensions;
using VectorStinger.Infrastructure.Bucket.Interfaces;
using VectorStinger.Infrastructure.Bucket.Services;
using VectorStinger.Infrastructure.DataAccess.Interface;
using VectorStinger.Infrastructure.DataAccess.Manager;
using VectorStinger.Infrastructure.OAuth.Implement;
using VectorStinger.Modules.Security.Configuration;

namespace VectorStinger.Application.Configurations
{
    public static class VectorStingerMain
    {
        public static IServiceCollection RegisterUserCases(
            this IServiceCollection services, 
            List<Type> serviceUserCase, 
            DatabaseSettings databaseSettings,
            IConfiguration configuration)
        {
            // Configuration of the repository
            services.AddScoped<IRepository>(serviceProvider =>
            {
                var repositoryLogger = serviceProvider.GetService<ILogger<IRepository>>();
                repositoryLogger?.LogDebug(
                    "Creando repositorio con proveedor {Provider} y cadena de conexión configurada", 
                    databaseSettings.Provider);

                return FactoryDataInterface<BdPlatheoTemplateContext>.CreateRepository(
                    databaseSettings.Provider,
                    databaseSettings.DefaultConnection,
                    repositoryLogger);
            });

            

            // Get assemblies once
            var assemblyApplication = typeof(VectorStingerMain).Assembly;
            var assemblySecurity = typeof(SecurityMain).Assembly;
            var assemblyWebTemplateMain = typeof(WebTemplateMain).Assembly;
            var assemblyKernel = typeof(VectorStingerCoreMain).Assembly;

            // Register OAuth provider
            services.AddTransient<IProviderAuthentication, FirebaseAuthentication>();

            // Register infrastructure components            
            services.AddTransient<HttpClient>();

            // Register bucket provider
            services.AddAwsS3Services(configuration);

            // Register managers from multiple assemblies
            RegisterManagers(services, assemblySecurity, assemblyKernel);
            RegisterManagers(services, assemblyWebTemplateMain, assemblyKernel);

            // Register use case components
            RegisterUseCaseComponents(services, assemblyApplication, serviceUserCase);

            return services;
        }

        private static void RegisterManagers(
            IServiceCollection services, 
            Assembly managerAssembly, 
            Assembly interfaceAssembly)
        {
            // Cache interface types for better performance
            var interfaceTypes = interfaceAssembly.GetTypes()
                .Where(t => t.IsInterface)
                .ToList();

            var managerTypes = managerAssembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(IManager).IsAssignableFrom(t))
                .ToList();

            foreach (var manager in managerTypes)
            {
                var managerInterface = interfaceTypes
                    .FirstOrDefault(t => t.IsAssignableFrom(manager) && t != typeof(IManager));

                if (managerInterface != null)
                {
                    services.AddTransient(managerInterface, manager);
                }
            }
        }

        private static void RegisterUseCaseComponents(
            IServiceCollection services, 
            Assembly assembly, 
            List<Type> serviceUserCase)
        {
            // Get all types once and cache them
            var allTypes = assembly.GetTypes();

            // Register User Case Input
            RegisterTypesByInterface<IUseCaseInput>(services, allTypes);

            // Register User Case Output
            RegisterTypesByInterface<IUseCaseOutput>(services, allTypes);

            // Register User Case Validation
            RegisterTypesByInterface<IUseCaseValidation>(services, allTypes);

            // Register User Cases
            var useCaseTypes = allTypes
                .Where(t => t.IsClass && !t.IsAbstract && typeof(IUserCase).IsAssignableFrom(t))
                .ToList();

            foreach (var userCase in useCaseTypes)
            {
                services.AddTransient(userCase);
                serviceUserCase.Add(userCase);
            }
        }

        private static void RegisterTypesByInterface<TInterface>(
            IServiceCollection services, 
            Type[] types)
        {
            var typesToRegister = types
                .Where(t => t.IsClass && !t.IsAbstract && typeof(TInterface).IsAssignableFrom(t))
                .ToList();

            foreach (var type in typesToRegister)
            {
                services.AddTransient(type);
            }
        }

        public static void RegisterFoldersConfiguration(
            this IServiceCollection services, 
            List<VectorStingerFolder> roomsyFolders)
        {
            const string ImagesFolderTarget = "Images";
            const string MissingFolderMessage = "Not found folder for images";

            if (roomsyFolders == null || roomsyFolders.Count == 0)
            {
                throw new ArgumentException(MissingFolderMessage);
            }

            var imagesFolder = roomsyFolders.FirstOrDefault(x => x.target == ImagesFolderTarget);
            
            if (imagesFolder == null)
            {
                throw new ArgumentException(MissingFolderMessage);
            }

            services.AddScoped<IImageManager, ImageManager>(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILogger<IImageManager>>();
                return new ImageManager(imagesFolder.path, logger);
            });
        }
    }
}
