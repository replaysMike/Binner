using Binner.Global.Common;
using Binner.Model;
using Microsoft.Extensions.Logging;

namespace Binner.Services.Integrations.PartInformation
{
    public class PartInfoServiceBase : BaseIntegrationBehavior
    {
        protected const string MissingDatasheetCoverName = "datasheetcover.png";

        public PartInfoServiceBase(ILogger<BaseIntegrationBehavior> logger, IStorageProvider storageProvider, IRequestContextAccessor requestContextAccessor, IPartTypeDetection<CommonPart> partTypeDetection)
            : base(logger, storageProvider, requestContextAccessor, partTypeDetection)
        {
        }
    }
}
