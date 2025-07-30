using Binner.Common.StorageProviders;
using Binner.Model;
using Binner.Model.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Binner.Web.Database
{
    public class ServiceProviderBuilder
    {
        private readonly WebHostServiceConfiguration _webHostServiceConfiguration;
        private readonly StorageProviderConfiguration _storageProviderConfiguration;

        public ServiceProviderBuilder(WebHostServiceConfiguration webHostServiceConfiguration, StorageProviderConfiguration storageProviderConfiguration)
        {
            _webHostServiceConfiguration = webHostServiceConfiguration;
            _storageProviderConfiguration = storageProviderConfiguration;
        }
        /// <summary>
        /// Build a service container with database support
        /// </summary>
        /// <returns></returns>
        public IServiceProvider Build()
        {
            var services = new ServiceCollection();
            // inject configuration from environment variables (if set)
            EnvironmentVarConstants.SetConfigurationFromEnvironment(_storageProviderConfiguration);

            services.AddTransient<IStorageProviderFactory, StorageProviderFactory>();
            HostBuilderFactory.RegisterDbContext(services, _storageProviderConfiguration);

            services.AddSingleton<IStorageProvider>((factory) =>
            {
                var providerFactory = new StorageProviderFactory();
                var storageProvider = providerFactory.CreateLimited(factory, _storageProviderConfiguration);
                return storageProvider;
            });

            services.AddSingleton(_webHostServiceConfiguration);
            services.AddSingleton(new LicensedProvider.LicenseConfiguration { LicenseKey = _webHostServiceConfiguration.Licensing?.LicenseKey });
            return services.BuildServiceProvider();
        }
    }
}
