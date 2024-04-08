using Binner.Common.Extensions;
using Binner.Common.Integrations.Models;
using Binner.Model.Configuration.Integrations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Binner.Common.Integrations
{
    public class OctopartApi : IIntegrationApi
    {
        public string Name => "Octopart";
        public const string BasePath = "/api/v3/parts";
        private readonly OctopartConfiguration _configuration;
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public bool IsEnabled => _configuration.Enabled;

        public IApiConfiguration Configuration => _configuration;

        public OctopartApi(OctopartConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpContextAccessor = httpContextAccessor;
            _client = new HttpClient();
        }

        public async Task<IApiResponse> GetDatasheetsAsync(string partNumber)
        {
            var datasheets = new List<string>();
            partNumber = "SN74S74N";
            var values = new Dictionary<string, string>
            {
                { "apikey", _configuration.ApiKey ?? string.Empty },
                //{ "apikey", "EXAMPLE_KEY" },
                { "queries" , $@"[{{""mpn"":""{partNumber}""}}]"},
                { "pretty_print", "true" },
                { "include[]", "datasheets" },
            };
            System.Environment.SetEnvironmentVariable("MONO_URI_IRIPARSING", "true");
            var uri = Url.Combine(false, _configuration.ApiUrl, BasePath, $"/match?" + string.Join("&", values.Select(x => $"{x.Key}={x.Value}")));

            var k = uri.ToString();
            var req = new HttpRequestMessage(HttpMethod.Get, uri);
            // req.Content = new FormUrlEncodedContent(values);
            var response = await _client.SendAsync(req);
            if (response.IsSuccessStatusCode)
            {
                var resultString = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<dynamic>(resultString);
                // dynamic requires Microsoft.CSharp nuget package
                if (result != null)
                {
                    foreach (var r in result["results"])
                    {
                        foreach (var item in r["items"])
                        {
                            foreach (var datasheet in item["datasheets"])
                            {
                                var url = datasheet["url"];
                                datasheets.Add(url.Value);
                            }
                        }
                    }
                }
                // parse into readable format
            }
            return new ApiResponse(datasheets, nameof(OctopartApi));
        }

        
        public Task<IApiResponse> SearchAsync(string partNumber, int recordCount = 25, Dictionary<string, string>? additionalOptions = null)
        {
            throw new NotImplementedException();
        }

        public Task<IApiResponse> SearchAsync(string partNumber, string partType, int recordCount = 25, Dictionary<string, string>? additionalOptions = null)
        {
            throw new NotImplementedException();
        }

        public Task<IApiResponse> SearchAsync(string partNumber, string partType, string mountingType, int recordCount = 25, Dictionary<string, string>? additionalOptions = null)
        {
            throw new NotImplementedException();
        }

        public Task<IApiResponse> GetOrderAsync(string orderId, Dictionary<string, string>? additionalOptions = null)
        {
            throw new NotImplementedException();
        }

        public Task<IApiResponse> GetProductDetailsAsync(string partNumber, Dictionary<string, string>? additionalOptions = null)
        {
            throw new NotImplementedException();
        }
    }
}
