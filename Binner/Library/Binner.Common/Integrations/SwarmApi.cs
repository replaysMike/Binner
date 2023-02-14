using Binner.Common.Integrations.Models;
using Binner.Common.Models.Configuration.Integrations;
using Binner.Common.Services;
using Binner.SwarmApi;
using Binner.SwarmApi.Request;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Binner.Common.Integrations
{
    public class SwarmApi : IIntegrationApi
    {
        private readonly SwarmConfiguration _configuration;
        private readonly ICredentialService _credentialService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly RequestContextAccessor _requestContext;
        private readonly SwarmApiClient _client;
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);

        public bool IsEnabled => _configuration.Enabled;

        public IApiConfiguration Configuration => _configuration;

        public SwarmApi(SwarmConfiguration configuration, ICredentialService credentialService, IHttpContextAccessor httpContextAccessor, RequestContextAccessor requestContext)
        {
            _configuration = configuration;
            _credentialService = credentialService;
            _httpContextAccessor = httpContextAccessor;
            _client = new SwarmApiClient(new SwarmApiConfiguration(_configuration.ApiKey ?? string.Empty, new Uri(_configuration.ApiUrl)));
            _requestContext = requestContext;
        }

        public Task<IApiResponse> SearchAsync(string partNumber, int recordCount = 25) => SearchAsync(partNumber, string.Empty, string.Empty, recordCount);

        public Task<IApiResponse> SearchAsync(string partNumber, string partType, int recordCount = 25) => SearchAsync(partNumber, partType, string.Empty, recordCount);

        public async Task<IApiResponse> SearchAsync(string partNumber, string partType = "", string mountingType = "", int recordCount = 50)
        {
            if (!(recordCount > 0)) throw new ArgumentOutOfRangeException(nameof(recordCount));

            var response = await _client.SearchPartsAsync(new SearchPartRequest { PartNumber = partNumber, PartType = partType, MountingType = mountingType });
            if (response.IsSuccessful && response.Response != null)
            {
                return new ApiResponse(response.Response, nameof(SwarmApi));
            }
            else if (response.IsRequestThrottled)
            {
                return ApiResponse.Create(response.Errors.First(), nameof(SwarmApi));
            }

            return ApiResponse.Create($"Api returned error status code {response.StatusCode}: {string.Join("\n", response.Errors)}", nameof(SwarmApi));
        }

        public Task<IApiResponse> GetOrderAsync(string orderId)
        {
            throw new NotImplementedException();
        }

        public async Task<IApiResponse> GetProductDetailsAsync(string partNumber)
        {
            var response = await _client.GetPartInformationAsync(new PartInformationRequest { PartNumber = partNumber });
            if (response.IsSuccessful && response.Response != null)
            {
                return new ApiResponse(response.Response, nameof(SwarmApi));
            }
            else if (response.IsRequestThrottled)
            {
                return ApiResponse.Create(response.Errors.First(), nameof(SwarmApi));
            }

            return ApiResponse.Create($"Api returned error status code {response.StatusCode}: {string.Join("\n", response.Errors)}", nameof(SwarmApi));
        }
    }
}
