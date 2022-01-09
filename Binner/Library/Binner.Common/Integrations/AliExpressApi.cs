using Binner.Common.Integrations.Models;
using Microsoft.AspNetCore.Http;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _client;

        public AliExpressApi(string apiKey, string apiUrl, IHttpContextAccessor httpContextAccessor)
        {
            _apiKey = apiKey;
            _apiUrl = apiUrl;
            _httpContextAccessor = httpContextAccessor;
            _client = new HttpClient();
        }

        public bool IsConfigured => !string.IsNullOrEmpty(_apiKey) && !string.IsNullOrEmpty(_apiUrl);

        public Task<IApiResponse> GetDatasheetsAsync(string partNumber)
        {
            return Task.FromResult<IApiResponse>(ApiResponse.Create(new List<string>(), nameof(AliExpressApi)));
        }
    }
}
