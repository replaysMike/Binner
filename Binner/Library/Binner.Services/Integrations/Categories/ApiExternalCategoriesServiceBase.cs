using Binner.Global.Common;
using Binner.Model;

namespace Binner.Services.Integrations.Categories
{
    public class ApiExternalCategoriesServiceBase : BaseIntegrationBehavior
    {
        public ApiExternalCategoriesServiceBase(IStorageProvider storageProvider, IRequestContextAccessor requestContextAccessor)
            : base(storageProvider, requestContextAccessor)
        {
        }
    }
}
