using Binner.Common.Extensions;
using Binner.Common.Integrations.Models;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations.Tme;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Binner.Common.Integrations
{
    public class TmeApi : IIntegrationApi
    {
        public string Name => "TME";
        public const string BasePath = "/";
        public const string DefaultApiUrl = "https://api.tme.eu/";
        private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(30);
        private readonly ILogger<TmeApi> _logger;
        private readonly TmeConfiguration _configuration;
        private readonly LocaleConfiguration _localeConfiguration;
        private readonly IApiHttpClient _client;
        private readonly IApiHttpClientFactory _clientFactory;
        private readonly MemoryCache _cache = new MemoryCache("TMEStaticCache");

        public bool IsEnabled => _configuration.Enabled;

        public IApiConfiguration Configuration => _configuration;

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            // ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            Converters = new List<JsonConverter> { new StringEnumConverter() }
        };

        /// <summary>
        /// TME Api
        /// Documentation available at https://developers.tme.eu/documentation/download
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="localeConfiguration"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public TmeApi(ILogger<TmeApi> logger, TmeConfiguration configuration, LocaleConfiguration localeConfiguration, IApiHttpClientFactory clientFactory)
        {
            _logger = logger;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _localeConfiguration = localeConfiguration ?? throw new ArgumentNullException(nameof(localeConfiguration));
            _clientFactory = clientFactory;
            _client = clientFactory.Create();
            _client.AddHeader(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public Task<IApiResponse> GetOrderAsync(string orderId, Dictionary<string, string>? additionalOptions = null)
        {
            throw new NotImplementedException();
        }

        public Task<IApiResponse> GetProductDetailsAsync(string partNumber, Dictionary<string, string>? additionalOptions = null)
        {
            throw new NotImplementedException();
        }

        public Task<IApiResponse> SearchAsync(string keyword, int recordCount = 20, Dictionary<string, string>? additionalOptions = null) => SearchAsync(keyword, string.Empty, string.Empty, recordCount, additionalOptions);

        public Task<IApiResponse> SearchAsync(string keyword, string partType, int recordCount = 20, Dictionary<string, string>? additionalOptions = null) => SearchAsync(keyword, partType, string.Empty, recordCount, additionalOptions);

        public async Task<IApiResponse> SearchAsync(string partNumber, string partType, string mountingType, int recordCount = 20, Dictionary<string, string>? additionalOptions = null)
        {
            // Products/Search
            ValidateConfiguration();

            if (!(recordCount > 0)) throw new ArgumentOutOfRangeException(nameof(recordCount));

            // note: PageSize = 20 and cannot be altered with the TME api. Only PageNumber (SearchPage) can be specified.
            var format = "json";
            var prefix = "Products";
            var action = "Search";
            var path = $"{prefix}/{action}.{format}";
            var uri = Url.Combine(GetApiUrl(), BasePath, path);

            // build the api params request
            // Text describing the searched product, may consist of multiple words. Example: "led diode","cover", "1N4007". (optional)
            var apiParams = new Dictionary<string, object> {
                { "SearchPlain", partNumber },
                { "SearchOrder", SearchOrder.ACCURACY.ToString() },
                { "SearchOrderType", SearchDirection.ASC.ToString() },
                { "SearchPage", 1 },
            };
            //apiParams.Add("SearchWithStock", "false");
            // Category identifier in which the products should be searched, such as "100328". (optional)
            //apiParams.Add("SearchCategory", partType); 
            // Parameter by which search results are filtered - SearchParameter[PARAMETER_ID][] = VALUE_ID (optional)
            //apiParams.Add("SearchParameter", mountingType); 
            // This param allows to filter products with stock only. Filtering occurs according to data that can differ from actual data displayed on www.tme.eu(optional)
            //apiParams.Add("SearchWithStock", mountingType); 
            // Parameter determines value type by which results will be sorted. Possible values ACCURACY, SYMBOL, ORIGINAL_SYMBOL, PRICE_FIRST_QUANTITY, PRICE_LAST_QUANTITY (optional)
            // apiParams.Add("SearchOrder", "ACCURACY");
            // Parameter which determines direction of sorting (ASC or DESC) (optional)
            //apiParams.Add("SearchOrderType", "ASC");
            var urlEncodedContent = await BuildApiParamsAsync(uri, _configuration.Country, TmeLanguages.MapLanguage(_localeConfiguration.Language), apiParams);

            // create POST message
            var requestMessage = CreateRequest(HttpMethod.Post, uri, urlEncodedContent);

            var response = await _client.SendAsync(requestMessage);
            var result = await TryHandleResponseAsync(response);
            if (!result.IsSuccessful)
            {
                return result.ApiResponse;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            // workaround fix for TME's API producing invalid/mutating json, tracked issue #360 created on their end (4/25/2025)
            responseJson = responseJson.Replace("\"CategoryList\":[]", "\"CategoryList\":{}");

            var results = JsonConvert.DeserializeObject<TmeResponse<ProductSearchResponse>>(responseJson, _serializerSettings) ?? new();
            return new ApiResponse(results, nameof(TmeApi));
        }

        /// <summary>
        /// Get a list of files for a list of part numbers
        /// </summary>
        /// <param name="partNumbers"></param>
        /// <returns></returns>
        public async Task<IApiResponse> GetProductFilesAsync(List<string> partNumbers)
        {
            // Products/GetProductsFiles
            ValidateConfiguration();

            var format = "json";
            var prefix = "Products";
            var action = "GetProductsFiles";
            var path = $"{prefix}/{action}.{format}";
            var uri = Url.Combine(GetApiUrl(), BasePath, path);

            // build the api params request
            // Text describing the searched product, may consist of multiple words. Example: "led diode","cover", "1N4007". (optional)
            var apiParams = new Dictionary<string, object>();
            var i = 0;
            foreach(var partNumber in partNumbers)
            {
                // api supports a maximum of 50 symbols to be passed
                if (i >= 50)
                    break;
                apiParams.Add($"SymbolList[{i}]", partNumber);
                i++;
            }

            var urlEncodedContent = await BuildApiParamsAsync(uri, _configuration.Country, TmeLanguages.MapLanguage(_localeConfiguration.Language), apiParams);

            // create POST message
            var requestMessage = CreateRequest(HttpMethod.Post, uri, urlEncodedContent);

            var response = await _client.SendAsync(requestMessage);
            var result = await TryHandleResponseAsync(response);
            if (!result.IsSuccessful)
            {
                return result.ApiResponse;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<TmeResponse<ProductFilesResponse>>(responseJson, _serializerSettings) ?? new();
            return new ApiResponse(results, nameof(TmeApi));
        }

        /// <summary>
        /// Get pricing information for a list of part numbers
        /// </summary>
        /// <param name="partNumbers"></param>
        /// <returns></returns>
        public async Task<IApiResponse> GetProductPricesAsync(List<string> partNumbers)
        {
            // Products/GetProductsFiles
            ValidateConfiguration();

            var format = "json";
            var prefix = "Products";
            var action = "GetPricesAndStocks";
            var path = $"{prefix}/{action}.{format}";
            var uri = Url.Combine(GetApiUrl(), BasePath, path);

            // build the api params request
            // Text describing the searched product, may consist of multiple words. Example: "led diode","cover", "1N4007". (optional)
            var apiParams = new Dictionary<string, object>();
            apiParams.Add("Currency", _localeConfiguration.Currency);
            apiParams.Add("GrossPrices", false);
            var i = 0;
            foreach (var partNumber in partNumbers)
            {
                // api supports a maximum of 50 symbols to be passed
                if (i >= 50)
                    break;
                apiParams.Add($"SymbolList[{i}]", partNumber);
                i++;
            }

            var urlEncodedContent = await BuildApiParamsAsync(uri, _configuration.Country, TmeLanguages.MapLanguage(_localeConfiguration.Language), apiParams);

            // create POST message
            var requestMessage = CreateRequest(HttpMethod.Post, uri, urlEncodedContent);

            var response = await _client.SendAsync(requestMessage);
            var result = await TryHandleResponseAsync(response);
            if (!result.IsSuccessful)
            {
                return result.ApiResponse;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<TmeResponse<PriceListResponse>>(responseJson, _serializerSettings) ?? new();
            return new ApiResponse(results, nameof(TmeApi));
        }

        public async Task<IApiResponse> GetCategoriesAsync()
        {
            ValidateConfiguration();

            var format = "json";
            var prefix = "Products";
            var action = "GetCategories";
            var path = $"{prefix}/{action}.{format}";
            var uri = Url.Combine(GetApiUrl(), BasePath, path);

            // build the api params request
            var isTree = false;
            // Text describing the searched product, may consist of multiple words. Example: "led diode","cover", "1N4007". (optional)
            var apiParams = new Dictionary<string, object> { { "Tree", isTree } };
            // Optional ID of category that'll narrow action result to it and its children.
            //apiParams.Add("CategoryId", 1);
            // Determines form of response. If true then tree will be returned. Param is optional, default - true.
            //apiParams.Add("Tree", false);
            var urlEncodedContent = await BuildApiParamsAsync(uri, _configuration.Country, TmeLanguages.MapLanguage(_localeConfiguration.Language), apiParams);

            // create POST message
            var requestMessage = CreateRequest(HttpMethod.Post, uri, urlEncodedContent);

            var response = await _client.SendAsync(requestMessage);
            var result = await TryHandleResponseAsync(response);
            if (!result.IsSuccessful)
            {
                return result.ApiResponse;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            if (isTree)
            {
                var results = JsonConvert.DeserializeObject<TmeResponse<CategoryTreeResponse>>(responseJson, _serializerSettings) ?? new();
                return new ApiResponse(results, nameof(TmeApi));
            }
            else
            {
                var results = JsonConvert.DeserializeObject<TmeResponse<CategoryResponse>>(responseJson, _serializerSettings) ?? new();

#pragma warning disable CS0162 // Unreachable code detected
                // produce a hashtable of all categories, which will be statically compiled into the StaticCategories class.
                if (false)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("private static Dictionary<int, Category> _categories = new Dictionary<int, Category>() {");
                    foreach (var category in results.Data!.CategoryTree)
                    {
                        sb.AppendLine($@"    {{{category.Id}, new Category {{ Id = {category.Id}, ParentId = {category.ParentId}, Name = ""{category.Name.Replace("\"", "\\\"")}"" }} }},");
                    }
                    sb.AppendLine("};");
                    var hashTable = sb.ToString();
                    Console.WriteLine(hashTable);
                }
#pragma warning restore CS0162 // Unreachable code detected

                return new ApiResponse(results, nameof(TmeApi));
            }
        }

        public async Task<string> ResolveExternalLinkAsync(TmeDocument document)
        {
            if (_configuration.ResolveExternalLinks)
            {
                var cachekey = $"LNK-{document.DocumentUrl}";
                if (document.DocumentType == DocumentTypes.LNK)
                {
                    var cachedValue = _cache.GetCacheItem(cachekey);
                    if (cachedValue != null && cachedValue.Value is string) return (string)cachedValue.Value;

                    var uri = new Uri($"https:{document.DocumentUrl}");
                    var client = _clientFactory.Create();
                    client.SetTimeout(TimeSpan.FromSeconds(5));
                    client.ClearHeaders();
                    Activity.Current = null; // remove traceparent header (thanks .net)
                    client.AddHeader("Host", uri.Host);
                    client.AddHeader("User-Agent", "Binner/1.0");
                    var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri); // tme omits the protocol from the url
                    var response = await client.SendAsync(requestMessage);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        _cache.Add(cachekey, responseJson, DateTimeOffset.UtcNow.Add(CacheLifetime));
                        return responseJson;
                    }
                }
            }
            return string.Empty;
        }

        private async Task<FormUrlEncodedContent> BuildApiParamsAsync(Uri uri, string country, string language, Dictionary<string, object> parameters)
        {
            if (string.IsNullOrWhiteSpace(_configuration.ApiKey))
                throw new ArgumentException("ApiKey is empty!");
            if (string.IsNullOrWhiteSpace(_configuration.ApplicationSecret))
                throw new ArgumentException("ApplicationSecret is empty!");
            if (string.IsNullOrWhiteSpace(country))
                country = "US";
            if (string.IsNullOrWhiteSpace(language))
                language = "EN";

            var apiParams = new Dictionary<string, object>();
            // 2 letter country
            apiParams.Add("Country", country.ToUpper());
            // 2 letter language identifier
            apiParams.Add("Language", language.ToUpper());

            foreach (var kvp in parameters)
                apiParams.Add(kvp.Key, kvp.Value);

            // anonymous key or private token
            apiParams.Add("Token", _configuration.ApiKey);

            // Encode and normalize params
            // values must be sorted by key ASC, or the remote API will fail to match the signature
            var ordered = apiParams
                .OrderByNaturalSort(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value.ToString());
            var urlEncodedContent = new FormUrlEncodedContent(ordered);
            var encodedParams = await urlEncodedContent.ReadAsStringAsync();

            // Calculate signature basis according the documentation
            var url = uri.ToString();
            var escapedUri = UrlEncode(url);
            var escapedParams = UrlEncode(encodedParams);
            var signatureBase = $"POST&{escapedUri}&{escapedParams}";

            // Calculate HMAC-SHA1 from signature and encode by Base64 function
            var hmacSha1 = HashHmac(signatureBase, _configuration.ApplicationSecret);
            var apiSignature = Convert.ToBase64String(hmacSha1);

            // Add ApiSignature to params
            apiParams.Add("ApiSignature", apiSignature);

            // resort keys
            ordered = apiParams
                .OrderByNaturalSort(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value.ToString());
            return new FormUrlEncodedContent(ordered);
        }

        private async Task<(bool IsSuccessful, IApiResponse ApiResponse)> TryHandleResponseAsync(HttpResponseMessage response)
        {
            IApiResponse apiResponse = ApiResponse.Create($"Api returned error status code {response.StatusCode}: {response.ReasonPhrase}", nameof(TmeApi));
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new TmeUnauthorizedException(response?.ReasonPhrase ?? string.Empty);
            else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                if (response.Headers.Contains("X-RateLimit-Limit"))
                {
                    // throttled
                    var remainingTime = TimeSpan.Zero;
                    if (response.Headers.Contains("X-RateLimit-Remaining"))
                        remainingTime = TimeSpan.FromSeconds(int.Parse(response.Headers.GetValues("X-RateLimit-Remaining").First()));
                    apiResponse = ApiResponse.Create($"{nameof(TmeApi)} request throttled. Try again in {remainingTime}", nameof(TmeApi));
                    return (false, apiResponse);
                }

                // return generic error
                return (false, apiResponse);
            }
            else if (response.IsSuccessStatusCode)
            {
                // allow processing of response
                return (true, apiResponse);
            }

            var resultString = await response.Content.ReadAsStringAsync();
            // return generic error
            return (false, apiResponse);
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, Uri uri, FormUrlEncodedContent content)
        {
            var message = new HttpRequestMessage(method, uri);
            message.Content = content;
            message.Headers.Add("Accept", "application/json");
            return message;
        }

        private byte[] HashHmac(string input, string key)
        {
            var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(key));
            byte[] byteArray = Encoding.ASCII.GetBytes(input);
            return hmac.ComputeHash(byteArray);
        }

        private string UrlEncode(string s)
        {
            // This function uses uppercase in escaped chars to be compatible with the documentation

            // Input: https://api.tme.eu/Products/GetParameters.json
            // https%3a%2f%2fapi.tme.eu%2fProducts%2fGetParameters.json - HttpUtility.UrlEncode
            // https%3A%2F%2Fapi.tme.eu%2FProducts%2FGetParameters.json - result
            // %3a => %3A ...

            var temp = System.Web.HttpUtility.UrlEncode(s).ToCharArray();
            for (var i = 0; i < temp.Length - 2; i++)
            {
                if (temp[i] == '%')
                {
                    temp[i + 1] = char.ToUpper(temp[i + 1]);
                    temp[i + 2] = char.ToUpper(temp[i + 2]);
                }
            }
            return new string(temp);
        }

        private string GetApiUrl() => string.IsNullOrEmpty(_configuration.ApiUrl) ? DefaultApiUrl : _configuration.ApiUrl;

        private void ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(_configuration.ApplicationSecret)) throw new BinnerConfigurationException($"{nameof(TmeConfiguration)} must specify a ApplicationSecret!");
            if (string.IsNullOrEmpty(_configuration.ApiKey)) throw new BinnerConfigurationException($"{nameof(TmeConfiguration)} must specify a ApiKey!");
            if (string.IsNullOrEmpty(_configuration.ApiUrl)) throw new BinnerConfigurationException($"{nameof(TmeConfiguration)} must specify a ApiUrl!");
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public override string ToString()
            => $"{nameof(TmeApi)}";

        public class TmeUnauthorizedException : Exception
        {
            public TmeUnauthorizedException(string message) : base(message)
            {

            }
        }
    }
}
