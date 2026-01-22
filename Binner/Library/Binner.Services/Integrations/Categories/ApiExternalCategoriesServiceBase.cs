using Binner.Global.Common;
using Binner.Model;
using Microsoft.Extensions.Logging;

namespace Binner.Services.Integrations.Categories
{
    public class ApiExternalCategoriesServiceBase : BaseIntegrationBehavior
    {
        public ApiExternalCategoriesServiceBase(ILogger<BaseIntegrationBehavior> logger, IStorageProvider storageProvider, IRequestContextAccessor requestContextAccessor)
            : base(logger, storageProvider, requestContextAccessor)
        {
        }
    }
}
