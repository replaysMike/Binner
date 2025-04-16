using AutoMapper;
using Binner.Common.Integrations;
using Binner.Common.Services;
using Binner.Data;
using Binner.Global.Common;
using Binner.LicensedProvider;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.StorageProvider.EntityFrameworkCore;
using Binner.Testing;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;

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
        public IntegrationApiFactory IntegrationApiFactory => new IntegrationApiFactory(IntegrationCredentialsCacheProvider.Object,
                HttpContextAccessor.Object, RequestContextAccessor.Object, CredentialService.Object,
                WebHostServiceConfiguration.Integrations, WebHostServiceConfiguration, ApiHttpClientFactory);
        public IApiHttpClientFactory ApiHttpClientFactory { get; set; }

        public TestContext()
        {
            WebHostServiceConfiguration = new WebHostServiceConfiguration();
            
            DbFactory = new Mock<IDbContextFactory<BinnerContext>>();
            DbFactory.Setup(f => f.CreateDbContext())
                .Returns(new BinnerContext(new DbContextOptionsBuilder<BinnerContext>()
                    .UseInMemoryDatabase("InMemoryTest")
                    .Options));
            
            StorageProvider = new InMemoryStorageProvider();
            Mapper = new Mock<IMapper>();
            HttpContextAccessor = new Mock<IHttpContextAccessor>();
            
            RequestContextAccessor = new Mock<IRequestContextAccessor>();
            RequestContextAccessor.Setup(x => x.GetUserContext())
                .Returns(TestConstants.UserContext);
            
            PartTypesCache = new Mock<IPartTypesCache>();
            LicensedService = new Mock<ILicensedService>();

            IntegrationCredentialsCacheProvider = new Mock<IIntegrationCredentialsCacheProvider>();

            // configure the integration credentials cache provider
            var apiCredentials = CreateApiCredentials();
            IntegrationCredentialsCacheProvider
                .Setup(x => x.Cache)
                .Returns(new IntegrationCredentialsCache(
                    new Dictionary<ApiCredentialKey, List<ApiCredential>> {
                        { ApiCredentialKey.Create(1), apiCredentials }
                }));

            CredentialService = new Mock<ICredentialService>();
            SwarmService = new Mock<ISwarmService>();

            ApiHttpClientFactory = new MockApiHttpClientFactory();
        }

        public ILogger<T> MockLogger<T>()
        {
            return new Mock<ILogger<T>>().Object;
        }

        public List<ApiCredential> CreateApiCredentials()
        {
            var apiCredentials = new List<ApiCredential>
            {
                new ApiCredential(TestConstants.UserId, new Dictionary<string, object>()
                {
                    { "Enabled", true },
                    { "ApiKey", "test" },
                    { "ApiUrl", "https://swarm.binner.io" },
                    { "Timeout", "00:00:05" },
                }, nameof(SwarmApi)),
                new ApiCredential(TestConstants.UserId, new Dictionary<string, object>()
                {
                    { "Enabled", true },
                    { "Site", "test" },
                    { "ClientId", "test" },
                    { "ClientSecret", "test" },
                    { "oAuthPostbackUrl", "https://localhost:8090/Authorization/Authorize" },
                    { "ApiUrl", "https://api.digikey.com" },
                }, nameof(DigikeyApi)),
                new ApiCredential(TestConstants.UserId, new Dictionary<string, object>()
                {
                    { "Enabled", true },
                    { "CartApiKey", "test" },
                    { "OrderApiKey", "test" },
                    { "SearchApiKey", "test" },
                    { "ApiUrl", "https://api.mouser.com" },
                }, nameof(MouserApi)),
                new ApiCredential(TestConstants.UserId, new Dictionary<string, object>()
                {
                    { "Enabled", true },
                    { "ApiKey", "test" },
                    { "Username", "test" },
                    { "ApiUrl", "https://api.arrow.com" },
                }, nameof(ArrowApi)),
                new ApiCredential(TestConstants.UserId, new Dictionary<string, object>()
                {
                    { "Enabled", true },
                    { "Country", "us" },
                    { "ApiKey", "test" },
                    { "ApplicationSecret", "" },
                    { "ApiUrl", "https://api.tme.eu/" },
                }, nameof(TmeApi))
            };
            return apiCredentials;
        }
    }
}
