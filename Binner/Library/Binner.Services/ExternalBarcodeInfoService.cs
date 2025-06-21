using Binner.Model;
using Binner.Services.Integrations.Barcode;
using Binner.Services.Integrations.ExternalOrder;

namespace Binner.Services
{
    public class ExternalBarcodeInfoService : IExternalBarcodeInfoService
    {
        private readonly IDigiKeyBarcodeInfoService _digiKeyBarcodeInfoService;

        public ExternalBarcodeInfoService(IDigiKeyBarcodeInfoService digiKeyBarcodeInfoService, IMouserExternalOrderService mouserOrderService, IArrowExternalOrderService arrowOrderService, ITmeExternalOrderService tmeOrderService)
        {
            _digiKeyBarcodeInfoService = digiKeyBarcodeInfoService;
        }

        /// <summary>
        /// Get barcode information from an external api
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual async Task<IServiceResult<PartResults?>> GetBarcodeInfoAsync(string barcode, ScannedLabelType barcodeType)
        {
            if (string.IsNullOrEmpty(barcode)) throw new InvalidOperationException($"Barcode must be provided");
            return await _digiKeyBarcodeInfoService.GetBarcodeInfoAsync(barcode, barcodeType);
        }
    }
}
