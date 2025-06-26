using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Requests;
using Binner.Model.Responses;

namespace Binner.Services.Integrations.ExternalOrder
{
    /// <summary>
    /// TME external order service
    /// </summary>
    /// <remarks>Maps an external order response to a CommonPart</remarks>
    public class TmeExternalOrderService : ApiExternalOrderServiceBase, ITmeExternalOrderService
    {
        protected readonly WebHostServiceConfiguration _configuration;
        protected readonly IIntegrationApiFactory _integrationApiFactory;

        public TmeExternalOrderService(WebHostServiceConfiguration configuration, IStorageProvider storageProvider, IIntegrationApiFactory integrationApiFactory, IRequestContextAccessor requestContextAccessor)
            : base(storageProvider, requestContextAccessor)
        {
            _configuration = configuration;
            _integrationApiFactory = integrationApiFactory;
        }

        public virtual async Task<IServiceResult<ExternalOrderResponse?>> GetExternalOrderAsync(OrderImportRequest request)
        {
            throw new NotSupportedException($"TME order imports are not yet supported as they don't have an API for it.");
        }

    }
}
