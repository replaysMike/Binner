using Binner.Common;
using Binner.Common.Extensions;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations;
using Binner.Model.Integrations.Element14;
using Binner.Services.Integrations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Binner.Services.Integrations
{
    public class Element14Api : IIntegrationApi
    {
        public string Name => "Element14";
        public const string DefaultApiUrl = "https://api.element14.com/";
        private readonly ILogger<Element14Api> _logger;
        private readonly Element14Configuration _configuration;
        private readonly UserConfiguration _userConfiguration;
        private readonly IApiHttpClient _client;
        private readonly IApiHttpClientFactory _clientFactory;

        public bool IsEnabled => _configuration.Enabled;

        public IApiConfiguration Configuration => _configuration;

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = new List<JsonConverter> { new StringEnumConverter() }
        };

        /// <summary>
        /// Element14 Api
        /// Documentation available at https://partner.element14.com/
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="localeConfiguration"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public Element14Api(ILogger<Element14Api> logger, Element14Configuration configuration, UserConfiguration userConfiguration, IApiHttpClientFactory clientFactory)
        {
            _logger = logger;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userConfiguration = userConfiguration ?? throw new ArgumentNullException(nameof(userConfiguration));
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

            var prefix = "catalog";
            var action = "products";
            var count = recordCount.ToString();
            var param = (
                "versionNumber=1.4" +
                $"&storeInfo.id={_configuration.Country}" +
                $"&term=any:{partNumber}" +
                $"&resultsSettings.numberOfResults={count}" +
                "&resultsSettings.offset=0" +
                "&callInfo.omitXmlSchema=false" +
                "&resultsSettings.responseGroup=large" +
                "&callInfo.responseDataFormat=json" +
                $"&callinfo.apiKey={_configuration.ApiKey}"
            );

            var path = $"{prefix}/{action}?{param}";
            var uri = Url.Combine(GetApiUrl(), path);

            // create GET message
            var requesElement14ssage = CreateRequest(HttpMethod.Get, uri);

            var response = await _client.SendAsync(requesElement14ssage);
            var result = await TryHandleResponseAsync(response);
            if (!result.IsSuccessful)
            {
                return result.ApiResponse;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<Element14SearchResult>(responseJson, _serializerSettings) ?? new();
            return new ApiResponse(results, nameof(Element14Api));
        }

        private async Task<(bool IsSuccessful, IApiResponse ApiResponse)> TryHandleResponseAsync(HttpResponseMessage response)
        {
            IApiResponse apiResponse = ApiResponse.Create($"Api returned error status code {response.StatusCode}: {response.ReasonPhrase}", nameof(Element14Api));
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new Element14UnauthorizedException(response?.ReasonPhrase ?? string.Empty);
            else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                if (response.Headers.Contains("X-RateLimit-Limit"))
                {
                    // throttled
                    var remainingTime = TimeSpan.Zero;
                    if (response.Headers.Contains("X-RateLimit-Remaining"))
                        remainingTime = TimeSpan.FromSeconds(int.Parse(response.Headers.GetValues("X-RateLimit-Remaining").First()));
                    apiResponse = ApiResponse.Create($"{nameof(Element14Api)} request throttled. Try again in {remainingTime}", nameof(Element14Api));
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

        private HttpRequestMessage CreateRequest(HttpMethod method, Uri uri)
        {
            var message = new HttpRequestMessage(method, uri);
            return message;
        }

        private string GetApiUrl() => string.IsNullOrEmpty(_configuration.ApiUrl) ? DefaultApiUrl : _configuration.ApiUrl;

        private void ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(_configuration.ApiKey)) throw new BinnerConfigurationException($"{nameof(Element14Configuration)} must specify a ApiKey!");
            if (string.IsNullOrEmpty(_configuration.ApiUrl)) throw new BinnerConfigurationException($"{nameof(Element14Configuration)} must specify a ApiUrl!");
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public override string ToString()
            => $"{nameof(Element14Api)}";

        public class Element14UnauthorizedException : Exception
        {
            public Element14UnauthorizedException(string message) : base(message)
            {

            }
        }
    }
}
