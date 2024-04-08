using Binner.Common.Integrations.ResponseProcessors;
using Binner.Common.Services;
using Binner.Model;
using Binner.Model.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TypeSupport.Extensions;

namespace Binner.Common.Integrations
{
    public class PartInformationProvider : IPartInformationProvider
    {
        private IIntegrationApiFactory _integrationApiFactory;
        private ILogger _logger;
        private WebHostServiceConfiguration _configuration;
        private List<Type> _providers = new()
        {
            typeof(SwarmApi),
            typeof(DigikeyApi),
            typeof(MouserApi),
            typeof(ArrowApi),
            typeof(NexarApi),
            typeof(TmeApi)
        };

        public PartInformationProvider(IIntegrationApiFactory integrationApiFactory, ILogger logger, WebHostServiceConfiguration configuration)
        {
            _integrationApiFactory = integrationApiFactory;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<PartInformationResults> FetchPartInformationAsync(string partNumber, string partType, string mountingType, string supplierPartNumbers, int userId, ICollection<PartType> partTypes, Part? inventoryPart)
        {
            var context = new ProcessingContext
            {
                PartNumber = partNumber,
                PartType = partType,
                MountingType = mountingType,
                SupplierPartNumbers = supplierPartNumbers,
                UserId = userId,
                InventoryPart = inventoryPart,
                ApiResponses = new Dictionary<string, Model.Integrations.ApiResponseState>(),
                Results = new PartResults(),
                PartTypes = partTypes
            };

            // for each configured provider, fetch results
            foreach (var provider in _providers)
            {
                // fetch part info, merge data
                var api = await _integrationApiFactory.CreateAsync(provider, userId) as IIntegrationApi;
                if (api == null) throw new NotSupportedException($"Unknown provider type '{provider.Name}'");

                if (api.Configuration.IsConfigured)
                {
                    // Process the api call, store results in context
                    await ProcessResponseAsync(provider, api, context);
                }
            }

            var results = new PartInformationResults(context.ApiResponses, context.Results);
            return results;
        }

        private async Task<ProcessingContext> ProcessResponseAsync(Type provider, IIntegrationApi api, ProcessingContext context)
        {
            var providerImplementations = new Dictionary<Type, Func<Task>>()
            {
                { typeof(SwarmApi), async () =>
                    {
                        var processor = new SwarmPartInfoResponseProcessor(_logger, _configuration, 10);
                        await processor.ExecuteAsync(api, context);
                    }
                },
                { typeof(DigikeyApi), async () =>
                    {
                        var processor = new DigiKeyPartInfoResponseProcessor(_logger, _configuration, 20);
                        await processor.ExecuteAsync(api, context);
                    }
                },
                { typeof(MouserApi), async () =>
                    {
                        var processor = new MouserPartInfoResponseProcessor(_logger, _configuration, 30);
                        await processor.ExecuteAsync(api, context);
                    }
                },
                { typeof(ArrowApi), async () =>
                    {
                        var processor = new ArrowPartInfoResponseProcessor(_logger, _configuration, 40);
                        await processor.ExecuteAsync(api, context);
                    }
                },
                { typeof(NexarApi), async () =>
                    {
                        var processor = new NexarPartInfoResponseProcessor(_logger, _configuration, 50);
                        await processor.ExecuteAsync(api, context);
                    }
                },
                { typeof(TmeApi), async () =>
                    {
                        var processor = new TmePartInfoResponseProcessor(_logger, _configuration, 60);
                        await processor.ExecuteAsync(api, context);
                    }
                },
            };

            if (_providers.Count != providerImplementations.Count)
                throw new BinnerConfigurationException($"The number of configured Api providers do not match! Please ensure the Apis are configured correctly in the {nameof(PartInformationProvider)}.");

            if (!_providers.Contains(provider))
                throw new BinnerConfigurationException($"The requested provider '{provider.Name}' does not exist in the {nameof(PartInformationProvider)}.");

            if (providerImplementations.ContainsKey(provider))
            {
                await providerImplementations[provider].Invoke();
                return context;
            }

            throw new BinnerConfigurationException($"The requested provider '{provider.Name}' does not exist in the {nameof(IResponseProcessor)} factory.");
        }

    }
}
