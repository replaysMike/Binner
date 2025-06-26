using Binner.Global.Common;
using Binner.Model;

namespace Binner.Services.Integrations.ExternalOrder
{
    public class ApiExternalOrderServiceBase : BaseIntegrationBehavior
    {
        public ApiExternalOrderServiceBase(IStorageProvider storageProvider, IRequestContextAccessor requestContextAccessor)
            : base(storageProvider, requestContextAccessor)
        {
        }        
    }
}
