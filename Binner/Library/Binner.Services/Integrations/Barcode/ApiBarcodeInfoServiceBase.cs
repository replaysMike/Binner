using Binner.Global.Common;
using Binner.Model;
using Microsoft.Extensions.Logging;

namespace Binner.Services.Integrations.Barcode
{
    public class ApiBarcodeInfoServiceBase : BaseIntegrationBehavior
    {
        public ApiBarcodeInfoServiceBase(ILogger<BaseIntegrationBehavior> logger, IStorageProvider storageProvider, IRequestContextAccessor requestContextAccessor, IPartTypeDetection<CommonPart> partTypeDetection)
            : base(logger, storageProvider, requestContextAccessor, partTypeDetection)
        {
        }
    }
}
