﻿using Binner.Common;
using Binner.Common.Extensions;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations;
using Binner.Model.Integrations.Mouser;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text;

namespace Binner.Services.Integrations
{
    public class MouserApi : IIntegrationApi
    {
        private const string BasePath = "/api/v1";
        public string Name => "Mouser";
        private readonly ILogger<MouserApi> _logger;
        private readonly MouserConfiguration _configuration;
        private readonly UserConfiguration _userConfiguration;
        private readonly IApiHttpClient _client;
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            // ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            Converters = new List<JsonConverter> { new StringEnumConverter() }
        };

        public bool IsEnabled => _configuration.Enabled;

        public IApiConfiguration Configuration => _configuration;

        public MouserApi(ILogger<MouserApi> logger, MouserConfiguration configuration, UserConfiguration userConfiguration, IApiHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _userConfiguration = userConfiguration;
            _client = httpClientFactory.Create();
        }

        private void ValidateOrderConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_configuration.ApiKeys.OrderApiKey)) throw new BinnerConfigurationException("Mouser API OrderApiKey cannot be empty!");
            if (string.IsNullOrWhiteSpace(_configuration.ApiUrl)) throw new BinnerConfigurationException("Mouser API ApiUrl cannot be empty!");
        }

        private void ValidateSearchConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_configuration.ApiKeys.SearchApiKey)) throw new BinnerConfigurationException("Mouser API SearchApiKey cannot be empty!");
            if (string.IsNullOrWhiteSpace(_configuration.ApiUrl)) throw new BinnerConfigurationException("Mouser API ApiUrl cannot be empty!");
        }

        public async Task<IApiResponse> GetOrderAsync(string orderId, Dictionary<string, string>? additionalOptions = null)
        {
            ValidateOrderConfiguration();

            // use the newer order history api, slightly different data format
            var uri = Url.Combine(_configuration.ApiUrl, BasePath, $"/orderhistory/webOrderNumber?webOrderNumber={orderId}&apiKey={_configuration.ApiKeys.OrderApiKey}");
            var requestMessage = CreateRequest(HttpMethod.Get, uri);
            var response = await _client.SendAsync(requestMessage);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return ApiResponse.Create($"Mouser Api returned Unauthorized access - check that your OrderApiKey is correctly configured.", nameof(MouserApi));
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<OrderHistory>(responseJson, _serializerSettings) ?? new();
                /*if (results.Errors.Any())
                    new ApiResponse(results.Errors.Select(x => x.Message ?? string.Empty), nameof(MouserApi));*/
                return new ApiResponse(results, nameof(MouserApi));
            }
            return ApiResponse.Create($"Mouser Api returned error status code {response.StatusCode}: {response.ReasonPhrase}", nameof(MouserApi));
        }

        public async Task<IApiResponse> GetOldOrderAsync(string orderId, Dictionary<string, string>? additionalOptions = null)
        {
            ValidateOrderConfiguration();

            var uri = Url.Combine(_configuration.ApiUrl, BasePath, $"/order/{orderId}?apiKey={_configuration.ApiKeys.OrderApiKey}");
            var requestMessage = CreateRequest(HttpMethod.Get, uri);
            var response = await _client.SendAsync(requestMessage);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return ApiResponse.Create($"Mouser Api returned Unauthorized access - check that your OrderApiKey is correctly configured.", nameof(MouserApi));
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<Order>(responseJson, _serializerSettings) ?? new();
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
            ValidateSearchConfiguration();

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
            var requestJson = JsonConvert.SerializeObject(request, _serializerSettings);
            requestMessage.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var response = await _client.SendAsync(requestMessage);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new MouserUnauthorizedException(response.ReasonPhrase ?? string.Empty);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<SearchResultsResponse>(responseJson, _serializerSettings) ?? new();
                if (results.Errors?.Any() == true)
                    throw new MouserErrorsException(results.Errors);
                return new ApiResponse(results.SearchResults?.Parts ?? new List<MouserPart>(), nameof(MouserApi));
            }
            return ApiResponse.Create($"Mouser Api returned error status code {response.StatusCode}: {response.ReasonPhrase}", nameof(MouserApi));
        }

        public Task<IApiResponse> SearchAsync(string keyword, int recordCount = ApiConstants.MaxRecords, Dictionary<string, string>? additionalOptions = null) =>
            SearchAsync(keyword, string.Empty, string.Empty, recordCount, additionalOptions);
       
        public Task<IApiResponse> SearchAsync(string keyword, string partType, int recordCount = ApiConstants.MaxRecords, Dictionary<string, string>? additionalOptions = null) => SearchAsync(keyword, partType, string.Empty, recordCount, additionalOptions);

        public async Task<IApiResponse> SearchAsync(string keyword, string partType, string mountingType, int recordCount = ApiConstants.MaxRecords, Dictionary<string, string>? additionalOptions = null)
        {
            ValidateSearchConfiguration();
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
            var requestJson = JsonConvert.SerializeObject(request, _serializerSettings);
            requestMessage.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var response = await _client.SendAsync(requestMessage);
            if (TryHandleResponse(response, out var apiResponse))
            {
                return apiResponse;
            }

            // 200 OK
            var responseJson = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<SearchResultsResponse>(responseJson, _serializerSettings) ?? new();
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

        public void Dispose()
        {
            _client.Dispose();
        }

        public override string ToString()
            => $"{nameof(MouserApi)}";

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
