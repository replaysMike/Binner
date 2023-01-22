using Binner.Common.Integrations.Models;
using Binner.Common.Models.Configuration.Integrations;
using Binner.Common.Services;
using Microsoft.AspNetCore.Http;
using System;
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
        private readonly HttpClient _client;
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
            _client = new HttpClient();
            _requestContext = requestContext;
        }

        public async Task<IApiResponse> SearchAsync(string partNumber, string partType = "", string mountingType = "", int recordCount = 50)
        {
            if (!(recordCount > 0)) throw new ArgumentOutOfRangeException(nameof(recordCount));

            // todo: wire up the public swarm service

            throw new NotImplementedException();
        }
    }
}
