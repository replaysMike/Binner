using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations;
using Binner.SwarmApi;
using Binner.SwarmApi.Request;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Binner.Services.Integrations
{
    public class SwarmApi : IIntegrationApi
    {
        public string Name => "Swarm";
        private readonly ILogger<SwarmApi> _logger;
        private readonly SwarmConfiguration _configuration;
        private readonly UserConfiguration _userConfiguration;
        private readonly SwarmApiClient _client;

        public bool IsEnabled => _configuration.Enabled;

        public IApiConfiguration Configuration => _configuration;

        public SwarmApi(ILogger<SwarmApi> logger, SwarmConfiguration configuration, UserConfiguration userConfiguration)
        {
            _logger = logger;
            
            // fixes an old data bug of unknown origin
            if (configuration.ApiUrl == "https://swarm") configuration.ApiUrl = "https://swarm.binner.io";

            _configuration = configuration;
            _userConfiguration = userConfiguration;
            var swarmApiConfiguration =
                new SwarmApiConfiguration(_configuration.ApiKey ?? string.Empty, new Uri(_configuration.ApiUrl))
                {
                    Timeout = _configuration.Timeout
                };
            _client = new SwarmApiClient(swarmApiConfiguration);
        }

        public Task<IApiResponse> SearchAsync(string partNumber, int recordCount = ApiConstants.MaxRecords, Dictionary<string, string>? additionalOptions = null) => SearchAsync(partNumber, string.Empty, string.Empty, recordCount, additionalOptions);

        public Task<IApiResponse> SearchAsync(string partNumber, string partType, int recordCount = ApiConstants.MaxRecords, Dictionary<string, string>? additionalOptions = null) => SearchAsync(partNumber, partType, string.Empty, recordCount, additionalOptions);

        public async Task<IApiResponse> SearchAsync(string partNumber, string partType, string mountingType, int recordCount = ApiConstants.MaxRecords, Dictionary<string, string>? additionalOptions = null)
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

        public void Dispose()
        {
        }
    }
}
