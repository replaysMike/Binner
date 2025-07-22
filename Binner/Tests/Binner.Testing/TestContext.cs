using AutoMapper;
using AutoMapper.Internal;
using Binner.Common.Integrations;
using Binner.Data;
using Binner.Global.Common;
using Binner.LicensedProvider;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations.DigiKey;
using Binner.Model.Requests;
using Binner.Services;
using Binner.Services.Integrations;
using Binner.Services.Integrations.ExternalOrder;
using Binner.Services.Integrations.PartInformation;
using Binner.StorageProvider.EntityFrameworkCore;
using Binner.Testing;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.ComponentModel;
using System.Reflection;

namespace Binner.Testing
{
    public class TestContext
    {
        public WebHostServiceConfiguration WebHostServiceConfiguration { get; set; }
        public StorageProviderConfiguration StorageProviderConfiguration { get; set; }
        public Mock<IDbContextFactory<BinnerContext>> DbFactory { get; set; }
        public IStorageProvider StorageProvider { get; set; }
        public IMapper Mapper { get; set; }
        public Mock<IHttpContextAccessor> HttpContextAccessor { get; set; }
        public Mock<IRequestContextAccessor> RequestContextAccessor { get; set; }
        public Mock<IPartTypesCache> PartTypesCache { get; set; }
        public Mock<ILicensedService<User, BinnerContext>> LicensedService { get; set; }
        public Mock<IIntegrationCredentialsCacheProvider> IntegrationCredentialsCacheProvider { get; set; }
        public Mock<ICredentialService> CredentialService { get; set; }
        public Mock<ILoggerFactory> LoggerFactory { get; set; }
        public Mock<ILogger> Logger { get; set; }
        public MockApiHttpClientFactory ApiHttpClientFactory { get; set; }
        public IExternalOrderService ExternalOrderService { get; set; }
        public Mock<IExternalBarcodeInfoService> ExternalBarcodeInfoService { get; set; }
        public IExternalPartInfoService ExternalPartInfoService { get; set; }
        public Mock<IExternalCategoriesService> ExternalCategoriesService { get; set; }
        public Mock<IUserConfigurationService> UserConfigurationService { get; set; }
        public Mock<IBaseIntegrationBehavior> BaseIntegrationBehavior { get; set; }
        public IDigiKeyExternalOrderService DigiKeyExternalOrderService { get; set; }
        public IMouserExternalOrderService MouserExternalOrderService { get; set; }
        public IArrowExternalOrderService ArrowExternalOrderService { get; set; }
        public ITmeExternalOrderService TmeExternalOrderService { get; set; }
        public IntegrationApiFactory IntegrationApiFactory => new IntegrationApiFactory(LoggerFactory.Object, Mapper, IntegrationCredentialsCacheProvider.Object,
        HttpContextAccessor.Object, RequestContextAccessor.Object, CredentialService.Object, ApiHttpClientFactory, UserConfigurationService.Object);


        private readonly OAuthCredential _digiKeyV3Credential = new OAuthCredential
        {
            AccessToken = "text-access",
            RefreshToken = "text-refresh",
            DateCreatedUtc = DateTime.UtcNow,
            DateExpiresUtc = DateTime.UtcNow.AddMinutes(30),
            DateModifiedUtc = DateTime.UtcNow,
            OAuthCredentialId = 1,
            Provider = nameof(DigikeyApi),
            ApiSettings = $@"{{""ApiVersion"": {(int)DigiKeyApiVersion.V3}}}",
            UserId = TestConstants.UserId,
        };
        private readonly OAuthCredential _digiKeyV4Credential = new OAuthCredential
        {
            AccessToken = "text-access",
            RefreshToken = "text-refresh",
            DateCreatedUtc = DateTime.UtcNow,
            DateExpiresUtc = DateTime.UtcNow.AddMinutes(30),
            DateModifiedUtc = DateTime.UtcNow,
            OAuthCredentialId = 1,
            Provider = nameof(DigikeyApi),
            ApiSettings = $@"{{""ApiVersion"": {(int)DigiKeyApiVersion.V4}}}",
            UserId = TestConstants.UserId,
        };

        private IMapper InitMapper()
        {
            var config = new AutoMapper.MapperConfiguration(cfg =>
            {
                // see: https://github.com/AutoMapper/AutoMapper/issues/3988
                cfg.Internal().MethodMappingEnabled = false;
                //cfg.ConstructServicesUsing(t => container.GetInstance(t));
                cfg.AddMaps(Assembly.Load("Binner.Services"));
            });

            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();
            return mapper;
        }

