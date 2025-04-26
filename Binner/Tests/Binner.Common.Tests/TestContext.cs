using AutoMapper;
using Binner.Common.Integrations;
using Binner.Common.Services;
using Binner.Data;
using Binner.Global.Common;
using Binner.LicensedProvider;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.StorageProvider.EntityFrameworkCore;
using Binner.Testing;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Octokit;
using System.Collections.Generic;
using System.Linq;

namespace Binner.Common.Tests
{
    public class TestContext
    {
        public WebHostServiceConfiguration WebHostServiceConfiguration { get; set; }
        public Mock<IDbContextFactory<BinnerContext>> DbFactory { get; set; }
        public IStorageProvider StorageProvider { get; set; }
        public Mock<IMapper> Mapper { get; set; }
        public Mock<IHttpContextAccessor> HttpContextAccessor { get; set; }
        public Mock<IRequestContextAccessor> RequestContextAccessor { get; set; }
        public Mock<IPartTypesCache> PartTypesCache { get; set; }
        public Mock<ILicensedService> LicensedService { get; set; }
        public Mock<IIntegrationCredentialsCacheProvider> IntegrationCredentialsCacheProvider { get; set; }
        public Mock<ICredentialService> CredentialService { get; set; }
        public Mock<ISwarmService> SwarmService { get; set; }
        public Mock<ILoggerFactory> LoggerFactory { get; set; }
        public IntegrationApiFactory IntegrationApiFactory => new IntegrationApiFactory(LoggerFactory.Object, IntegrationCredentialsCacheProvider.Object,
                HttpContextAccessor.Object, RequestContextAccessor.Object, CredentialService.Object,
                WebHostServiceConfiguration.Integrations, WebHostServiceConfiguration, ApiHttpClientFactory);
        public IApiHttpClientFactory ApiHttpClientFactory { get; set; }

        public TestContext()
        {
            WebHostServiceConfiguration = new WebHostServiceConfiguration();

            LoggerFactory = new Mock<ILoggerFactory>();
            DbFactory = new Mock<IDbContextFactory<BinnerContext>>();
            DbFactory.Setup(f => f.CreateDbContext())
                .Returns(new BinnerContext(new DbContextOptionsBuilder<BinnerContext>()
                    .UseInMemoryDatabase("InMemoryTest")
                    .Options));

            StorageProvider = new InMemoryStorageProvider();
            Mapper = new Mock<IMapper>();
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
            LicensedService = new Mock<ILicensedService>();

            IntegrationCredentialsCacheProvider = new Mock<IIntegrationCredentialsCacheProvider>();

            // configure the integration credentials cache provider
            var apiCredentials = CreateDefaultApiCredentials();
            ApplyApiCredentials(apiCredentials);

            CredentialService = new Mock<ICredentialService>();
            SwarmService = new Mock<ISwarmService>();

            ApiHttpClientFactory = new MockApiHttpClientFactory();
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
            WebHostServiceConfiguration.Integrations.Swarm.Enabled = swarmCreds.GetCredentialBool("Enabled");
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
            WebHostServiceConfiguration.Integrations.Tme.ResolveExternalLinks = tmeCreds.GetCredentialBool("ResolveExternalLinks");
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
    }
}
