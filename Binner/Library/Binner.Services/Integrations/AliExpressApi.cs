﻿using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations;
using Microsoft.AspNetCore.Http;

namespace Binner.Services.Integrations
{
    public class AliExpressApi : IIntegrationApi
    {
        public string Name => "AliExpress";
       private const string BasePath = "/api/v3/parts";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _client;
        private readonly AliExpressConfiguration _configuration;

        public bool IsEnabled => _configuration.Enabled;

        public IApiConfiguration Configuration => _configuration;

        public AliExpressApi(AliExpressConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _client = new HttpClient();
        }

        public Task<IApiResponse> GetDatasheetsAsync(string partNumber)
        {
            throw new NotImplementedException();
        }

        public Task<IApiResponse> SearchAsync(string partNumber, int recordCount = ApiConstants.MaxRecords, Dictionary<string, string>? additionalOptions = null)
        {
            throw new NotImplementedException();
        }

        public Task<IApiResponse> SearchAsync(string partNumber, string partType, int recordCount = ApiConstants.MaxRecords, Dictionary<string, string>? additionalOptions = null)
        {
            throw new NotImplementedException();
        }

        public Task<IApiResponse> SearchAsync(string partNumber, string partType, string mountingType, int recordCount = ApiConstants.MaxRecords, Dictionary<string, string>? additionalOptions = null)
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

        public void Dispose()
        {
        }

        public override string ToString()
            => $"{nameof(AliExpressApi)}";

    }
}
