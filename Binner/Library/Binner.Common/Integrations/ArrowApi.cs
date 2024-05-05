using Binner.Common.Extensions;
using Binner.Common.Integrations.Models;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations.Arrow;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Binner.Common.Integrations
{
    public partial class ArrowApi : IIntegrationApi
    {
        private const string BasePath = "";
        public string Name => "Arrow";
        private readonly ArrowConfiguration _configuration;
        private readonly HttpClient _client;
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            Converters = new List<JsonConverter> { new StringEnumConverter() }
        };

        public bool IsEnabled => _configuration.Enabled;

        public IApiConfiguration Configuration => _configuration;

        public ArrowApi(ArrowConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpContextAccessor = httpContextAccessor;
            _client = new HttpClient();
        }

        public Task<IApiResponse> SearchAsync(string keyword, int recordCount = 25, Dictionary<string, string>? additionalOptions = null) => SearchAsync(keyword, string.Empty, string.Empty, recordCount, additionalOptions);

        public Task<IApiResponse> SearchAsync(string keyword, string partType, int recordCount = 25, Dictionary<string, string>? additionalOptions = null) => SearchAsync(keyword, partType, string.Empty, recordCount, additionalOptions);

        public async Task<IApiResponse> SearchAsync(string keyword, string partType, string mountingType, int recordCount = 25, Dictionary<string, string>? additionalOptions = null)
        {
            if (string.IsNullOrEmpty(_configuration.Username)) throw new BinnerConfigurationException("ArrowConfiguration must specify a Username!");
            if (string.IsNullOrEmpty(_configuration.ApiKey)) throw new BinnerConfigurationException("ArrowConfiguration must specify a ApiKey!");
            if (string.IsNullOrEmpty(_configuration.ApiUrl)) throw new BinnerConfigurationException("ArrowConfiguration must specify a ApiUrl!");

            if (!(recordCount > 0)) throw new ArgumentOutOfRangeException(nameof(recordCount));
            
            var recordStart = 0;
            var uri = Url.Combine(_configuration.ApiUrl, BasePath, $"/itemservice/v4/en/search/token?login={_configuration.Username}&apiKey={_configuration.ApiKey}&start={recordStart}&rows={recordCount}&search_token={keyword}");
            var requestMessage = CreateRequest(HttpMethod.Get, uri);
            var response = await _client.SendAsync(requestMessage);
            if (TryHandleResponse(response, out var apiResponse))
            {
                return apiResponse;
            }

            // 200 OK
            var resultString = response.Content.ReadAsStringAsync().Result;
            //resultString = FakeResults.SearchResult1;

            var results = JsonConvert.DeserializeObject<ArrowResponse>(resultString, _serializerSettings) ?? new();
            return new ApiResponse(results, nameof(ArrowApi));
        }

        public async Task<IApiResponse> GetOrderAsync(string orderId, Dictionary<string, string>? additionalOptions = null)
        {
            if (string.IsNullOrEmpty(_configuration.Username)) throw new BinnerConfigurationException($"{nameof(ArrowConfiguration)} must specify a Username!");
            if (string.IsNullOrEmpty(_configuration.ApiKey)) throw new BinnerConfigurationException($"{nameof(ArrowConfiguration)} must specify a ApiKey!");
            if (string.IsNullOrEmpty(_configuration.ApiUrl)) throw new BinnerConfigurationException($"{nameof(ArrowConfiguration)} must specify a ApiUrl!");
            if (additionalOptions == null) throw new ArgumentNullException(nameof(additionalOptions));
            if (!additionalOptions.ContainsKey("password") || string.IsNullOrEmpty(additionalOptions["password"])) throw new ArgumentNullException(nameof(additionalOptions), "User password value is required!");
            var username = _configuration.Username;
            if (additionalOptions.ContainsKey("username") && !string.IsNullOrEmpty(additionalOptions["username"]))
                username = additionalOptions["username"];
            
            var passwordHash = Sha256Hash(additionalOptions["password"]);
            // the orders api has it's own dedicated url path, so we don't bother to make this configurable
            var uri = Url.Combine("https://www.arrow.com", BasePath, $"/services-cc/automatedCheckout/checkOrderStatus?username={username}&password={passwordHash}&format=json");
            var requestMessage = CreateRequest(HttpMethod.Post, uri);
            var response = await _client.SendAsync(requestMessage);
            if (TryHandleResponse(response, out var apiResponse))
            {
                return apiResponse;
            }

            // 200 OK
            var resultString = response.Content.ReadAsStringAsync().Result;
            var results = JsonConvert.DeserializeObject<OrderResponse>(resultString, _serializerSettings) ?? new();
            return new ApiResponse(results, nameof(ArrowApi));
        }

        public Task<IApiResponse> GetProductDetailsAsync(string partNumber, Dictionary<string, string>? additionalOptions = null)
        {
            throw new NotImplementedException();
        }

        private string Sha256Hash(string value)
        {
            var builder = new StringBuilder();
            using (var hash = SHA256.Create())            
            {
                var enc = Encoding.UTF8;
                var result = hash.ComputeHash(enc.GetBytes(value));

                foreach (var b in result)
                    builder.Append(b.ToString("X2"));
            }

            return builder.ToString();
        }

        private bool TryHandleResponse(HttpResponseMessage response,out IApiResponse apiResponse)
        {
            apiResponse = apiResponse = ApiResponse.Create($"Api returned error status code {response.StatusCode}: {response.ReasonPhrase}", nameof(ArrowApi));
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new ArrowUnauthorizedException(response?.ReasonPhrase ?? string.Empty);
            else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                if (response.Headers.Contains("X-RateLimit-Limit"))
                {
                    // throttled
                    var remainingTime = TimeSpan.Zero;
                    if (response.Headers.Contains("X-RateLimit-Remaining"))
                        remainingTime = TimeSpan.FromSeconds(int.Parse(response.Headers.GetValues("X-RateLimit-Remaining").First()));
                    apiResponse = ApiResponse.Create($"{nameof(ArrowApi)} request throttled. Try again in {remainingTime}", nameof(ArrowApi));
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

            var resultString = response.Content.ReadAsStringAsync().Result;
            // return generic error
            return true;
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, Uri uri)
        {
            var message = new HttpRequestMessage(method, uri);
            return message;
        }

        public class ArrowUnauthorizedException : Exception
        {
            public ArrowUnauthorizedException(string message) : base(message)
            {

            }
        }
    }
}
