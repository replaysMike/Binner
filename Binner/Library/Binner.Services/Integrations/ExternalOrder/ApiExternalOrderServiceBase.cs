using Binner.Global.Common;
using Binner.Model;
using Microsoft.Extensions.Logging;

namespace Binner.Services.Integrations.ExternalOrder
{
    public class ApiExternalOrderServiceBase : BaseIntegrationBehavior
    {
        public ApiExternalOrderServiceBase(ILogger<BaseIntegrationBehavior> logger, IStorageProvider storageProvider, IRequestContextAccessor requestContextAccessor)
            : base(logger, storageProvider, requestContextAccessor)
        {
        }        
    }
}
