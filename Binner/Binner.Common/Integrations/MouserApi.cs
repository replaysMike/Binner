using Binner.Common.Extensions;
using Binner.Common.Integrations.Models;
using Binner.Common.Integrations.Models.Mouser;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
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
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly HttpClient _client;
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            // ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            Converters = new List<JsonConverter> { new StringEnumConverter() }
        };

        public bool IsConfigured => !string.IsNullOrEmpty(_apiKey) && !string.IsNullOrEmpty(_apiUrl);

        public MouserApi(string apiKey, string apiUrl, IHttpContextAccessor httpContextAccessor)
        {
            _apiKey = apiKey;
            _apiUrl = apiUrl;
            _httpContextAccessor = httpContextAccessor;
            _client = new HttpClient();
        }

        public async Task<IApiResponse> GetOrderAsync(string orderId)
        {
            var uri = Url.Combine(_apiUrl, BasePath, $"/search/order/{orderId}?apiKey={_apiKey}");
            var requestMessage = CreateRequest(HttpMethod.Get, uri);
            var response = await _client.SendAsync(requestMessage);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new MouserUnauthorizedException(response.ReasonPhrase);
            if (response.IsSuccessStatusCode)
            {
                var resultString = response.Content.ReadAsStringAsync().Result;
                var results = JsonConvert.DeserializeObject<Order>(resultString, _serializerSettings);
                if (results.Errors.Any())
                    new ApiResponse(results.Errors.Select(x => x.Message), nameof(MouserApi));
                return new ApiResponse(results, nameof(MouserApi));
            }
            return ApiResponse.Create($"Api returned error status code {response.StatusCode}: {response.ReasonPhrase}", nameof(MouserApi));
        }


        public async Task<IApiResponse> GetPartsAsync(string partNumber, string partType = "", string mountingType = "")
        {
            var uri = Url.Combine(_apiUrl, BasePath, $"/search/partnumber?apiKey={_apiKey}");
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
                throw new MouserUnauthorizedException(response.ReasonPhrase);

            if (response.IsSuccessStatusCode)
            {
                var resultString = response.Content.ReadAsStringAsync().Result;
                var results = JsonConvert.DeserializeObject<SearchResultsResponse>(resultString, _serializerSettings);
                if (results.Errors.Any())
                    new ApiResponse(results.Errors.Select(x => x.Message), nameof(MouserApi));
                return new ApiResponse(results.SearchResults.Parts, nameof(MouserApi));
            }
            return ApiResponse.Create($"Api returned error status code {response.StatusCode}: {response.ReasonPhrase}", nameof(MouserApi));
        }
        public async Task<IApiResponse> SearchAsync(string keyword)
        {
            var uri = Url.Combine(_apiUrl, BasePath, $"/search/keyword?apiKey={_apiKey}");
            var requestMessage = CreateRequest(HttpMethod.Post, uri);
            var request = new
            {
                SearchByKeywordRequest = new SearchByKeywordRequest
                {
                    Keyword = keyword,
                    SearchOptions = SearchOptions.InStock
                }
            };
            var json = JsonConvert.SerializeObject(request, _serializerSettings);
            requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.SendAsync(requestMessage);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new MouserUnauthorizedException(response.ReasonPhrase);

            if (response.IsSuccessStatusCode)
            {
                var resultString = response.Content.ReadAsStringAsync().Result;
                var results = JsonConvert.DeserializeObject<SearchResultsResponse>(resultString, _serializerSettings);
                if (results.Errors.Any())
                    throw new MouserErrorsException(results.Errors);
                return new ApiResponse(results.SearchResults.Parts, nameof(MouserApi));
            }
            return null;
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
        public MouserErrorsException(ICollection<Error> errors)
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
