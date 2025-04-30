using Binner.Common.Integrations.Models;
using Binner.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binner.Common.Integrations
{
    public interface IDigikeyApi
    {
        Task<IApiResponse> GetOrderAsync(OAuthAuthorization authenticationResponse, string orderId);
        Task<IApiResponse> GetBarcodeDetailsAsync(OAuthAuthorization authenticationResponse, string barcode, ScannedBarcodeType barcodeType);
        Task<IApiResponse> GetCategoriesAsync(OAuthAuthorization authenticationResponse);
        Task<IApiResponse> SearchAsync(OAuthAuthorization authenticationResponse, string partNumber, string? partType, string? mountingType, int recordCount = 25, Dictionary<string, string>? additionalOptions = null);
        Task<IApiResponse> GetProductDetailsAsync(OAuthAuthorization authenticationResponse, string partNumber);
    }
}