        public TestContext()
        {
            // init AutoMapper
            Mapper = InitMapper();
            WebHostServiceConfiguration = new WebHostServiceConfiguration();
            StorageProviderConfiguration = new StorageProviderConfiguration()
            {
                Provider = nameof(InMemoryStorageProvider),
                UserUploadedFilesPath = "/userfiles",
                ProviderConfiguration = new Dictionary<string, string> { }
            };

            LoggerFactory = new Mock<ILoggerFactory>();
            Logger = new Mock<ILogger>();
            LoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(MockLogger<It.IsAnyType>());
            
            DbFactory = new Mock<IDbContextFactory<BinnerContext>>();
            DbFactory.Setup(f => f.CreateDbContext())
                .Returns(new BinnerContext(new DbContextOptionsBuilder<BinnerContext>()
                    .UseInMemoryDatabase("InMemoryTest")
                    .Options));

            StorageProvider = new InMemoryStorageProvider();
            HttpContextAccessor = new Mock<IHttpContextAccessor>();
            var httpContextMock = new Mock<HttpContext>();
            var requestMock = new Mock<HttpRequest>();
            var headersMock = new Mock<IHeaderDictionary>();
            headersMock.Setup(x => x["Referer"])
                .Returns("https://tests.binner.io");
            httpContextMock.Setup(x => x.Request)
                .Returns(requestMock.Object);
            HttpContextAccessor.Setup(x => x.HttpContext)
                .Returns(httpContextMock.Object);

            RequestContextAccessor = new Mock<IRequestContextAccessor>();
            RequestContextAccessor.Setup(x => x.GetUserContext())
                .Returns(TestConstants.UserContext);

            PartTypesCache = new Mock<IPartTypesCache>();
            LicensedService = new Mock<ILicensedService<User, BinnerContext>>();
            ExternalBarcodeInfoService = new Mock<IExternalBarcodeInfoService>();
            ExternalCategoriesService = new Mock<IExternalCategoriesService>();
            UserConfigurationService = new Mock<IUserConfigurationService>();
            UserConfigurationService.Setup(x => x.GetCachedPrinterConfiguration(It.IsAny<int?>()))
                .Returns(new UserPrinterConfiguration());
            UserConfigurationService.Setup(x => x.GetCachedOrganizationConfiguration(It.IsAny<int?>()))
                .Returns(new OrganizationConfiguration());
            UserConfigurationService.Setup(x => x.GetCachedOrganizationIntegrationConfiguration(It.IsAny<int?>()))
                .Returns(new OrganizationIntegrationConfiguration());
            UserConfigurationService.Setup(x => x.GetCachedUserConfiguration(It.IsAny<int?>()))
                .Returns(new UserConfiguration());
            BaseIntegrationBehavior = new Mock<IBaseIntegrationBehavior>();
            IntegrationCredentialsCacheProvider = new Mock<IIntegrationCredentialsCacheProvider>();

            // configure the integration credentials cache provider
            var apiCredentials = CreateDefaultApiCredentials();
            ApplyApiCredentials(apiCredentials);

            CredentialService = new Mock<ICredentialService>();
            CredentialService.Setup(x => x.CreateOAuthRequestAsync(It.IsAny<OAuthAuthorization>()))
                .ReturnsAsync((OAuthAuthorization d) =>
                {
                    d.AuthorizationUrl = "https://localhost:8090/Authorization/Authorize?code=test&state=test";
                    d.AuthorizationReceived = true;
                    d.AccessToken = "test-access";
                    d.RefreshToken = "test-refresh";
                    return d;
                });
            SetDigiKeyApiVersion(DigiKeyApiVersion.V3);
            ApiHttpClientFactory = new MockApiHttpClientFactory();

            // concrete implementations
            ExternalPartInfoService = new ExternalPartInfoService(WebHostServiceConfiguration, StorageProvider, IntegrationApiFactory, RequestContextAccessor.Object, MockLogger<ExternalPartInfoService>(), ExternalBarcodeInfoService.Object, UserConfigurationService.Object);
            DigiKeyExternalOrderService = new DigiKeyExternalOrderService(WebHostServiceConfiguration, StorageProvider, IntegrationApiFactory, RequestContextAccessor.Object, UserConfigurationService.Object);
            MouserExternalOrderService = new MouserExternalOrderService(WebHostServiceConfiguration, StorageProvider, IntegrationApiFactory, RequestContextAccessor.Object, UserConfigurationService.Object);
            ArrowExternalOrderService = new ArrowExternalOrderService(WebHostServiceConfiguration, StorageProvider, IntegrationApiFactory, RequestContextAccessor.Object, UserConfigurationService.Object);
            TmeExternalOrderService = new TmeExternalOrderService(WebHostServiceConfiguration, StorageProvider, IntegrationApiFactory, RequestContextAccessor.Object);
            ExternalOrderService = new ExternalOrderService(DigiKeyExternalOrderService, MouserExternalOrderService, ArrowExternalOrderService, TmeExternalOrderService);
        }

