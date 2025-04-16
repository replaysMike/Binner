﻿using AutoMapper.Internal;
using Binner.Common.Integrations;
using Binner.Common.IO;
using Binner.Common.IO.Printing;
using Binner.Common.Services;
using Binner.Common.Services.Authentication;
using Binner.Common.StorageProviders;
using Binner.Global.Common;
using Binner.LicensedProvider;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.IO.Printing;
using Binner.Model.IO.Printing.PrinterHardware;
using Binner.StorageProvider.EntityFrameworkCore;
using Binner.Web.ServiceHost;
using LightInject;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Binner.Web.Configuration
{
    public partial class StartupConfiguration
    {
        public static void ConfigureIoC(IServiceContainer container, IServiceCollection services)
        {
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1#service-lifetimes
            // Transient = created each time requested, Scoped = once per http/scope request, Singleton = first time requested only
            // allow the container to be injected
            services.AddSingleton<IServiceContainer, ServiceContainer>();
            services.AddSingleton(container);
            container.RegisterInstance(container);

            // register printer configuration
            RegisterPrinterService(container);

            // register Api integrations
            RegisterApiIntegrations(container);

            // register services
            RegisterServices(container);

            // register licensed services
            RegisterLicensedServices(container);

            // configure mapping
            RegisterMappingProfiles(container);

            // register storage provider
            var storageProviderConfig = container.GetInstance<StorageProviderConfiguration>();
            
            // inject configuration from environment variables (if set)
            EnvironmentVarConstants.SetConfigurationFromEnvironment(storageProviderConfig);

            container.Register<IStorageProviderFactory, StorageProviderFactory>(new PerContainerLifetime());
            var providerFactory = container.GetInstance<IStorageProviderFactory>();

            // register db context
            HostBuilderFactory.RegisterDbContext(services, storageProviderConfig);

            container.RegisterSingleton<IStorageProvider>((factory) =>
            {
                var storageProvider = providerFactory.Create(container, storageProviderConfig.Provider, storageProviderConfig.ProviderConfiguration);
                return storageProvider;
            });

            // register the font manager
            container.Register<FontManager>(new PerContainerLifetime());

            // register an http client factory
            container.Register<HttpClientFactory>(new PerContainerLifetime());

            // request context
            container.Register<IRequestContextAccessor, RequestContextAccessor>(new PerContainerLifetime());

            // the main server app
            container.Register<BinnerWebHostService>(new PerContainerLifetime());
        }

        private static void RegisterMappingProfiles(IServiceContainer container)
        {
            var profile = new BinnerMappingProfile();
            AnyMapper.Mapper.Configure(config =>
            {
                config.AddProfile(profile);
            });
            // register automapper
            var config = new AutoMapper.MapperConfiguration(cfg =>
            {
                // see: https://github.com/AutoMapper/AutoMapper/issues/3988
                cfg.Internal().MethodMappingEnabled = false; 
                cfg.AddMaps(Assembly.Load("Binner.Common"));
            });
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            container.RegisterInstance(mapper);
        }

        private static void RegisterPrinterService(IServiceContainer container)
        {
            container.Register<IBarcodeGenerator, BarcodeGenerator>(new PerScopeLifetime());
            container.Register<ILabelGenerator, LabelGenerator>(new PerScopeLifetime());
            container.Register<ILabelPrinterHardware, DymoLabelPrinterHardware>(new PerScopeLifetime());
        }

        private static void RegisterServices(IServiceContainer container)
        {
            container.Register<IPartTypesCache, PartTypesCache>(new PerContainerLifetime());
            container.Register<IPartService, PartService>(new PerScopeLifetime());
            container.Register<IPartTypeService, PartTypeService>(new PerScopeLifetime());
            container.Register<IProjectService, ProjectService>(new PerScopeLifetime());
            container.Register<IPcbService, PcbService>(new PerScopeLifetime());
            container.Register<ICredentialService, CredentialService>(new PerScopeLifetime());
            container.Register<ISettingsService, SettingsService>(new PerScopeLifetime());
            container.Register<ISwarmService, SwarmService>(new PerScopeLifetime());
            container.Register<IStoredFileService, StoredFileService>(new PerScopeLifetime());
            container.Register<IUserService, UserService>(new PerScopeLifetime());
            container.Register<IAuthenticationService, AuthenticationService>(new PerScopeLifetime());
            container.Register<IAccountService, AccountService>(new PerScopeLifetime());
            container.Register<IAdminService, AdminService>(new PerScopeLifetime());
            container.Register<IPrintService, PrintService>(new PerScopeLifetime());
            container.Register<IBackupProvider, BackupProvider>(new PerScopeLifetime());
            container.Register<JwtService>(new PerScopeLifetime());
            container.Register<IntegrationService>(new PerScopeLifetime());
            container.Register<IVersionManagementService, VersionManagementService>(new PerScopeLifetime());
        }

        private static void RegisterLicensedServices(IServiceContainer container)
        {
            /* Register the licensed services, provided by PostSharp */
            var configLicenseKey = container.GetInstance<Binner.Model.Configuration.LicenseConfiguration>().LicenseKey;
            container.RegisterInstance<Binner.LicensedProvider.LicenseConfiguration>(new LicensedProvider.LicenseConfiguration { LicenseKey = configLicenseKey });
            container.Register<ILicensedService, LicensedService>(new PerScopeLifetime());
            container.Register<ILicensedStorageProvider, LicensedStorageProvider>(new PerScopeLifetime());
        }

        private static void RegisterApiIntegrations(IServiceContainer container)
        {
            // register integration apis
            container.Register<IApiHttpClientFactory, ApiHttpClientFactory>(new PerScopeLifetime());
            container.Register<IIntegrationApiFactory, IntegrationApiFactory>(new PerScopeLifetime());
            container.Register<IIntegrationCredentialsCacheProvider, IntegrationCredentialsCacheProvider>(new PerScopeLifetime());
        }
    }
}
