using Binner.Common.Extensions;
using Binner.Common.Integrations.Models;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations.DigiKey;
using Binner.Model.Integrations.DigiKey.V3;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static Binner.Common.Integrations.DigikeyApi;

namespace Binner.Common.Integrations
{
    public sealed class DigikeyV3Api : BaseDigikeyApi, IDigikeyApi
    {
        private readonly ILogger<DigikeyApi> _logger;
        private readonly DigikeyConfiguration _configuration;
        private readonly LocaleConfiguration _localeConfiguration;
        private readonly JsonSerializerSettings _serializerSettings;

        #region Include Fields
        private static readonly List<string> V3IncludeFieldNames = new List<string> {
            "DigiKeyPartNumber",
            "QuantityAvailable",
            "Manufacturer",
            "ManufacturerPartNumber",
            "PrimaryDatasheet",
            "ProductDescription",
            "DetailedDescription",
            "MinimumOrderQuantity",
            "NonStock",
            "UnitPrice",
            "ProductStatus",
            "ProductUrl",
            "PrimaryPhoto",
            "PrimaryVideo",
            "Packaging",
            "AlternatePackaging",
            "Family",
            "Category",
            "Parameters"
        };
        #endregion

        public DigikeyV3Api(ILogger<DigikeyApi> logger, DigikeyConfiguration configuration, LocaleConfiguration localeConfiguration, JsonSerializerSettings serializerSettings, IApiHttpClientFactory httpClientFactory)
            : base(logger, configuration, localeConfiguration, serializerSettings, httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _localeConfiguration = localeConfiguration;
            _serializerSettings = serializerSettings;
        }

