using NUnit.Framework;

namespace Binner.Services.Tests
{
    [TestFixture]
    public class PartServiceTmeTests
    {
        [Test]
        public async Task ShouldSearchTmePart()
        {
            var testContext = new Testing.TestContext();
            var apiCredentials = testContext.CreateApiCredentials(enableTme: true); // enable tme only
            testContext.ApplyApiCredentials(apiCredentials);

            testContext.ApiHttpClientFactory.SetResponseMapping(new Dictionary<string, string>
            {
                // tme makes 3 api calls to get all part information
                { "/Products/Search.json", "Tme-PartSearch-1-Search.json" },
                { "/Products/GetPricesAndStocks.json", "Tme-PartSearch-1-Prices.json" },
                { "/Products/GetProductsFiles.json", "Tme-PartSearch-1-Files.json" },
            });

            var partService = ConstructPartService(testContext);

            var response = await partService.GetPartInformationAsync("LM393");

            Assert.That(response?.Response, Is.Not.Null);
            Assert.That(response.Response.Parts.Count, Is.EqualTo(20));
            Assert.That(response.Response.Datasheets.Count, Is.EqualTo(13)); // because we haven't mocked the api request to GetProductFilesAsync()
            Assert.That(response.Response.ProductImages.Count, Is.EqualTo(12));
            var part = response.Response.Parts.First();
            Assert.That(part.Supplier, Is.EqualTo("TME"));
            Assert.That(part.SupplierPartNumber, Is.EqualTo("LM393DGKR"));
            Assert.That(part.BasePartNumber, Is.EqualTo("LM393DGKR"));
            Assert.That(part.Description, Is.EqualTo("IC: comparator; universal; Cmp: 2; 2÷30V; SMT; VSSOP8; reel,tape"));
            Assert.That(part.Manufacturer, Is.EqualTo("TEXAS INSTRUMENTS"));
            Assert.That(part.ManufacturerPartNumber, Is.EqualTo("LM393DGKR"));
            Assert.That(part.Keywords.Count, Is.EqualTo(6));
            Assert.That(part.Cost, Is.EqualTo(0.477));
            Assert.That(part.Currency, Is.EqualTo("USD"));
            Assert.That(part.PartType, Is.EqualTo("IC"));
            Assert.That(part.QuantityAvailable, Is.EqualTo(6052));
        }

        [Test]
        public async Task ShouldSearchTmePartNoResult()
        {
            var testContext = new Testing.TestContext();
            var apiCredentials = testContext.CreateApiCredentials(enableTme: true); // enable tme only
            testContext.ApplyApiCredentials(apiCredentials);

            testContext.ApiHttpClientFactory.SetResponseMapping(new Dictionary<string, string>
            {
                // tme makes 3 api calls to get all part information
                { "/Products/Search.json", "TmePartSearch-NoResults.json" }
            });

            var partService = ConstructPartService(testContext);

            var response = await partService.GetPartInformationAsync("LN393");

            Assert.That(response?.Response, Is.Not.Null);
            Assert.That(response.Response.Parts.Count, Is.EqualTo(0));
            Assert.That(response.Response.Datasheets.Count, Is.EqualTo(0));
            Assert.That(response.Response.ProductImages.Count, Is.EqualTo(0));
        }

        private PartService ConstructPartService(Testing.TestContext testContext) => new PartService(testContext.WebHostServiceConfiguration, testContext.MockLogger<PartService>(), testContext.StorageProvider, testContext.Mapper.Object,
                    testContext.IntegrationApiFactory, testContext.RequestContextAccessor.Object, testContext.PartTypesCache.Object, testContext.ExternalOrderService, testContext.ExternalBarcodeInfoService.Object, testContext.ExternalPartInfoService, testContext.ExternalCategoriesService.Object, testContext.BaseIntegrationBehavior.Object, testContext.UserConfigurationService.Object);
    }
}
