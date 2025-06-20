using Binner.Model.Integrations.DigiKey;
using Binner.Services;
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
        public async Task ShouldImportDigiKeyV3OrderCAD()
        {
            var testContext = new TestContext();
            var apiCredentials = testContext.CreateApiCredentials(enableDigiKey: true); // enable digikey only
            testContext.ApplyApiCredentials(apiCredentials);
            testContext.SetDigiKeyApiVersion(DigiKeyApiVersion.V3);

            testContext.ApiHttpClientFactory = new MockApiHttpClientFactory(new Dictionary<string, string>
            {
                { $"/OrderDetails/v3/Status/", "DigiKey-v3-ExternalOrder-1-CAD.json" }
            });

            var partService = new PartService(testContext.WebHostServiceConfiguration, testContext.MockLogger<PartService>(), testContext.StorageProvider, testContext.Mapper.Object,
                    testContext.IntegrationApiFactory, testContext.RequestContextAccessor.Object, testContext.PartTypesCache.Object);
            
            var request = new Model.Requests.OrderImportRequest
            {   
                OrderId = TestConstants.OrderId,
                Username = TestConstants.UserName,
                Password = TestConstants.Password,
                Supplier = TestConstants.DigiKeySupplier,
                RequestProductInfo = false
            };
            var response = await partService.GetExternalOrderAsync(request);

            Assert.That(response?.Response, Is.Not.Null);
            Assert.That(response.Response.Currency, Is.EqualTo("CAD"));
            Assert.That(response.Response.Parts.Count, Is.EqualTo(44));
            var part = response.Response.Parts.First();
            Assert.That(part.QuantityAvailable, Is.EqualTo(500));
            Assert.That(part.Supplier, Is.EqualTo("DigiKey"));
            Assert.That(part.SupplierPartNumber, Is.EqualTo("1276-6455-1-ND"));
            Assert.That(part.Description, Is.EqualTo("CAP CER 10UF 16V X5R 0805"));
            Assert.That(part.Cost, Is.EqualTo(0.0264));
        }

        [Test]
        public async Task ShouldImportDigiKeyV3OrderUSD()
        {
            var testContext = new TestContext();
            var apiCredentials = testContext.CreateApiCredentials(enableDigiKey: true); // enable digikey only
            testContext.ApplyApiCredentials(apiCredentials);
            testContext.SetDigiKeyApiVersion(DigiKeyApiVersion.V3);

            testContext.ApiHttpClientFactory = new MockApiHttpClientFactory(new Dictionary<string, string>
            {
                { $"/OrderDetails/v3/Status/", "DigiKey-v3-ExternalOrder-1-USD.json" }
            });

            var partService = new PartService(testContext.WebHostServiceConfiguration, testContext.MockLogger<PartService>(), testContext.StorageProvider, testContext.Mapper.Object,
                    testContext.IntegrationApiFactory, testContext.RequestContextAccessor.Object, testContext.PartTypesCache.Object);

            var request = new Model.Requests.OrderImportRequest
            {
                OrderId = TestConstants.OrderId,
                Username = TestConstants.UserName,
                Password = TestConstants.Password,
                Supplier = TestConstants.DigiKeySupplier,
                RequestProductInfo = false
            };
            var response = await partService.GetExternalOrderAsync(request);

            Assert.That(response?.Response, Is.Not.Null);
            Assert.That(response.Response.Currency, Is.EqualTo("USD"));
            Assert.That(response.Response.Parts.Count, Is.EqualTo(44));
            var part = response.Response.Parts.First();
            Assert.That(part.QuantityAvailable, Is.EqualTo(500));
            Assert.That(part.Supplier, Is.EqualTo("DigiKey"));
            Assert.That(part.SupplierPartNumber, Is.EqualTo("1276-6455-1-ND"));
            Assert.That(part.Description, Is.EqualTo("CAP CER 10UF 16V X5R 0805"));
            Assert.That(part.Cost, Is.EqualTo(0.0264));
        }

        [Test]
        public async Task ShouldImportDigiKeyV3OrderEUR()
        {
            var testContext = new TestContext();
            var apiCredentials = testContext.CreateApiCredentials(enableDigiKey: true); // enable digikey only
            testContext.ApplyApiCredentials(apiCredentials);
            testContext.SetDigiKeyApiVersion(DigiKeyApiVersion.V3);

            testContext.ApiHttpClientFactory = new MockApiHttpClientFactory(new Dictionary<string, string>
            {
                { $"/OrderDetails/v3/Status/", "DigiKey-v3-ExternalOrder-1-EUR.json" }
            });

            var partService = new PartService(testContext.WebHostServiceConfiguration, testContext.MockLogger<PartService>(), testContext.StorageProvider, testContext.Mapper.Object,
                    testContext.IntegrationApiFactory, testContext.RequestContextAccessor.Object, testContext.PartTypesCache.Object);

            var request = new Model.Requests.OrderImportRequest
            {
                OrderId = TestConstants.OrderId,
                Username = TestConstants.UserName,
                Password = TestConstants.Password,
                Supplier = TestConstants.DigiKeySupplier,
                RequestProductInfo = false
            };
            var response = await partService.GetExternalOrderAsync(request);

            Assert.That(response?.Response, Is.Not.Null);
            Assert.That(response.Response.Currency, Is.EqualTo("EUR"));
            Assert.That(response.Response.Parts.Count, Is.EqualTo(44));
            var part = response.Response.Parts.First();
            Assert.That(part.QuantityAvailable, Is.EqualTo(500));
            Assert.That(part.Supplier, Is.EqualTo("DigiKey"));
            Assert.That(part.SupplierPartNumber, Is.EqualTo("1276-6455-1-ND"));
            Assert.That(part.Description, Is.EqualTo("CAP CER 10UF 16V X5R 0805"));
            Assert.That(part.Cost, Is.EqualTo(0.0264));
        }

        [Test]
        public async Task ShouldImportDigiKeyV4OrderCAD()
        {
            var testContext = new TestContext();
            var apiCredentials = testContext.CreateApiCredentials(enableDigiKey: true); // enable digikey only
            testContext.ApplyApiCredentials(apiCredentials);
            testContext.SetDigiKeyApiVersion(DigiKeyApiVersion.V4);

            testContext.ApiHttpClientFactory = new MockApiHttpClientFactory(new Dictionary<string, string>
            {
                { $"orderstatus/v4/salesorder/", "DigiKey-v4-ExternalOrder-1-CAD.json" }
            });

            var partService = new PartService(testContext.WebHostServiceConfiguration, testContext.MockLogger<PartService>(), testContext.StorageProvider, testContext.Mapper.Object,
                    testContext.IntegrationApiFactory, testContext.RequestContextAccessor.Object, testContext.PartTypesCache.Object);

            var request = new Model.Requests.OrderImportRequest
            {
                OrderId = TestConstants.OrderId,
                Username = TestConstants.UserName,
                Password = TestConstants.Password,
                Supplier = TestConstants.DigiKeySupplier,
                RequestProductInfo = false
            };
            var response = await partService.GetExternalOrderAsync(request);

            Assert.That(response?.Response, Is.Not.Null);
            Assert.That(response.Response.Amount, Is.EqualTo(758.56000000000006d));
            Assert.That(response.Response.Currency, Is.EqualTo("CAD"));
            Assert.That(response.Response.Parts.Count, Is.EqualTo(44));
            var part = response.Response.Parts.First();
            Assert.That(part.QuantityAvailable, Is.EqualTo(500));
            Assert.That(part.Cost, Is.EqualTo(0.0264));
            Assert.That(part.Description, Is.EqualTo("CAP CER 10UF 16V X5R 0805"));
            Assert.That(part.ManufacturerPartNumber, Is.EqualTo("CL21A106KOQNNNG"));
            Assert.That(part.Reference, Is.EqualTo("TESTPROJECT 24V CAB C10"));
            Assert.That(part.Supplier, Is.EqualTo("DigiKey"));
            Assert.That(part.SupplierPartNumber, Is.EqualTo("1276-6455-1-ND"));
        }

        [Test]
        public async Task ShouldImportDigiKeyV4OrderUSD()
        {
            var testContext = new TestContext();
            var apiCredentials = testContext.CreateApiCredentials(enableDigiKey: true); // enable digikey only
            testContext.ApplyApiCredentials(apiCredentials);
            testContext.SetDigiKeyApiVersion(DigiKeyApiVersion.V4);

            testContext.ApiHttpClientFactory = new MockApiHttpClientFactory(new Dictionary<string, string>
            {
                { $"orderstatus/v4/salesorder/", "DigiKey-v4-ExternalOrder-1-USD.json" }
            });

            var partService = new PartService(testContext.WebHostServiceConfiguration, testContext.MockLogger<PartService>(), testContext.StorageProvider, testContext.Mapper.Object,
                    testContext.IntegrationApiFactory, testContext.RequestContextAccessor.Object, testContext.PartTypesCache.Object);

            var request = new Model.Requests.OrderImportRequest
            {
                OrderId = TestConstants.OrderId,
                Username = TestConstants.UserName,
                Password = TestConstants.Password,
                Supplier = TestConstants.DigiKeySupplier,
                RequestProductInfo = false
            };
            var response = await partService.GetExternalOrderAsync(request);

            Assert.That(response?.Response, Is.Not.Null);
            Assert.That(response.Response.Amount, Is.EqualTo(758.56000000000006d));
            Assert.That(response.Response.Currency, Is.EqualTo("USD"));
            Assert.That(response.Response.Parts.Count, Is.EqualTo(44));
            var part = response.Response.Parts.First();
            Assert.That(part.QuantityAvailable, Is.EqualTo(500));
            Assert.That(part.Cost, Is.EqualTo(0.0264));
            Assert.That(part.Description, Is.EqualTo("CAP CER 10UF 16V X5R 0805"));
            Assert.That(part.ManufacturerPartNumber, Is.EqualTo("CL21A106KOQNNNG"));
            Assert.That(part.Reference, Is.EqualTo("TESTPROJECT 24V CAB C10"));
            Assert.That(part.Supplier, Is.EqualTo("DigiKey"));
            Assert.That(part.SupplierPartNumber, Is.EqualTo("1276-6455-1-ND"));
        }

        [Test]
        public async Task ShouldImportDigiKeyV4OrderEUR()
        {
            var testContext = new TestContext();
            var apiCredentials = testContext.CreateApiCredentials(enableDigiKey: true); // enable digikey only
            testContext.ApplyApiCredentials(apiCredentials);
            testContext.SetDigiKeyApiVersion(DigiKeyApiVersion.V4);

            testContext.ApiHttpClientFactory = new MockApiHttpClientFactory(new Dictionary<string, string>
            {
                { $"orderstatus/v4/salesorder/", "DigiKey-v4-ExternalOrder-1-EUR.json" }
            });

            var partService = new PartService(testContext.WebHostServiceConfiguration, testContext.MockLogger<PartService>(), testContext.StorageProvider, testContext.Mapper.Object,
                                testContext.IntegrationApiFactory, testContext.RequestContextAccessor.Object, testContext.PartTypesCache.Object);

            var request = new Model.Requests.OrderImportRequest
            {
                OrderId = TestConstants.OrderId,
                Username = TestConstants.UserName,
                Password = TestConstants.Password,
                Supplier = TestConstants.DigiKeySupplier,
                RequestProductInfo = false
            };
            var response = await partService.GetExternalOrderAsync(request);

            Assert.That(response?.Response, Is.Not.Null);
            Assert.That(response.Response.Amount, Is.EqualTo(758.56000000000006d));
            Assert.That(response.Response.Currency, Is.EqualTo("EUR"));
            Assert.That(response.Response.Parts.Count, Is.EqualTo(44));
            var part = response.Response.Parts.First();
            Assert.That(part.QuantityAvailable, Is.EqualTo(500));
            Assert.That(part.Cost, Is.EqualTo(0.0264));
            Assert.That(part.Description, Is.EqualTo("CAP CER 10UF 16V X5R 0805"));
            Assert.That(part.ManufacturerPartNumber, Is.EqualTo("CL21A106KOQNNNG"));
            Assert.That(part.Reference, Is.EqualTo("TESTPROJECT 24V CAB C10"));
            Assert.That(part.Supplier, Is.EqualTo("DigiKey"));
            Assert.That(part.SupplierPartNumber, Is.EqualTo("1276-6455-1-ND"));
        }

        [Test]
        public async Task ShouldSearchDigiKeyV3Part()
        {
            var testContext = new TestContext();
            var apiCredentials = testContext.CreateApiCredentials(enableDigiKey: true); // enable digikey only
            testContext.ApplyApiCredentials(apiCredentials);
            testContext.SetDigiKeyApiVersion(DigiKeyApiVersion.V3);

            testContext.ApiHttpClientFactory = new MockApiHttpClientFactory(new Dictionary<string, string>
            {
                { "/Search/v3/Products/Keyword", "DigiKey-v3-PartSearch-1.json" },
            });

            var partService = new PartService(testContext.WebHostServiceConfiguration, testContext.MockLogger<PartService>(), testContext.StorageProvider, testContext.Mapper.Object,
                    testContext.IntegrationApiFactory, testContext.RequestContextAccessor.Object, testContext.PartTypesCache.Object);

            var response = await partService.GetPartInformationAsync("LM358");

            Assert.That(response?.Response, Is.Not.Null);
            Assert.That(response.Response.Parts.Count, Is.EqualTo(66));
            Assert.That(response.Response.Datasheets.Count, Is.EqualTo(13));
            Assert.That(response.Response.ProductImages.Count, Is.EqualTo(5));
            var part = response.Response.Parts.Skip(2).First();
            Assert.That(part.Supplier, Is.EqualTo("DigiKey"));
            Assert.That(part.SupplierPartNumber, Is.EqualTo("2156-LM358PE3-ND"));
            Assert.That(part.BasePartNumber, Is.EqualTo("LM358"));
            Assert.That(part.Description, Is.EqualTo("IC OPAMP GP 2 CIRCUIT 8DIP\r\nGeneral Purpose Amplifier 2 Circuit 8-PDIP"));
            Assert.That(part.Manufacturer, Is.EqualTo("Texas Instruments"));
            Assert.That(part.ManufacturerPartNumber, Is.EqualTo("LM358PE3"));
            Assert.That(part.Keywords.Count, Is.EqualTo(7));
            Assert.That(part.Cost, Is.EqualTo(0.14));
            Assert.That(part.Currency, Is.EqualTo("USD"));
            Assert.That(part.PartType, Is.EqualTo("IC"));
            Assert.That(part.QuantityAvailable, Is.EqualTo(274232));
        }

        [Test]
        public async Task ShouldSearchDigiKeyV4Part()
        {
            var testContext = new TestContext();
            var apiCredentials = testContext.CreateApiCredentials(enableDigiKey: true); // enable digikey only
            testContext.ApplyApiCredentials(apiCredentials);
            testContext.SetDigiKeyApiVersion(DigiKeyApiVersion.V4); // test v4 api

            testContext.ApiHttpClientFactory = new MockApiHttpClientFactory(new Dictionary<string, string>
            {
                { "/products/v4/search/keyword", "DigiKey-v4-PartSearch-1.json" },
            });

            var partService = new PartService(testContext.WebHostServiceConfiguration, testContext.MockLogger<PartService>(), testContext.StorageProvider, testContext.Mapper.Object,
                    testContext.IntegrationApiFactory, testContext.RequestContextAccessor.Object, testContext.PartTypesCache.Object);

            var response = await partService.GetPartInformationAsync("LM358");

            Assert.That(response?.Response, Is.Not.Null);
            Assert.That(response.Response.Parts.Count, Is.EqualTo(26));
            Assert.That(response.Response.Datasheets.Count, Is.EqualTo(13));
            Assert.That(response.Response.ProductImages.Count, Is.EqualTo(5));
            var part = response.Response.Parts.Skip(2).First();
            Assert.That(part.Supplier, Is.EqualTo("DigiKey"));
            Assert.That(part.SupplierPartNumber, Is.EqualTo("2156-LM358PE3-ND"));
            Assert.That(part.BasePartNumber, Is.EqualTo("LM358"));
            Assert.That(part.Description, Is.EqualTo("IC OPAMP GP 2 CIRCUIT 8DIP\r\nGeneral Purpose Amplifier 2 Circuit 8-PDIP"));
            Assert.That(part.Manufacturer, Is.EqualTo("Texas Instruments"));
            Assert.That(part.ManufacturerPartNumber, Is.EqualTo("LM358PE3"));
            Assert.That(part.Keywords.Count, Is.EqualTo(12));
            Assert.That(part.Cost, Is.EqualTo(0.14));
            Assert.That(part.Currency, Is.EqualTo("USD"));
            Assert.That(part.PartType, Is.EqualTo("IC"));
            Assert.That(part.QuantityAvailable, Is.EqualTo(274232));
        }
    }
}
