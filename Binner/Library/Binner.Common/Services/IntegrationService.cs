using AutoMapper;
using Binner.Common.Integrations;
using Binner.Common.Integrations.Models.DigiKey;
using Binner.Common.Integrations.Models.Mouser;
using Binner.Common.Models;
using Binner.Common.Models.Configuration.Integrations;
using Binner.Common.Models.Responses;
using Binner.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public class IntegrationService
    {
        private const string MissingDatasheetCoverName = "datasheetcover.png";
        private const StringComparison ComparisonType = StringComparison.InvariantCultureIgnoreCase;
        private readonly IStorageProvider _storageProvider;
        private readonly IMapper _mapper;
        private readonly IIntegrationApiFactory _integrationApiFactory;
        private readonly RequestContextAccessor _requestContext;
        private readonly ISwarmService _swarmService;

        public IntegrationService(IStorageProvider storageProvider, IMapper mapper, IIntegrationApiFactory integrationApiFactory, ISwarmService swarmService, RequestContextAccessor requestContextAccessor)
        {
            _storageProvider = storageProvider;
            _mapper = mapper;
            _integrationApiFactory = integrationApiFactory;
            _requestContext = requestContextAccessor;
            _swarmService = swarmService;
        }

        public async Task<TestApiResponse> TestApiAsync(string apiName)
        {
            var user = _requestContext.GetUserContext();

            switch (apiName.ToLower())
            {
                case "swarm":
                    {
                        var api = await _integrationApiFactory.CreateAsync<Integrations.SwarmApi>(user?.UserId ?? 0);
                        if (!api.IsEnabled)
                            return new TestApiResponse(nameof(Integrations.SwarmApi), "Api is not enabled.");
                        try
                        {
                            var result = await api.SearchAsync("LM555", 1);
                            if (result.Errors.Any())
                                return new TestApiResponse(nameof(Integrations.SwarmApi), string.Join(". ", result.Errors));
                            return new TestApiResponse(nameof(Integrations.SwarmApi), true);
                        }
                        catch (Exception ex)
                        {
                            return new TestApiResponse(nameof(Integrations.SwarmApi), ex.GetBaseException().Message);
                        }
                    }
                case "digikey":
                    {
                        var api = await _integrationApiFactory.CreateAsync<Integrations.DigikeyApi>(user?.UserId ?? 0);
                        if (!api.IsEnabled)
                            return new TestApiResponse(nameof(Integrations.DigikeyApi), "Api is not enabled.");
                        try
                        {
                            var result = await api.SearchAsync("LM555", 1);
                            if (result.Errors.Any())
                            {
                                var errorResult = new TestApiResponse(nameof(Integrations.DigikeyApi), string.Join(". ", result.Errors));
                                if (result.RequiresAuthentication)
                                    errorResult.AuthorizationUrl = result.RedirectUrl;
                                return errorResult;
                            }
                            return new TestApiResponse(nameof(Integrations.DigikeyApi), true);
                        }
                        catch (Exception ex)
                        {
                            return new TestApiResponse(nameof(Integrations.DigikeyApi), ex.GetBaseException().Message);
                        }
                    }
                case "mouser":
                    {
                        var api = await _integrationApiFactory.CreateAsync<Integrations.MouserApi>(user?.UserId ?? 0);
                        if (!api.IsEnabled)
                            return new TestApiResponse(nameof(Integrations.MouserApi), "Api is not enabled.");
                        try
                        {
                            var result = await api.SearchAsync("LM555", 1);
                            if (result.Errors.Any())
                                return new TestApiResponse(nameof(Integrations.MouserApi), string.Join(". ", result.Errors));
                            return new TestApiResponse(nameof(Integrations.MouserApi), true);
                        }
                        catch (Exception ex)
                        {
                            return new TestApiResponse(nameof(Integrations.MouserApi), ex.GetBaseException().Message);
                        }
                    }
                case "octopart":
                    {
                        var api = await _integrationApiFactory.CreateAsync<Integrations.OctopartApi>(user?.UserId ?? 0);
                        if (!api.IsEnabled)
                            return new TestApiResponse(nameof(Integrations.OctopartApi), "Api is not enabled.");
                        try
                        {
                            var result = await api.SearchAsync("LM555", 1);
                            if (result.Errors.Any())
                                return new TestApiResponse(nameof(Integrations.OctopartApi), string.Join(". ", result.Errors));
                            return new TestApiResponse(nameof(Integrations.OctopartApi), true);
                        }
                        catch (Exception ex)
                        {
                            return new TestApiResponse(nameof(Integrations.OctopartApi), ex.GetBaseException().Message);
                        }
                    }
            }

            return new TestApiResponse(apiName, $"Unknown api name!");
        }
    }
}
