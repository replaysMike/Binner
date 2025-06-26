using Binner.Global.Common;
using Binner.Model;

namespace Binner.Services.Integrations.Barcode
{
    public class ApiBarcodeInfoServiceBase : BaseIntegrationBehavior
    {
        public ApiBarcodeInfoServiceBase(IStorageProvider storageProvider, IRequestContextAccessor requestContextAccessor)
            : base(storageProvider, requestContextAccessor)
        {
        }
    }
}
