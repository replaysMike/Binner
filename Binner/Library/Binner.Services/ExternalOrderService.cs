using Binner.Model;
using Binner.Model.Requests;
using Binner.Model.Responses;
using Binner.Services.Integrations.ExternalOrder;

namespace Binner.Services
{
    public class ExternalOrderService : IExternalOrderService
    {
        private readonly IDigiKeyExternalOrderService _digiKeyOrderService;
        private readonly IMouserExternalOrderService _mouserOrderService;
        private readonly IArrowExternalOrderService _arrowOrderService;
        private readonly ITmeExternalOrderService _tmeOrderService;

        public ExternalOrderService(IDigiKeyExternalOrderService digiKeyOrderService, IMouserExternalOrderService mouserOrderService, IArrowExternalOrderService arrowOrderService, ITmeExternalOrderService tmeOrderService)
        {
            _digiKeyOrderService = digiKeyOrderService;
            _mouserOrderService = mouserOrderService;
            _arrowOrderService = arrowOrderService;
            _tmeOrderService = tmeOrderService; 
        }

        /// <summary>
        /// Get an external order
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual async Task<IServiceResult<ExternalOrderResponse?>> GetExternalOrderAsync(OrderImportRequest request)
        {
            if (string.IsNullOrEmpty(request.OrderId)) throw new InvalidOperationException($"OrderId must be provided");

            switch (request.Supplier?.ToLower())
            {
                case "digikey":
                    return await _digiKeyOrderService.GetExternalOrderAsync(request);
                case "mouser":
                    return await _mouserOrderService.GetExternalOrderAsync(request);
                case "arrow":
                    return await _arrowOrderService.GetExternalOrderAsync(request);
                case "tme":
                    //return await _tmeOrderService.GetExternalOrderAsync(request);
                    throw new NotSupportedException($"TME order imports are not yet supported as they don't have an API for it.");
                default:
                    throw new InvalidOperationException($"Unknown supplier {request.Supplier}");
            }
        }
    }
}
