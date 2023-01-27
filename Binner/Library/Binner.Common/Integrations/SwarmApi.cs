using Binner.Common.Integrations.Models;
using Binner.Common.Models.Configuration.Integrations;
using Binner.Common.Services;
using Binner.SwarmApi;
using Binner.SwarmApi.Request;
using Microsoft.AspNetCore.Http;
using NPOI.OpenXmlFormats.Wordprocessing;
using System;
using System.Linq;
using System.Net.Http;
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

        /// <summary>
        /// Returns true if the Api is properly configured
        /// </summary>
        public bool IsSearchPartsConfigured => _configuration.Enabled;

        public bool IsUserConfigured => _configuration.Enabled;

        public SwarmApi(SwarmConfiguration configuration, ICredentialService credentialService, IHttpContextAccessor httpContextAccessor, RequestContextAccessor requestContext)
        {
            _configuration = configuration;
            _credentialService = credentialService;
            _httpContextAccessor = httpContextAccessor;
            _client = new SwarmApiClient(new SwarmApiConfiguration(_configuration.ApiKey, new Uri(_configuration.ApiUrl)));
            _requestContext = requestContext;
        }

        public async Task<IApiResponse> SearchAsync(string partNumber, string partType = "", string mountingType = "", int recordCount = 50)
        {
            if (!(recordCount > 0)) throw new ArgumentOutOfRangeException(nameof(recordCount));

            var response = await _client.SearchPartsAsync(new SearchPartRequest { PartNumber = partNumber, PartType = partType, MountingType = mountingType });
            if (response.IsSuccessful)
            {
                return new ApiResponse(response.Response, nameof(SwarmApi));
            }
            else if (response.IsRequestThrottled)
            {
                return ApiResponse.Create(response.Errors.First(), nameof(SwarmApi));
            }

            return ApiResponse.Create($"Api returned error status code {response.StatusCode}: {string.Join("\n", response.Errors)}", nameof(SwarmApi));
        }

        public async Task<IApiResponse> GetPartInformationAsync(string partNumber, string partType = "", string mountingType = "", int recordCount = 50)
        {
            if (!(recordCount > 0)) throw new ArgumentOutOfRangeException(nameof(recordCount));

            var response = await _client.GetPartInformationAsync(new PartInformationRequest { PartNumber = partNumber, PartType = partType, MountingType = mountingType });
            if (response.IsSuccessful)
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
