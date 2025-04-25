using Binner.Common.Services;
using Binner.Testing;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Binner.Common.Tests.Services
{
    [TestFixture]
    public class PartServiceMouserTests
    {
        [Test]
        public async Task ShouldImportMouserOrderCAD()
        {
            var testContext = new TestContext();
            var apiCredentials = testContext.CreateApiCredentials(enableMouser: true); // enable mouser only
            testContext.ApplyApiCredentials(apiCredentials);

            testContext.ApiHttpClientFactory = new MockApiHttpClientFactory(new Dictionary<string, string>
            {
                { $"/api/v1/orderhistory/webOrderNumber", "Mouser-ExternalOrder-1-CAD.json" }
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
        public async Task ShouldImportMouserOrderUSD()
        {
            var testContext = new TestContext();
            var apiCredentials = testContext.CreateApiCredentials(enableMouser: true); // enable mouser only
            testContext.ApplyApiCredentials(apiCredentials);

            testContext.ApiHttpClientFactory = new MockApiHttpClientFactory(new Dictionary<string, string>
            {
                { $"/api/v1/orderhistory/webOrderNumber", "Mouser-ExternalOrder-1-USD.json" }
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
        public async Task ShouldImportMouserOrderEUR()
        {
            var testContext = new TestContext();
            var apiCredentials = testContext.CreateApiCredentials(enableMouser: true); // enable mouser only
            testContext.ApplyApiCredentials(apiCredentials);

            testContext.ApiHttpClientFactory = new MockApiHttpClientFactory(new Dictionary<string, string>
            {
                { $"/api/v1/orderhistory/webOrderNumber", "Mouser-ExternalOrder-1-EUR.json" }
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