        public ILogger<T> MockLogger<T>()
        {
            return new Mock<ILogger<T>>().Object;
        }

        public void ApplyApiCredentials(List<ApiCredential> apiCredentials)
        {
            IntegrationCredentialsCacheProvider
                .Setup(x => x.Cache)
                .Returns(new IntegrationCredentialsCache(
                    new Dictionary<ApiCredentialKey, List<ApiCredential>> {
                        { ApiCredentialKey.Create(TestConstants.UserId), apiCredentials }
                }));
            // setup config based on credentials configuration
            var swarmCreds = apiCredentials.Where(x => x.ApiName == nameof(SwarmApi)).First();
            var digikeyCreds = apiCredentials.Where(x => x.ApiName == nameof(DigikeyApi)).First();
            var mouserCreds = apiCredentials.Where(x => x.ApiName == nameof(MouserApi)).First();
            var octopartCreds = apiCredentials.Where(x => x.ApiName == nameof(NexarApi)).First();
            var arrowCreds = apiCredentials.Where(x => x.ApiName == nameof(ArrowApi)).First();
            var tmeCreds = apiCredentials.Where(x => x.ApiName == nameof(TmeApi)).First();
            // todo: move to user config service
            /*WebHostServiceConfiguration.Integrations.Swarm.Enabled = swarmCreds.GetCredentialBool("Enabled");
            WebHostServiceConfiguration.Integrations.Swarm.ApiKey = swarmCreds.GetCredentialString("ApiKey");
            WebHostServiceConfiguration.Integrations.Swarm.ApiUrl = swarmCreds.GetCredentialString("ApiUrl");

            WebHostServiceConfiguration.Integrations.Digikey.Enabled = digikeyCreds.GetCredentialBool("Enabled");
            WebHostServiceConfiguration.Integrations.Digikey.ClientId = digikeyCreds.GetCredentialString("ClientId");
            WebHostServiceConfiguration.Integrations.Digikey.ClientSecret = digikeyCreds.GetCredentialString("ClientSecret");
            WebHostServiceConfiguration.Integrations.Digikey.oAuthPostbackUrl = digikeyCreds.GetCredentialString("oAuthPostbackUrl");
            WebHostServiceConfiguration.Integrations.Digikey.ApiUrl = digikeyCreds.GetCredentialString("ApiUrl");
            WebHostServiceConfiguration.Integrations.Digikey.Site = (DigikeyLocaleSite)digikeyCreds.GetCredentialInt32("Site");

            WebHostServiceConfiguration.Integrations.Mouser.Enabled = mouserCreds.GetCredentialBool("Enabled");
            WebHostServiceConfiguration.Integrations.Mouser.ApiKeys.SearchApiKey = mouserCreds.GetCredentialString("SearchApiKey");
            WebHostServiceConfiguration.Integrations.Mouser.ApiKeys.OrderApiKey = mouserCreds.GetCredentialString("OrderApiKey");
            WebHostServiceConfiguration.Integrations.Mouser.ApiUrl = mouserCreds.GetCredentialString("ApiUrl");

            WebHostServiceConfiguration.Integrations.Nexar.Enabled = octopartCreds.GetCredentialBool("Enabled");
            WebHostServiceConfiguration.Integrations.Nexar.ClientId = octopartCreds.GetCredentialString("ClientId");
            WebHostServiceConfiguration.Integrations.Nexar.ClientSecret = octopartCreds.GetCredentialString("ClientSecret");

            WebHostServiceConfiguration.Integrations.Arrow.Enabled = arrowCreds.GetCredentialBool("Enabled");
            WebHostServiceConfiguration.Integrations.Arrow.ApiKey = arrowCreds.GetCredentialString("ApiKey");
            WebHostServiceConfiguration.Integrations.Arrow.Username = arrowCreds.GetCredentialString("Username");
            WebHostServiceConfiguration.Integrations.Arrow.ApiUrl = arrowCreds.GetCredentialString("ApiUrl");

            WebHostServiceConfiguration.Integrations.Tme.Enabled = tmeCreds.GetCredentialBool("Enabled");
            WebHostServiceConfiguration.Integrations.Tme.Country = tmeCreds.GetCredentialString("Country");
            WebHostServiceConfiguration.Integrations.Tme.ApiKey = tmeCreds.GetCredentialString("ApiKey");
            WebHostServiceConfiguration.Integrations.Tme.ApplicationSecret = tmeCreds.GetCredentialString("ApplicationSecret");
            WebHostServiceConfiguration.Integrations.Tme.ApiUrl = tmeCreds.GetCredentialString("ApiUrl");
            WebHostServiceConfiguration.Integrations.Tme.ResolveExternalLinks = tmeCreds.GetCredentialBool("ResolveExternalLinks");*/
        }

