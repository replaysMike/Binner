using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Binner.Common.Integrations
{
    public class AliExpressApi : IIntegrationApi
    {
        private const string BasePath = "/api/v3/parts";
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly HttpClient _client;

        public AliExpressApi(string apiKey, string apiUrl)
        {
            _apiKey = apiKey;
            _apiUrl = apiUrl;
            _client = new HttpClient();
        }

        public bool IsConfigured => !string.IsNullOrEmpty(_apiKey) && !string.IsNullOrEmpty(_apiUrl);

        public async Task<ICollection<string>> GetDatasheetsAsync(string partNumber)
        {
            return new List<string>();
        }
    }
}
