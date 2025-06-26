using Binner.Model;
using Binner.Model.Requests;
using Binner.Model.Responses;

namespace Binner.Services
{
    public interface IExternalOrderService
    {
        /// <summary>
        /// Get an order from an external api
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IServiceResult<ExternalOrderResponse?>> GetExternalOrderAsync(OrderImportRequest request);
    }
}
