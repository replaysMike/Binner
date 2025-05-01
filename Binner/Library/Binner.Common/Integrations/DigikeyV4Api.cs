using Binner.Common.Extensions;
using Binner.Common.Integrations.Models;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations.DigiKey;
using Binner.Model.Integrations.DigiKey.V4;
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
    public sealed class DigikeyV4Api : BaseDigikeyApi, IDigikeyApi
    {
        private const DigiKeyApiVersion ApiVersion = DigiKeyApiVersion.V4;
        private readonly ILogger<DigikeyApi> _logger;
        private readonly DigikeyConfiguration _configuration;
        private readonly JsonSerializerSettings _serializerSettings;

        #region Include Fields
        private static readonly List<string> V4IncludeFieldNames = new List<string> {
            "Description",
            "Manufacturer",
            "ManufacturerProductNumber",
            "UnitPrice",
            "ProductUrl",
            "DatasheetUrl",
            "PhotoUrl",
            "ProductVariations",
            "QuantityAvailable",
            "ProductStatus",
            "BackOrderNotAllowed",
            "NormallyStocking",
            "Discontinued",
            "EndOfLife",
            "PrimaryVideoUrl",
            "Parameters",
            "BaseProductNumber",
            "Category",
            "ManufacturerLeadWeeks",
            "ManufacturerPublicQuantity",
            "Series",
            "Classifications",
            "OtherNames"
        };
        #endregion

        public DigikeyV4Api(ILogger<DigikeyApi> logger, DigikeyConfiguration configuration, LocaleConfiguration localeConfiguration, JsonSerializerSettings serializerSettings, IApiHttpClientFactory httpClientFactory)
            : base(logger, configuration, localeConfiguration, serializerSettings, httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _serializerSettings = serializerSettings;
        }

        public async Task<IApiResponse> GetOrderAsync(OAuthAuthorization authenticationResponse, string orderId)
        {
            try
            {
                // set what fields we want from the API
                var uri = Url.Combine(_configuration.ApiUrl, $"orderstatus/v4/salesorder/{orderId}");
                var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Get, uri);
                // perform a keywords API search
                var response = await _client.SendAsync(requestMessage);
                var result = await TryHandleResponseAsync(response, authenticationResponse, ApiVersion);
                if (!result.IsSuccessful)
                {
                    // return api error
                    return result.Response;
                }

                // 200 OK
                var resultString = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<SalesOrder>(resultString, _serializerSettings) ?? new();
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
            _logger.LogInformation($"[{nameof(SearchAsync)}] Called using accesstoken='{authenticationResponse.AccessToken.Sanitize()}'");

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
                    case "surfacemount":
                        packageTypeEnum = MountingTypes.SurfaceMount;
                        break;
                    case "through hole":
                    case "throughhole":
                        packageTypeEnum = MountingTypes.ThroughHole;
                        break;
                }
            }

            try
            {
                // set what fields we want from the API
                var values = new Dictionary<string, string> {
                    { "includes", $"Products({string.Join(",", V4IncludeFieldNames)})" },
                };
                // includes are passed as Products({comma delimited list of field names})
                //var uri = Url.Combine(_configuration.ApiUrl, $"products/v4/search/keyword?" + string.Join("&", values.Select(x => $"{x.Key}={x.Value}")));
                var uri = Url.Combine(_configuration.ApiUrl, $"products/v4/search/keyword"); // fetches all fields

                // map taxonomies (categories) to the part type
                var taxonomies = MapTaxonomies(partType, packageTypeEnum);

                // attempt to detect certain words and apply them as parametric filters
                var (parametricFilters, filteredKeywords) = MapParametricFilters(keywords, packageTypeEnum, taxonomies);

                // todo: we will need a lot more data in order to do filtering pre-emptively.
                // DigiKey v4 requires an initial search to be done, and the ability to filter category & child category at the same time.
                // so, it's very complicated and requires some data mapping.
                /*var parameterFilterRequest = new ParameterFilterRequest();
                parameterFilterRequest.ParameterFilters = new List<ParametricCategory>();
                parameterFilterRequest.CategoryFilter = new FilterId { Id = "797" };
                foreach (var parametricFilter in parametricFilters)
                {
                    var filter = new ParametricCategory
                    {
                        ParameterId = 1742,
                        FilterValues = new List<FilterId> { new FilterId { Id = "351775" } },
                        //ParameterId = parametricFilter.ParameterId,
                        // valueId is resistance
                        //FilterValues = new List<FilterId> { new FilterId { Id = parametricFilter?.ValueId ?? string.Empty } },
                    };
                    parameterFilterRequest.ParameterFilters.Add(filter);
                }*/

                var request = new KeywordSearchRequest
                {
                    //Keywords = string.Join(" ", filteredKeywords),
                    Keywords = string.Join(" ", keywords),
                    Limit = recordCount,
                    Offset = 0,
                    FilterOptionsRequest = new FilterOptionsRequest
                    {
                        // manufacturers must be referenced by numeric manufactuer id as string, not name.
                        //ManufacturerFilter = new List<FilterId>() { new FilterId("1882") },
                        // category's must be referenced by numeric category id as string, not name.
                        CategoryFilter = taxonomies.Any() 
                            ? taxonomies.Select(x => new FilterId() { Id = ((int)x).ToString() }).ToList() 
                            : new List<FilterId>(),
                        // v4 parametric filters are much more complicated. They require an initial search first which returns a list of
                        // all the possible parametric filters available - more like how the DigiKey website works.
                        // The Category Id, subcategory Id, and parameter Id are all required to be passed in the request.
                        //ParameterFilterRequest = parameterFilterRequest,
                        /*ParameterFilterRequest = new ParameterFilterRequest
                        {
                            CategoryFilter = new FilterId { Id = "797" },
                            ParameterFilters = new List<ParametricCategory>
                            {
                                new ParametricCategory
                                {
                                    ParameterId = 1742,
                                    FilterValues = new List<FilterId> { new FilterId { Id = "351775" } },
                                }
                            }
                        }*/
                    },
                    SortOptions = new SortOptions
                    {
                        Field = SortFields.QuantityAvailable,
                        SortOrder = SortDirection.Descending
                    },
                };
                var result = await PerformApiSearchQueryAsync(authenticationResponse, uri, request);
                if (result.ErrorResponse != null) return result.ErrorResponse;

                // if no results are returned, perform a secondary search on the original keyword search with no modifications via parametric filtering
                if (!result.SuccessResponse.Products.Any() && filteredKeywords.Count != keywords.Count)
                {
                    request = new KeywordSearchRequest
                    {
                        Keywords = string.Join(" ", keywords),
                        Limit = recordCount,
                        Offset = 0,
                        // no filter options specified
                        FilterOptionsRequest = new FilterOptionsRequest { },
                        SortOptions = new SortOptions
                        {
                            Field = SortFields.QuantityAvailable,
                            SortOrder = SortDirection.Descending
                        },
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
                var uri = Url.Combine(_configuration.ApiUrl, $"products/v4/search/{HttpUtility.UrlEncode(partNumber)}/productdetails");
                _logger.LogInformation($"[{nameof(GetProductDetailsAsync)}] Creating product search request for '{partNumber}'...");
                var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Get, uri);
                // perform a keywords API search
                var response = await _client.SendAsync(requestMessage);
                _logger.LogInformation($"[{nameof(GetProductDetailsAsync)}] Api responded with '{response.StatusCode}'");
                var result = await TryHandleResponseAsync(response, authenticationResponse, ApiVersion);
                if (!result.IsSuccessful)
                {
                    // return api error
                    return result.Response;
                }

                // 200 OK
                var responseJson = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<ProductDetails>(responseJson, _serializerSettings) ?? new();
                return new ApiResponse(results, nameof(DigikeyApi));
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<(IApiResponse? ErrorResponse, KeywordSearchResponse SuccessResponse, string JsonRequest, string JsonResponse)> PerformApiSearchQueryAsync(OAuthAuthorization authenticationResponse, Uri uri, KeywordSearchRequest request)
        {
            _logger.LogInformation($"[{nameof(PerformApiSearchQueryAsync)}] Creating search request for '{request.Keywords}' using accesstoken='{authenticationResponse.AccessToken.Sanitize()}'...");
            using var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Post, uri);
            var requestJson = JsonConvert.SerializeObject(request, _serializerSettings);
            requestMessage.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            // perform a keywords API search
            using var response = await _client.SendAsync(requestMessage);
            _logger.LogInformation($"[{nameof(PerformApiSearchQueryAsync)}] Api responded with '{response.StatusCode}'. accesstoken='{authenticationResponse.AccessToken.Sanitize()}'");
            var result = await TryHandleResponseAsync(response, authenticationResponse, ApiVersion);
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

        public async Task<IApiResponse> ProductSearchAsync(OAuthAuthorization authenticationResponse, string partNumber)
        {
            /* important reminder - don't reference authResponse in here! */
            try
            {
                // set what fields we want from the API
                var uri = Url.Combine(_configuration.ApiUrl, $"products/v4/search/{HttpUtility.UrlEncode(partNumber)}/productdetails");
                _logger.LogInformation($"[{nameof(ProductSearchAsync)}] Creating product search request for '{partNumber}'...");
                var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Get, uri);
                // perform a keywords API search
                var response = await _client.SendAsync(requestMessage);
                _logger.LogInformation($"[{nameof(ProductSearchAsync)}] Api responded with '{response.StatusCode}'");
                var result = await TryHandleResponseAsync(response, authenticationResponse, ApiVersion);
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
                var result = await TryHandleResponseAsync(response, authenticationResponse, ApiVersion);
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
                var uri = Url.Combine(_configuration.ApiUrl, "products/v4/search/categories");
                _logger.LogInformation($"[{nameof(GetCategoriesAsync)}] Creating categories request...");
                var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Get, uri);
                // perform a keywords API search
                var response = await _client.SendAsync(requestMessage);
                _logger.LogInformation($"[{nameof(GetCategoriesAsync)}] Api responded with '{response.StatusCode}'");
                var result = await TryHandleResponseAsync(response, authenticationResponse, ApiVersion);
                if (!result.IsSuccessful)
                {
                    // return api error
                    return result.Response;
                }

                // 200 OK
                var responseJson = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<Model.Integrations.DigiKey.V4.CategoriesResponse>(responseJson, _serializerSettings) ?? new();
                return new ApiResponse(results, nameof(DigikeyApi));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
