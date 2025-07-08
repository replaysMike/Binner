using Binner.Common;
using Binner.Legacy.StorageProviders;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.IO.Printing;
using LightInject;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;

namespace Binner.Web.Configuration
{
    public partial class StartupConfiguration
    {
        private static readonly string _configFile = EnvironmentVarConstants.GetEnvOrDefault(EnvironmentVarConstants.Config, AppConstants.AppSettings);

        public static IConfigurationRoot Configure(IServiceContainer container, IServiceCollection services)
        {
            //var configPath = AppDomain.CurrentDomain.BaseDirectory;
            //var configPath = Environment.CurrentDirectory;
            var configPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName) ?? string.Empty;
            var configFile = Path.Combine(configPath, _configFile);
            Console.WriteLine($".Net Core bundle path: {AppContext.BaseDirectory}");
            Console.WriteLine($"Config file location: {configFile}");
            var configuration = Config.GetConfiguration(configFile);
            if (configuration == null) throw new InvalidOperationException($"Could not load configuration from {configFile}");
            var serviceConfiguration = configuration.GetSection(nameof(WebHostServiceConfiguration)).Get<WebHostServiceConfiguration>();
            if (serviceConfiguration == null) throw new InvalidOperationException($"Could not load WebHostServiceConfiguration from {configFile}, configuration file may be invalid or lacking read permissions!");
            var authenticationConfiguration = serviceConfiguration.Authentication;
            var storageProviderConfiguration = configuration.GetSection(nameof(StorageProviderConfiguration)).Get<StorageProviderConfiguration>();
            if (storageProviderConfiguration == null) throw new InvalidOperationException($"Could not load StorageProviderConfiguration from {configFile}, configuration file may be invalid or lacking read permissions!");
            // inject configuration from environment variables (if set)
            EnvironmentVarConstants.SetConfigurationFromEnvironment(storageProviderConfiguration);
            var binnerConfig = new BinnerFileStorageConfiguration(storageProviderConfiguration.ProviderConfiguration);

            // support IOptions<> MS dependency injection
            services.Configure<WebHostServiceConfiguration>(options => configuration.GetSection(nameof(WebHostServiceConfiguration)).Bind(options));
            services.Configure<StorageProviderConfiguration>(options => configuration.GetSection(nameof(StorageProviderConfiguration)).Bind(options));
            services.Configure<AuthenticationConfiguration>(options => configuration.GetSection(nameof(AuthenticationConfiguration)).Bind(options));

            // register traditional configuration with LightInject
            var licenseConfig = new LicenseConfiguration { LicenseKey = serviceConfiguration.LicenseKey };
            container.RegisterInstance(licenseConfig);
            services.AddSingleton(licenseConfig);
            container.RegisterInstance(configuration);
            services.AddSingleton(serviceConfiguration);
            container.RegisterInstance(serviceConfiguration);
            services.AddSingleton(authenticationConfiguration);
            container.RegisterInstance(authenticationConfiguration);
            services.AddSingleton(storageProviderConfiguration);
            container.RegisterInstance(storageProviderConfiguration);
            services.AddSingleton(binnerConfig);
            container.RegisterInstance(binnerConfig);
            services.AddSingleton(serviceConfiguration.Licensing);
            container.RegisterInstance(serviceConfiguration.Licensing);

            return configuration;
        }
    }
}
