using AutoMapper.Internal;
using Binner.Common.Cache;
using Binner.Common.Integrations;
using Binner.Common.IO;
using Binner.Common.StorageProviders;
using Binner.Data;
using Binner.Global.Common;
using Binner.LicensedProvider;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.IO.Printing;
using Binner.Model.IO.Printing.PrinterHardware;
using Binner.Model.Responses;
using Binner.Services;
using Binner.Services.Authentication;
using Binner.Services.Integrations;
using Binner.Services.Integrations.Barcode;
using Binner.Services.Integrations.Categories;
using Binner.Services.Integrations.ExternalOrder;
using Binner.Services.Integrations.PartInformation;
using Binner.Services.IO;
using Binner.Services.IO.Printing;
using Binner.Services.MappingProfiles.ModelCommon;
using Binner.Services.Printing;
using Binner.StorageProvider.EntityFrameworkCore;
using Binner.Web.Authorization;
using Binner.Web.Database;
using Binner.Web.ServiceHost;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using DataModel = Binner.Data.Model;

namespace Binner.Web.Configuration
{
    public partial class StartupConfiguration
    {
        public static void ConfigureIoC(IServiceCollection services)
        {
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1#service-lifetimes
            // Transient = created each time requested, Scoped = once per http/scope request, Singleton = first time requested only
            // allow the container to be injected
            services.AddSingleton<IAuthorizationHandler, KiCadTokenAuthorizationHandler>();

            // register database factory for contexts
            RegisterDbFactory(services);

            // register printer configuration
            RegisterPrinterService(services);

            // register Api integrations
            RegisterApiIntegrations(services);

            // register services
            RegisterServices(services);

            // register licensed services
            RegisterLicensedServices(services);

            // register storage provider factory
            services.AddSingleton<IStorageProviderFactory, StorageProviderFactory>();
            
            // register storage provider
            var sp = services.BuildServiceProvider(); // intermediate service provider to access config
            var storageProviderConfig = sp.GetRequiredService<StorageProviderConfiguration>();
            // inject configuration from environment variables (if set)
            EnvironmentVarConstants.SetConfigurationFromEnvironment(storageProviderConfig);
            // register db context
            HostBuilderFactory.RegisterDbContext(services, storageProviderConfig);
            services.AddSingleton<IStorageProvider>((factory) =>
            {
                var providerFactory = factory.GetRequiredService<IStorageProviderFactory>();
                var storageProvider = providerFactory.Create(factory, storageProviderConfig);
                return storageProvider;
            });

            // register the font manager
            services.AddTransient<FontManager>();

            // register an http client factory
            services.AddTransient<HttpClientFactory>();

            // request context
            services.AddTransient<IRequestContextAccessor, RequestContextAccessor>();

            // the main server app
            services.AddSingleton<BinnerWebHostService>();

            // configure mapping (do this last, it depends on all other services)
            RegisterMappingProfiles(services);
        }

        private static void RegisterDbFactory(IServiceCollection services)
        {
            services.AddTransient<IGenericDbContextFactory, GenericDbContextFactory<BinnerContext>>();
        }

        private static void RegisterMappingProfiles(IServiceCollection services)
        {
            // register automapper
            services.AddTransient<PartTypeMappingAction<DataModel.Part, PartResponse>>();
            services.AddTransient<PartTypeMappingAction<Part, Binner.Model.CommonPart>>();
            services.AddTransient<PartTypeMappingAction<Part, PartResponse>>();
            var config = new AutoMapper.MapperConfiguration(cfg =>
            {
                // see: https://github.com/AutoMapper/AutoMapper/issues/3988
                cfg.Internal().MethodMappingEnabled = false;
                // enable DI in AutoMapper mapping profiles, specifically the PartTypeMappingAction which requires IPartTypesCache, IRequestContextAccessor
                var sp = services.BuildServiceProvider();
                cfg.ConstructServicesUsing(t => sp.GetRequiredService(t));
                cfg.AddMaps(Assembly.Load("Binner.Services"));
            });
            
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            services.AddSingleton(mapper);
        }

