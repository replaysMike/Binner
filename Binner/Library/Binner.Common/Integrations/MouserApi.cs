using Binner.Common.Extensions;
using Binner.Common.Integrations.Models;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations.Mouser;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Binner.Common.Integrations
{
    public class MouserApi : IIntegrationApi
    {
        private const string BasePath = "/api/v1";
        public string Name => "Mouser";
        private readonly MouserConfiguration _configuration;
        private readonly HttpClient _client;
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            // ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            Converters = new List<JsonConverter> { new StringEnumConverter() }
        };

        public bool IsEnabled => _configuration.Enabled;

        public IApiConfiguration Configuration => _configuration;

        public MouserApi(MouserConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _client = new HttpClient();
        }

        public async Task<IApiResponse> GetOrderAsync(string orderId, Dictionary<string, string>? additionalOptions = null)
        {
            // use the newer order history api, slightly different data format
            var uri = Url.Combine(_configuration.ApiUrl, BasePath, $"/orderhistory/webOrderNumber?webOrderNumber={orderId}&apiKey={_configuration.ApiKeys.OrderApiKey}");
            var requestMessage = CreateRequest(HttpMethod.Get, uri);
            var response = await _client.SendAsync(requestMessage);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return ApiResponse.Create($"Mouser Api returned Unauthorized access - check that your OrderApiKey is correctly configured.", nameof(MouserApi));
            if (response.IsSuccessStatusCode)
            {
                var resultString = response.Content.ReadAsStringAsync().Result;
                var results = JsonConvert.DeserializeObject<OrderHistory>(resultString, _serializerSettings) ?? new();
                /*if (results.Errors.Any())
                    new ApiResponse(results.Errors.Select(x => x.Message ?? string.Empty), nameof(MouserApi));*/
                return new ApiResponse(results, nameof(MouserApi));
            }
            return ApiResponse.Create($"Mouser Api returned error status code {response.StatusCode}: {response.ReasonPhrase}", nameof(MouserApi));
        }

        public async Task<IApiResponse> GetOldOrderAsync(string orderId, Dictionary<string, string>? additionalOptions = null)
        {
            var uri = Url.Combine(_configuration.ApiUrl, BasePath, $"/order/{orderId}?apiKey={_configuration.ApiKeys.OrderApiKey}");
            var requestMessage = CreateRequest(HttpMethod.Get, uri);
            var response = await _client.SendAsync(requestMessage);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return ApiResponse.Create($"Mouser Api returned Unauthorized access - check that your OrderApiKey is correctly configured.", nameof(MouserApi));
            if (response.IsSuccessStatusCode)
            {
                var resultString = response.Content.ReadAsStringAsync().Result;
                var results = JsonConvert.DeserializeObject<Order>(resultString, _serializerSettings) ?? new();
                if (results.Errors.Any())
                    new ApiResponse(results.Errors.Select(x => x.Message ?? string.Empty), nameof(MouserApi));
                return new ApiResponse(results, nameof(MouserApi));
            }
            return ApiResponse.Create($"Mouser Api returned error status code {response.StatusCode}: {response.ReasonPhrase}", nameof(MouserApi));
        }

        /// <summary>
        /// Get information about a mouser part number
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns></returns>
        public async Task<IApiResponse> GetProductDetailsAsync(string partNumber, Dictionary<string, string>? additionalOptions = null)
        {
            var uri = Url.Combine(_configuration.ApiUrl, BasePath, $"/search/partnumber?apiKey={_configuration.ApiKeys.SearchApiKey}");
            var requestMessage = CreateRequest(HttpMethod.Post, uri);
            var request = new
            {
                SearchByPartRequest = new SearchByPartRequest
                {
                    MouserPartNumber = partNumber,
                    PartSearchOptions = PartSearchOptions.BeginsWith
                }
            };
            var json = JsonConvert.SerializeObject(request, _serializerSettings);
            requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.SendAsync(requestMessage);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new MouserUnauthorizedException(response.ReasonPhrase ?? string.Empty);

            if (response.IsSuccessStatusCode)
            {
                var resultString = response.Content.ReadAsStringAsync().Result;
                var results = JsonConvert.DeserializeObject<SearchResultsResponse>(resultString, _serializerSettings) ?? new();
                if (results.Errors?.Any() == true)
                    throw new MouserErrorsException(results.Errors);
                return new ApiResponse(results.SearchResults?.Parts ?? new List<MouserPart>(), nameof(MouserApi));
            }
            return ApiResponse.Create($"Mouser Api returned error status code {response.StatusCode}: {response.ReasonPhrase}", nameof(MouserApi));
        }

        public Task<IApiResponse> SearchAsync(string keyword, int recordCount = 25, Dictionary<string, string>? additionalOptions = null) =>
            SearchAsync(keyword, string.Empty, string.Empty, recordCount, additionalOptions);
       
        public Task<IApiResponse> SearchAsync(string keyword, string partType, int recordCount = 25, Dictionary<string, string>? additionalOptions = null) => SearchAsync(keyword, partType, string.Empty, recordCount, additionalOptions);

        public async Task<IApiResponse> SearchAsync(string keyword, string partType, string mountingType, int recordCount = 25, Dictionary<string, string>? additionalOptions = null)
        {
            if (!(recordCount > 0)) throw new ArgumentOutOfRangeException(nameof(recordCount));
            var uri = Url.Combine(_configuration.ApiUrl, BasePath, $"/search/keyword?apiKey={_configuration.ApiKeys.SearchApiKey}");
            var requestMessage = CreateRequest(HttpMethod.Post, uri);
            var request = new
            {
                SearchByKeywordRequest = new SearchByKeywordRequest
                {
                    Records = recordCount,
                    Keyword = keyword,
                    SearchOptions = SearchOptions.InStock
                }
            };
            var json = JsonConvert.SerializeObject(request, _serializerSettings);
            requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.SendAsync(requestMessage);
            if (TryHandleResponse(response, out var apiResponse))
            {
                return apiResponse;
            }

            // 200 OK
            var resultString = response.Content.ReadAsStringAsync().Result;
            var results = JsonConvert.DeserializeObject<SearchResultsResponse>(resultString, _serializerSettings) ?? new();
            if (results.Errors?.Any() == true)
                throw new MouserErrorsException(results.Errors);
            return new ApiResponse(results, nameof(MouserApi));
        }

        /// <summary>
        /// Handle known error conditions first, if response is OK false will be returned
        /// </summary>
        /// <param name="response"></param>
        /// <param name="apiResponse"></param>
        /// <returns></returns>
        /// <exception cref="DigikeyUnauthorizedException"></exception>
        private bool TryHandleResponse(HttpResponseMessage response,out IApiResponse apiResponse)
        {
            apiResponse = apiResponse = ApiResponse.Create($"Api returned error status code {response.StatusCode}: {response.ReasonPhrase}", nameof(MouserApi));
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new MouserUnauthorizedException(response?.ReasonPhrase ?? string.Empty);
            else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                if (response.Headers.Contains("X-RateLimit-Limit"))
                {
                    // throttled
                    var remainingTime = TimeSpan.Zero;
                    if (response.Headers.Contains("X-RateLimit-Remaining"))
                        remainingTime = TimeSpan.FromSeconds(int.Parse(response.Headers.GetValues("X-RateLimit-Remaining").First()));
                    apiResponse = ApiResponse.Create($"{nameof(MouserApi)} request throttled. Try again in {remainingTime}", nameof(MouserApi));
                    return true;
                }

                // return generic error
                return true;
            }
            else if (response.IsSuccessStatusCode)
            {
                // allow processing of response
                return false;
            }

            // return generic error
            return true;
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, Uri uri)
        {
            var message = new HttpRequestMessage(method, uri);
            return message;
        }
    }

    public class MouserErrorsException : Exception
    {
        public ICollection<Error> Errors { get; set; }
        public MouserErrorsException(ICollection<Error> errors) : base(errors.FirstOrDefault()?.Message)
        {
            Errors = errors;
        }
    }

    public class MouserUnauthorizedException : Exception
    {
        public MouserUnauthorizedException(string message) : base(message)
        {

        }
    }
}
