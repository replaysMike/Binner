using Binner.Global.Common;
using Binner.Model;

namespace Binner.Services.Integrations.PartInformation
{
    public class PartInfoServiceBase : BaseIntegrationBehavior
    {
        protected const string MissingDatasheetCoverName = "datasheetcover.png";

        public PartInfoServiceBase(IStorageProvider storageProvider, IRequestContextAccessor requestContextAccessor)
            : base(storageProvider, requestContextAccessor)
        {
        }
    }
}
