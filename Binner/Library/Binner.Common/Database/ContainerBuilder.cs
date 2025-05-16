using Binner.Common.StorageProviders;
using Binner.LicensedProvider;
using Binner.Model;
using Binner.Model.Configuration;
using LightInject;
using LightInject.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Binner.Common.Database
{
    public class ContainerBuilder
    {
        private readonly WebHostServiceConfiguration _webHostServiceConfiguration;
        private readonly StorageProviderConfiguration _storageProviderConfiguration;

        public ContainerBuilder(WebHostServiceConfiguration webHostServiceConfiguration, StorageProviderConfiguration storageProviderConfiguration)
        {
            _webHostServiceConfiguration = webHostServiceConfiguration;
            _storageProviderConfiguration = storageProviderConfiguration;
        }
        /// <summary>
        /// Build a service container with database support
        /// </summary>
        /// <returns></returns>
        public IServiceContainer Build()
        {
            // inject configuration from environment variables (if set)
            EnvironmentVarConstants.SetConfigurationFromEnvironment(_storageProviderConfiguration);
            var containerOptions = new ContainerOptions
            {
                EnablePropertyInjection = false,
                DefaultServiceSelector = s => s.Last()
            };
            var container = new ServiceContainer(containerOptions);
            container.Register<IStorageProviderFactory, StorageProviderFactory>(new PerContainerLifetime());
            var services = new ServiceCollection();
            HostBuilderFactory.RegisterDbContext(services, _storageProviderConfiguration);

            var serviceProvider = container.CreateServiceProvider(services);
            var providerFactory = container.GetInstance<IStorageProviderFactory>();
            container.RegisterSingleton<IStorageProvider>((factory) =>
            {
                var storageProvider = providerFactory.CreateLimited(container, _storageProviderConfiguration);
                return storageProvider;
            });

            container.RegisterInstance(_webHostServiceConfiguration);
            container.RegisterInstance(new LicensedProvider.LicenseConfiguration { LicenseKey = _webHostServiceConfiguration.LicenseKey });
            return container;
        }
    }
}
