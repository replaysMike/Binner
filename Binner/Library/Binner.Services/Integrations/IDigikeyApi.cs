
using Binner.Common;
using Binner.Model;
using Binner.Model.Integrations;

namespace Binner.Services.Integrations
{
    public interface IDigikeyApi
    {
        Task<IApiResponse> GetOrderAsync(OAuthAuthorization authenticationResponse, string orderId);
        Task<IApiResponse> GetBarcodeDetailsAsync(OAuthAuthorization authenticationResponse, string barcode, ScannedLabelType barcodeType);
        Task<IApiResponse> GetCategoriesAsync(OAuthAuthorization authenticationResponse);
        Task<IApiResponse> SearchAsync(OAuthAuthorization authenticationResponse, string partNumber, string? partType, string? mountingType, int recordCount = ApiConstants.MaxRecords, Dictionary<string, string>? additionalOptions = null);
        Task<IApiResponse> GetProductDetailsAsync(OAuthAuthorization authenticationResponse, string partNumber);
    }
}