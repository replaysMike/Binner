using NUnit.Framework;

namespace Binner.Services.Tests
{
    [TestFixture]
    public class PartServiceDigiKeyTests
    {

        private PartService ConstructPartService(Testing.TestContext testContext) => new PartService(testContext.WebHostServiceConfiguration, testContext.MockLogger<PartService>(), testContext.StorageProvider, testContext.Mapper,
                    testContext.IntegrationApiFactory, testContext.RequestContextAccessor.Object, testContext.PartTypesCache.Object, testContext.ExternalOrderService, testContext.ExternalBarcodeInfoService.Object, testContext.ExternalPartInfoService, testContext.ExternalCategoriesService.Object, testContext.BaseIntegrationBehavior.Object, testContext.UserConfigurationService.Object);
    }
}
