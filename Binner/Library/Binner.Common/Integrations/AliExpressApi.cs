﻿using Binner.Common.Integrations.Models;
using Binner.Common.Models.Configuration.Integrations;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Binner.Common.Integrations
{
    public class AliExpressApi : IIntegrationApi
    {
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

        public Task<IApiResponse> SearchAsync(string partNumber, int recordCount = 25)
        {
            throw new NotImplementedException();
        }

        public Task<IApiResponse> SearchAsync(string partNumber, string partType, int recordCount = 25)
        {
            throw new NotImplementedException();
        }

        public Task<IApiResponse> SearchAsync(string partNumber, string partType, string mountingType, int recordCount = 25)
        {
            throw new NotImplementedException();
        }

        public Task<IApiResponse> GetOrderAsync(string orderId)
        {
            throw new NotImplementedException();
        }

        public Task<IApiResponse> GetProductDetailsAsync(string partNumber)
        {
            throw new NotImplementedException();
        }
    }
}