        private static void RegisterPrinterService(IServiceCollection services)
        {
            services.AddTransient<IBarcodeGenerator, BarcodeGenerator>();
            services.AddTransient<ILabelGenerator, LabelGenerator>();
            services.AddTransient<ILabelPrinterHardware, DymoLabelPrinterHardware>();
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IPartTypesCache, PartTypesCache>();
            services.AddTransient<IPartService, PartService>();
            services.AddTransient<IPartTypeService, PartTypeService>();
            services.AddTransient<IProjectService, ProjectService>();
            services.AddTransient<IPcbService, PcbService>();
            services.AddTransient<ICredentialService, CredentialService>();
            services.AddTransient<ISettingsService, SettingsService>();
            services.AddTransient<IStoredFileService, StoredFileService>();
            services.AddTransient<IUserService<User>, UserService<User>>();
            services.AddTransient<IOrganizationService<Organization>, OrganizationService<Organization>>();
            services.AddTransient<IAuthenticationService, AuthenticationService>();
            services.AddTransient<IAccountService<Account>, AccountService<Account>>();
            services.AddTransient<IAdminService, AdminService>();
            services.AddTransient<IPrintService, PrintService>();
            services.AddTransient<IPartScanHistoryService, PartScanHistoryService>();
            services.AddTransient<IOrderImportHistoryService, OrderImportHistoryService>();
            services.AddTransient<IBackupProvider, BackupProvider>();
            services.AddTransient<JwtService>();
            services.AddTransient<IntegrationService>();
            services.AddTransient<IVersionManagementService, VersionManagementService>();
            services.AddTransient<IExternalOrderService, ExternalOrderService>();
            services.AddTransient<IExternalPartInfoService, ExternalPartInfoService>();
            services.AddTransient<IExternalBarcodeInfoService, ExternalBarcodeInfoService>();
            services.AddTransient<IExternalCategoriesService, ExternalCategoriesService>();
            services.AddTransient<IUserConfigurationService, UserConfigurationService>();
            services.AddTransient<IUserConfigurationCacheProvider, UserConfigurationCacheProvider>();
            services.AddTransient<IOrganizationConfigurationCacheProvider, OrganizationConfigurationCacheProvider>();

            // Binner only
            services.AddTransient<ConfigFileMigrator>();
        }

        private static void RegisterLicensedServices(IServiceCollection services)
        {
            /* Register the licensed services, provided by PostSharp */
            var sp = services.BuildServiceProvider(); // intermediate service provider to access config
            var configLicenseKey = sp.GetRequiredService<Binner.Model.Configuration.LicenseConfiguration>().LicenseKey;
            services.AddSingleton<Binner.LicensedProvider.LicenseConfiguration>(new LicensedProvider.LicenseConfiguration { LicenseKey = configLicenseKey });
            services.AddTransient<ILicensedService<User, BinnerContext>, LicensedService<User, BinnerContext>>();
            services.AddTransient<ILicensedStorageProvider, LicensedStorageProvider>();
        }

        private static void RegisterApiIntegrations(IServiceCollection services)
        {
            // register integration apis
            services.AddTransient<IApiHttpClientFactory, ApiHttpClientFactory>();
            services.AddTransient<IIntegrationApiFactory, IntegrationApiFactory>();
            services.AddTransient<IIntegrationCredentialsCacheProvider, IntegrationCredentialsCacheProvider>();
            services.AddTransient<IBaseIntegrationBehavior, BaseIntegrationBehavior>();

            // external order services
            services.AddTransient<IDigiKeyExternalOrderService, DigiKeyExternalOrderService>();
            services.AddTransient<IMouserExternalOrderService, MouserExternalOrderService>();
            services.AddTransient<IArrowExternalOrderService, ArrowExternalOrderService>();
            services.AddTransient<ITmeExternalOrderService, TmeExternalOrderService>();

            // external barcode info services
            services.AddTransient<IDigiKeyBarcodeInfoService, DigiKeyBarcodeInfoService>();

            // external categories services
            services.AddTransient<IDigiKeyExternalCategoriesService, DigiKeyExternalCategoriesService>();
        }
    }
}
