using Binner.Common.Integrations.Models;
using Binner.Common.Services;
using Binner.Global.Common;
using Binner.Model.Configuration.Integrations;
using Binner.SwarmApi;
using Binner.SwarmApi.Request;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Binner.Common.Integrations
{
    public class SwarmApi : IIntegrationApi
    {
        public string Name => "Swarm";
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
            var swarmApiConfiguration =
                new SwarmApiConfiguration(_configuration.ApiKey ?? string.Empty, new Uri(_configuration.ApiUrl))
                {
                    Timeout = _configuration.Timeout
                };
            _client = new SwarmApiClient(swarmApiConfiguration);
            _requestContext = requestContext;
        }

        public Task<IApiResponse> SearchAsync(string partNumber, int recordCount = 25, Dictionary<string, string>? additionalOptions = null) => SearchAsync(partNumber, string.Empty, string.Empty, recordCount, additionalOptions);

        public Task<IApiResponse> SearchAsync(string partNumber, string partType, int recordCount = 25, Dictionary<string, string>? additionalOptions = null) => SearchAsync(partNumber, partType, string.Empty, recordCount, additionalOptions);

        public async Task<IApiResponse> SearchAsync(string partNumber, string partType, string mountingType, int recordCount = 50, Dictionary<string, string>? additionalOptions = null)
        {
            if (!(recordCount > 0)) throw new ArgumentOutOfRangeException(nameof(recordCount));

            try
            {
                var response = await _client.SearchPartsAsync(new SearchPartRequest
                    { PartNumber = partNumber, PartType = partType, MountingType = mountingType });
                if (response.IsSuccessful && response.Response != null)
                {
                    return new ApiResponse(response.Response, nameof(SwarmApi));
                }
                else if (response.IsRequestThrottled)
                {
                    return ApiResponse.Create(response.Errors.First(), nameof(SwarmApi));
                }

                return ApiResponse.Create(
                    $"Api returned error status code {response.StatusCode}: {string.Join("\n", response.Errors)}",
                    nameof(SwarmApi));
            }
            catch (TimeoutException ex)
            {
                // treat timeouts as warnings
                return ApiResponse.CreateWarning($"Api request timed out: {ex.Message}", nameof(SwarmApi));
            }
            catch (JsonReaderException ex)
            {
                // unexpected response, server down?
                return ApiResponse.CreateWarning($"Server down! {ex.Message}", nameof(SwarmApi));
            }
            catch (Exception ex)
            {
                // unexpected error
                return ApiResponse.CreateWarning($"Server not available. {ex.Message}", nameof(SwarmApi));
            }
        }

        public Task<IApiResponse> GetOrderAsync(string orderId, Dictionary<string, string>? additionalOptions = null)
        {
            throw new NotImplementedException();
        }

        public async Task<IApiResponse> GetProductDetailsAsync(string partNumber, Dictionary<string, string>? additionalOptions = null)
        {
            try
            {
                var response = await _client.GetPartInformationAsync(new PartInformationRequest
                { PartNumber = partNumber });
                if (response.IsSuccessful && response.Response != null)
                {
                    return new ApiResponse(response.Response, nameof(SwarmApi));
                }
                else if (response.IsRequestThrottled)
                {
                    return ApiResponse.Create(response.Errors.First(), nameof(SwarmApi));
                }

                return ApiResponse.Create(
                    $"Api returned error status code {response.StatusCode}: {string.Join("\n", response.Errors)}",
                    nameof(SwarmApi));
            }
            catch (TimeoutException ex)
            {
                return ApiResponse.Create($"Api request timed out: {ex.Message}", nameof(SwarmApi));
            }
        }
    }
}
