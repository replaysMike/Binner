using Binner.Common.Services;
using Binner.Testing;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Binner.Common.Tests.Services
{
    [TestFixture]
    public class PartServiceDigiKeyTests
    {
        [Test]
        public async Task ShouldImportDigiKeyOrderCAD()
        {
            var testContext = new TestContext();
            var apiCredentials = testContext.CreateApiCredentials(enableDigiKey: true); // enable digikey only
            testContext.ApplyApiCredentials(apiCredentials);

            testContext.ApiHttpClientFactory = new MockApiHttpClientFactory(new Dictionary<string, string>
            {
                { $"/OrderDetails/v3/Status/", "DigiKey-v3-ExternalOrder-1-CAD.json" }
            });

            var partService = new PartService(testContext.DbFactory.Object, testContext.WebHostServiceConfiguration, testContext.MockLogger<PartService>(), testContext.StorageProvider, testContext.Mapper.Object,
                    testContext.IntegrationApiFactory, testContext.SwarmService.Object, testContext.RequestContextAccessor.Object, testContext.PartTypesCache.Object, testContext.LicensedService.Object);
            
            var request = new Model.Requests.OrderImportRequest
            {   
                OrderId = TestConstants.OrderId,
                Username = TestConstants.UserName,
                Password = TestConstants.Password,
                Supplier = TestConstants.MouserSupplier,
                RequestProductInfo = false
            };
            var response = await partService.GetExternalOrderAsync(request);

            Assert.NotNull(response?.Response);
            Assert.That(response.Response.Amount, Is.EqualTo(6.70));
            Assert.That(response.Response.Currency, Is.EqualTo("CAD"));
            Assert.That(response.Response.Parts.Count, Is.EqualTo(1));
            Assert.That(response.Response.Parts.First().QuantityAvailable, Is.EqualTo(5));
        }

        [Test]
        public async Task ShouldImportDigiKeyOrderUSD()
        {
            var testContext = new TestContext();
            var apiCredentials = testContext.CreateApiCredentials(enableDigiKey: true); // enable digikey only
            testContext.ApplyApiCredentials(apiCredentials);

            testContext.ApiHttpClientFactory = new MockApiHttpClientFactory(new Dictionary<string, string>
            {
                { $"/OrderDetails/v3/Status/", "DigiKey-v3-ExternalOrder-1-USD.json" }
            });

            var partService = new PartService(testContext.DbFactory.Object, testContext.WebHostServiceConfiguration, testContext.MockLogger<PartService>(), testContext.StorageProvider, testContext.Mapper.Object,
                    testContext.IntegrationApiFactory, testContext.SwarmService.Object, testContext.RequestContextAccessor.Object, testContext.PartTypesCache.Object, testContext.LicensedService.Object);

            var request = new Model.Requests.OrderImportRequest
            {
                OrderId = TestConstants.OrderId,
                Username = TestConstants.UserName,
                Password = TestConstants.Password,
                Supplier = TestConstants.MouserSupplier,
                RequestProductInfo = false
            };
            var response = await partService.GetExternalOrderAsync(request);

            Assert.NotNull(response?.Response);
            Assert.That(response.Response.Amount, Is.EqualTo(6.70));
            Assert.That(response.Response.Currency, Is.EqualTo("USD"));
            Assert.That(response.Response.Parts.Count, Is.EqualTo(1));
            Assert.That(response.Response.Parts.First().QuantityAvailable, Is.EqualTo(5));
        }

        [Test]
        public async Task ShouldImportDigiKeyOrderEUR()
        {
            var testContext = new TestContext();
            var apiCredentials = testContext.CreateApiCredentials(enableDigiKey: true); // enable digikey only
            testContext.ApplyApiCredentials(apiCredentials);

            testContext.ApiHttpClientFactory = new MockApiHttpClientFactory(new Dictionary<string, string>
            {
                { $"/OrderDetails/v3/Status/", "DigiKey-v3-ExternalOrder-1-EUR.json" }
            });

            var partService = new PartService(testContext.DbFactory.Object, testContext.WebHostServiceConfiguration, testContext.MockLogger<PartService>(), testContext.StorageProvider, testContext.Mapper.Object,
                    testContext.IntegrationApiFactory, testContext.SwarmService.Object, testContext.RequestContextAccessor.Object, testContext.PartTypesCache.Object, testContext.LicensedService.Object);

            var request = new Model.Requests.OrderImportRequest
            {
                OrderId = TestConstants.OrderId,
                Username = TestConstants.UserName,
                Password = TestConstants.Password,
                Supplier = TestConstants.MouserSupplier,
                RequestProductInfo = false
            };
            var response = await partService.GetExternalOrderAsync(request);

            Assert.NotNull(response?.Response);
            Assert.That(response.Response.Amount, Is.EqualTo(6.70));
            Assert.That(response.Response.Currency, Is.EqualTo("EUR"));
            Assert.That(response.Response.Parts.Count, Is.EqualTo(1));
            Assert.That(response.Response.Parts.First().QuantityAvailable, Is.EqualTo(5));
        }
    }
}
