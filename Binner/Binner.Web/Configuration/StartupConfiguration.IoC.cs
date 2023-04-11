﻿using AutoMapper.Internal;
using Binner.Common;
using Binner.Common.Configuration;
using Binner.Common.Integrations;
using Binner.Common.IO.Printing;
using Binner.Common.Services;
using Binner.Common.StorageProviders;
using Binner.Web.ServiceHost;
using LightInject;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Binner.Model.Common;

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

            // configure mapping
            RegisterMappingProfiles(container);

            // register storage provider
            var storageProviderConfig = container.GetInstance<StorageProviderConfiguration>();
            container.Register<IStorageProviderFactory, StorageProviderFactory>(new PerContainerLifetime());
            var providerFactory = container.GetInstance<IStorageProviderFactory>();

            // register db context
            HostBuilderFactory.RegisterDbContext(services, storageProviderConfig);

            container.RegisterSingleton<IStorageProvider>((factory) =>
            {
                var storageProvider = providerFactory.Create(container, storageProviderConfig.ProviderConfiguration);
                return storageProvider;
            });

            // register the font manager
            container.Register<FontManager>(new PerContainerLifetime());

            // request context
            container.Register<RequestContextAccessor>(new PerContainerLifetime());

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
            container.Register<ILabelPrinterHardware>((serviceFactory) =>
            {
                var config = serviceFactory.GetInstance<WebHostServiceConfiguration>();
                var barcodeGenerator = serviceFactory.GetInstance<IBarcodeGenerator>();
                return new DymoLabelPrinterHardware(new PrinterSettings
                {
                    PrinterName = config.PrinterConfiguration.PrinterName,
                    PartLabelName = config.PrinterConfiguration.PartLabelName,
                    PartLabelSource = config.PrinterConfiguration.PartLabelSource,
                    PartLabelTemplate = config.PrinterConfiguration.PartLabelTemplate,
                    LabelDefinitions = config.PrinterConfiguration.LabelDefinitions
                }, barcodeGenerator);
            }, new PerScopeLifetime());
        }

        private static void RegisterServices(IServiceContainer container)
        {
            container.Register<IPartService, PartService>(new PerScopeLifetime());
            container.Register<IPartTypeService, PartTypeService>(new PerScopeLifetime());
            container.Register<IProjectService, ProjectService>(new PerScopeLifetime());
            container.Register<IPcbService, PcbService>(new PerScopeLifetime());
            container.Register<ICredentialService, CredentialService>(new PerScopeLifetime());
            container.Register<ISettingsService, SettingsService>(new PerScopeLifetime());
            container.Register<ISwarmService, SwarmService>(new PerScopeLifetime());
            container.Register<IStoredFileService, StoredFileService>(new PerScopeLifetime());
            container.Register<IntegrationService>(new PerScopeLifetime());
            container.Register<VersionManagementService>(new PerScopeLifetime());
        }

        private static void RegisterApiIntegrations(IServiceContainer container)
        {
            // register integration apis    
            container.Register<IIntegrationApiFactory, IntegrationApiFactory>(new PerScopeLifetime());
            container.Register<IIntegrationCredentialsCacheProvider, IntegrationCredentialsCacheProvider>(new PerScopeLifetime());
        }
    }
}
