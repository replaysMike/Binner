using Binner.Model;
using Binner.Model.Requests;
using Binner.Model.Responses;

namespace Binner.Services.Integrations.ExternalOrder
{
    /// <summary>
    /// Defines external orders interface for retrieving external order information.
    /// </summary>
    public interface IApiExternalOrderService
    {
        Task<IServiceResult<ExternalOrderResponse?>> GetExternalOrderAsync(OrderImportRequest request);
    }
}