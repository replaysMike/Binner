using Binner.Model;

namespace Binner.Services
{
    public interface IExternalBarcodeInfoService
    {
        /// <summary>
        /// Get a barcode information from an external API
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IServiceResult<PartResults?>> GetBarcodeInfoAsync(string barcode, ScannedLabelType barcodeType);
    }
}
