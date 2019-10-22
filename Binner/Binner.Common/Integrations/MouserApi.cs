using Binner.Common.Extensions;
using Binner.Common.Integrations.Models.Mouser;
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
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly HttpClient _client;
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            // ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            Converters = new List<JsonConverter> { new StringEnumConverter() }
        };

        public bool IsConfigured => !string.IsNullOrEmpty(_apiKey) && !string.IsNullOrEmpty(_apiUrl);

        public MouserApi(string apiKey, string apiUrl)
        {
            _apiKey = apiKey;
            _apiUrl = apiUrl;
            _client = new HttpClient();
        }

        public async Task<ICollection<MouserPart>> GetPartsAsync(string partNumber, string partType = "", string packageType = "")
        {
            var uri = Url.Combine(_apiUrl, BasePath, $"/search/partnumber?apiKey={_apiKey}");
            var requestMessage = CreateRequest(HttpMethod.Post, uri);
            var request = new {
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
                    throw new MouserErrorsException(results.Errors);
                return results.SearchResults.Parts;
            }
            return null;
        }
        public async Task<ICollection<MouserPart>> SearchAsync(string keyword)
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
                return results.SearchResults.Parts;
            }
            return null;
        }

        public async Task<ICollection<object>> BuildCartAsync()
        {
            return null;
        }

        public async Task<ICollection<object>> GetOrderAsync()
        {
            return null;
        }

        public async Task<ICollection<object>> CreateOrderAsync()
        {
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