        public async Task<IApiResponse> GetOrderAsync(OAuthAuthorization authenticationResponse, string orderId)
        {
            try
            {
                // set what fields we want from the API
                var uri = Url.Combine(_configuration.ApiUrl, "OrderDetails/v3/Status/", orderId);
                var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Get, uri);
                // perform a keywords API search
                var response = await _client.SendAsync(requestMessage);
                var result = await TryHandleResponseAsync(response, authenticationResponse);
                if (!result.IsSuccessful)
                {
                    // return api error
                    return result.Response;
                }

                // 200 OK
                var responseJson = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<OrderSearchResponse>(responseJson, _serializerSettings) ?? new();
                return new ApiResponse(results, nameof(DigikeyApi));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IApiResponse> SearchAsync(OAuthAuthorization authenticationResponse, string partNumber, string? partType, string? mountingType, int recordCount = 25, Dictionary<string, string>? additionalOptions = null)
        {
            /* important reminder - don't reference authResponse in here! */
            _logger.LogInformation($"[{nameof(SearchAsync)}] Called using accesstoken='{authenticationResponse.AccessToken}'");

            var keywords = new List<string>();
            if (!string.IsNullOrEmpty(partNumber))
                keywords = partNumber
                    .ToLower()
                    .Split([" "], StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            var packageTypeEnum = MountingTypes.None;
            if (!string.IsNullOrEmpty(mountingType))
            {
                switch (mountingType.ToLower())
                {
                    case "surface mount":
                        packageTypeEnum = MountingTypes.SurfaceMount;
                        break;
                    case "through hole":
                        packageTypeEnum = MountingTypes.ThroughHole;
                        break;
                }
            }

            try
            {
                // set what fields we want from the API
                var values = new Dictionary<string, string> {
                    { "Includes", $"Products({string.Join(",", V3IncludeFieldNames)})" },
                };
                // includes are passed as Products({comma delimited list of field names})
                var uri = Url.Combine(_configuration.ApiUrl, "/Search/v3/Products", $"/Keyword?" + string.Join("&", values.Select(x => $"{x.Key}={x.Value}")));
                //var uri = Url.Combine(_configuration.ApiUrl, $"/Search/v3/Products"); // fetches all fields

                // map taxonomies (categories) to the part type
                var taxonomies = MapTaxonomies(partType, packageTypeEnum);

                // attempt to detect certain words and apply them as parametric filters
                var (parametricFilters, filteredKeywords) = MapParametricFilters(keywords, packageTypeEnum, taxonomies);

                var request = new KeywordSearchRequest
                {
                    Keywords = string.Join(" ", filteredKeywords),
                    RecordCount = recordCount,
                    Filters = new Filters
                    {
                        TaxonomyIds = taxonomies.Select(x => (int)x).ToList(),
                        ParametricFilters = parametricFilters
                    },
                    SearchOptions = new List<SearchOptions> { }
                };
                var result = await PerformApiSearchQueryAsync(authenticationResponse, uri, request);
                if (result.ErrorResponse != null) return result.ErrorResponse;

                // if no results are returned, perform a secondary search on the original keyword search with no modifications via parametric filtering
                if (!result.SuccessResponse.Products.Any() && filteredKeywords.Count != keywords.Count)
                {
                    request = new KeywordSearchRequest
                    {
                        Keywords = string.Join(" ", keywords),
                        RecordCount = recordCount,
                        Filters = new Filters
                        {
                            TaxonomyIds = taxonomies.Select(x => (int)x).ToList(),
                            // don't include parametric filters
                            ParametricFilters = new List<ParametricFilter>()
                        },
                        SearchOptions = new List<SearchOptions> { }
                    };
                    result = await PerformApiSearchQueryAsync(authenticationResponse, uri, request);
                    if (result.ErrorResponse != null) return result.ErrorResponse;
                }

                return new ApiResponse(result.SuccessResponse, nameof(DigikeyApi));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IApiResponse> GetProductDetailsAsync(OAuthAuthorization authenticationResponse, string partNumber)
        {
            /* same as SearchAsync() but passing the api different parameters */
            try
            {
                // set what fields we want from the API
                var uri = Url.Combine(_configuration.ApiUrl, "Search/v3/Products/", HttpUtility.UrlEncode(partNumber));
                _logger.LogInformation($"[{nameof(GetProductDetailsAsync)}] Creating product search request for '{partNumber}'...");
                var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Get, uri);
                // perform a keywords API search
                var response = await _client.SendAsync(requestMessage);
                _logger.LogInformation($"[{nameof(GetProductDetailsAsync)}] Api responded with '{response.StatusCode}'");
                var result = await TryHandleResponseAsync(response, authenticationResponse);
                if (!result.IsSuccessful)
                {
                    // return api error
                    return result.Response;
                }

                // 200 OK
                var responseJson = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<Product>(responseJson, _serializerSettings) ?? new();
                return new ApiResponse(results, nameof(DigikeyApi));
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<(IApiResponse? ErrorResponse, KeywordSearchResponse SuccessResponse, string Request, string Response)> PerformApiSearchQueryAsync(OAuthAuthorization authenticationResponse, Uri uri, KeywordSearchRequest request)
        {
            _logger.LogInformation($"[{nameof(PerformApiSearchQueryAsync)}] Creating search request for '{request.Keywords}' using accesstoken='{authenticationResponse.AccessToken}'...");
            using var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Post, uri);
            var requestJson = JsonConvert.SerializeObject(request, _serializerSettings);
            requestMessage.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            // perform a keywords API search
            using var response = await _client.SendAsync(requestMessage);
            _logger.LogInformation($"[{nameof(PerformApiSearchQueryAsync)}] Api responded with '{response.StatusCode}'. accesstoken='{authenticationResponse.AccessToken}'");
            var result = await TryHandleResponseAsync(response, authenticationResponse);
            if (!result.IsSuccessful)
            {
                // return api error
                return (result.Response, new KeywordSearchResponse(), requestJson, string.Empty);
            }

            // 200 OK
            var responseJson = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<KeywordSearchResponse>(responseJson, _serializerSettings) ?? new();
            return (null, results, requestJson, responseJson);
        }

        private async Task<IApiResponse> ProductSearchAsync(OAuthAuthorization authenticationResponse, string partNumber)
        {
            /* important reminder - don't reference authResponse in here! */
            try
            {
                // set what fields we want from the API
                var uri = Url.Combine(_configuration.ApiUrl, "Search/v3/Products/", HttpUtility.UrlEncode(partNumber));
                _logger.LogInformation($"[{nameof(ProductSearchAsync)}] Creating product search request for '{partNumber}'...");
                var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Get, uri);
                // perform a keywords API search
                var response = await _client.SendAsync(requestMessage);
                _logger.LogInformation($"[{nameof(ProductSearchAsync)}] Api responded with '{response.StatusCode}'");
                var result = await TryHandleResponseAsync(response, authenticationResponse);
                if (!result.IsSuccessful)
                {
                    // return api error
                    return result.Response;
                }

                // 200 OK
                var responseJson = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<Product>(responseJson, _serializerSettings) ?? new();
                return new ApiResponse(results, nameof(DigikeyApi));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IApiResponse> GetBarcodeDetailsAsync(OAuthAuthorization authenticationResponse, string barcode, ScannedBarcodeType barcodeType)
        {
            /* important reminder - don't reference authResponse in here! */
            try
            {
                // set what fields we want from the API
                // https://developer.digikey.com/products/barcode/barcoding/productbarcode

                var is2dBarcode = barcode.StartsWith("[)>");
                var endpoint = "Barcoding/v3/ProductBarcodes/";
                switch (barcodeType)
                {
                    case ScannedBarcodeType.Product:
                    default:
                        endpoint = "Barcoding/v3/ProductBarcodes/";
                        if (is2dBarcode)
                            endpoint = "Barcoding/v3/Product2DBarcodes/";
                        break;
                    case ScannedBarcodeType.Packlist:
                        endpoint = "Barcoding/v3/PackListBarcodes/";
                        if (is2dBarcode)
                            endpoint = "Barcoding/v3/PackList2DBarcodes/";
                        break;
                }

                var barcodeFormatted = barcode.ToString();
                if (is2dBarcode)
                {
                    // DigiKey requires the GS (Group separator) to be \u241D, and the RS (Record separator) to be \u241E
                    // GS
                    var gsReplacement = "\u241D";
                    barcodeFormatted = barcodeFormatted.Replace("\u001d", gsReplacement);
                    barcodeFormatted = barcodeFormatted.Replace("\u005d", gsReplacement);
                    // RS
                    var rsReplacement = "\u241E";
                    barcodeFormatted = barcodeFormatted.Replace("\u001e", rsReplacement);
                    barcodeFormatted = barcodeFormatted.Replace("\u005e", rsReplacement);

                }
                var barcodeFormattedPath = HttpUtility.UrlEncode(barcodeFormatted);

                var uri = new Uri(string.Join("/", _configuration.ApiUrl, endpoint) + barcodeFormattedPath);
                var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Get, uri);
                // perform a keywords API search
                var response = await _client.SendAsync(requestMessage);
                var result = await TryHandleResponseAsync(response, authenticationResponse);
                if (!result.IsSuccessful)
                {
                    // return api error
                    var contentString = await response.Content.ReadAsStringAsync();
                    result.Response.Errors.Add(contentString);
                    return result.Response;
                }

                // 200 OK
                var responseJson = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<ProductBarcodeResponse>(responseJson, _serializerSettings) ?? new();
                return new ApiResponse(results, nameof(DigikeyApi));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IApiResponse> GetCategoriesAsync(OAuthAuthorization authenticationResponse)
        {
            /* important reminder - don't reference authResponse in here! */
            try
            {
                // set what fields we want from the API
                var uri = Url.Combine(_configuration.ApiUrl, "Search/v3/Categories");
                _logger.LogInformation($"[{nameof(GetCategoriesAsync)}] Creating categories request...");
                var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Get, uri);
                // perform a keywords API search
                var response = await _client.SendAsync(requestMessage);
                _logger.LogInformation($"[{nameof(GetCategoriesAsync)}] Api responded with '{response.StatusCode}'");
                var result = await TryHandleResponseAsync(response, authenticationResponse);
                if (!result.IsSuccessful)
                {
                    // return api error
                    return result.Response;
                }

                // 200 OK
                var responseJson = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<Model.Integrations.DigiKey.V3.CategoriesResponse>(responseJson, _serializerSettings) ?? new();
                return new ApiResponse(results, nameof(DigikeyApi));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