        public List<ApiCredential> CreateDefaultApiCredentials()
        {
            return CreateApiCredentials(enableSwarm: true, enableDigiKey: true, enableMouser: true, enableOctopart: true, enableArrow: true, enableTme: true);
        }

        public List<ApiCredential> CreateApiCredentials(bool enableSwarm = false, bool enableDigiKey = false, bool enableMouser = false, bool enableOctopart = false, bool enableArrow = false, bool enableTme = false)
        {
            var apiCredentials = new List<ApiCredential>
            {
                new ApiCredential(TestConstants.UserId, new Dictionary<string, object>()
                {
                    { "Enabled", enableSwarm },
                    { "ApiKey", "test" },
                    { "ApiUrl", "https://swarm.binner.io" },
                    { "Timeout", "00:00:05" },
                }, nameof(SwarmApi)),
                new ApiCredential(TestConstants.UserId, new Dictionary<string, object>()
                {
                    { "Enabled", enableDigiKey },
                    { "Site", (int)DigikeyLocaleSite.US },
                    { "ClientId", "test" },
                    { "ClientSecret", "test" },
                    { "oAuthPostbackUrl", "https://localhost:8090/Authorization/Authorize" },
                    { "ApiUrl", "https://api.digikey.com" },
                }, nameof(DigikeyApi)),
                new ApiCredential(TestConstants.UserId, new Dictionary<string, object>()
                {
                    { "Enabled", enableMouser },
                    { "CartApiKey", "test" },
                    { "OrderApiKey", "test" },
                    { "SearchApiKey", "test" },
                    { "ApiUrl", "https://api.mouser.com" },
                }, nameof(MouserApi)),
                new ApiCredential(TestConstants.UserId, new Dictionary<string, object>()
                {
                    { "Enabled", enableOctopart },
                    { "ClientId", "test" },
                    { "ClientSecret", "test" },
                }, nameof(NexarApi)),
                new ApiCredential(TestConstants.UserId, new Dictionary<string, object>()
                {
                    { "Enabled", enableArrow },
                    { "ApiKey", "test" },
                    { "Username", "test" },
                    { "ApiUrl", "https://api.arrow.com" },
                }, nameof(ArrowApi)),
                new ApiCredential(TestConstants.UserId, new Dictionary<string, object>()
                {
                    { "Enabled", enableTme },
                    { "Country", "us" },
                    { "ApiKey", "test" },
                    { "ApplicationSecret", "test" },
                    { "ApiUrl", "https://api.tme.eu/" },
                    { "ResolveExternalLinks", false }, // for tests, don't make extra http calls for external links
                }, nameof(TmeApi))
            };
            return apiCredentials;
        }

        public void SetDigiKeyApiVersion(DigiKeyApiVersion testVersion)
        {
            switch(testVersion)
            {
                case DigiKeyApiVersion.V3:
                    CredentialService.Setup(x => x.GetOAuthCredentialAsync(It.IsAny<string>()))
                        .ReturnsAsync((string providerName) =>
                        {
                            return _digiKeyV3Credential;
                        });
                    break;
                case DigiKeyApiVersion.V4:
                    CredentialService.Setup(x => x.GetOAuthCredentialAsync(It.IsAny<string>()))
                        .ReturnsAsync((string providerName) =>
                        {
                            return _digiKeyV4Credential;
                        });
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
