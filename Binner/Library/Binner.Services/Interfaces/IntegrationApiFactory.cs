using AutoMapper;
using Binner.Common.Integrations;
using Binner.Global.Common;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Services.Integrations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Binner.Services
{
    public class IntegrationApiFactory : IIntegrationApiFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IMapper _mapper;
        private readonly IIntegrationCredentialsCacheProvider _credentialProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRequestContextAccessor _requestContext;
        private readonly ICredentialService _credentialService;
        private readonly IApiHttpClientFactory _httpClientFactory;
        private readonly IUserConfigurationService _userConfigurationService;
        /// <summary>
        /// Default user configuration used for global/system integration APIs
        /// </summary>
        private readonly UserConfiguration _defaultUserConfiguration = new UserConfiguration
        {
            Language = Model.Languages.En,
            Currency = Model.Currencies.USD,
        };

        public IntegrationApiFactory(ILoggerFactory loggerFactory, IMapper mapper, IIntegrationCredentialsCacheProvider credentialProvider, IHttpContextAccessor httpContextAccessor, IRequestContextAccessor requestContext, ICredentialService credentialService, IApiHttpClientFactory httpClientFactory, IUserConfigurationService userConfigurationService)
        {
            _loggerFactory = loggerFactory;
            _mapper = mapper;
            _credentialProvider = credentialProvider;
            _httpContextAccessor = httpContextAccessor;
            _requestContext = requestContext;
            _credentialService = credentialService;
            _httpClientFactory = httpClientFactory;
            _userConfigurationService = userConfigurationService;
        }

        /// <summary>
        /// Create an integration Api for system use
        /// Only for global system usage as no user api keys will be available
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public T CreateGlobal<T>(IntegrationConfiguration integrationConfiguration)
            where T : class
        {
            var apiType = typeof(T);

            if (apiType == typeof(Integrations.SwarmApi))
            {
                var result = CreateGlobalSwarmApi(integrationConfiguration);
                var resultTyped = result as T;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(DigikeyApi))
            {
                var result = CreateGlobalDigikeyApi(integrationConfiguration);
                var resultTyped = result as T;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(MouserApi))
            {
                var result = CreateGlobalMouserApi(integrationConfiguration);
                var resultTyped = result as T;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(NexarApi))
            {
                var result = CreateGlobalNexarApi(integrationConfiguration);
                var resultTyped = result as T;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(ArrowApi))
            {
                var result = CreateGlobalArrowApi(integrationConfiguration);
                var resultTyped = result as T;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(TmeApi))
            {
                var result = CreateGlobalTmeApi(integrationConfiguration);
                var resultTyped = result as T;
                if (resultTyped != null)
                    return resultTyped;
            }
            throw new NotImplementedException($"Unhandled type '{apiType.Name}'");
        }

        /// <summary>
        /// Create an integration Api for system use
        /// Only for global system usage as no user api keys will be available
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IIntegrationApi CreateGlobal(Type apiType, IntegrationConfiguration integrationConfiguration)
        {
            if (apiType == typeof(Integrations.SwarmApi))
            {
                var result = CreateGlobalSwarmApi(integrationConfiguration);
                var resultTyped = result;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(DigikeyApi))
            {
                var result = CreateGlobalDigikeyApi(integrationConfiguration);
                var resultTyped = result;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(MouserApi))
            {
                var result = CreateGlobalMouserApi(integrationConfiguration);
                var resultTyped = result;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(NexarApi))
            {
                var result = CreateGlobalNexarApi(integrationConfiguration);
                var resultTyped = result;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(ArrowApi))
            {
                var result = CreateGlobalArrowApi(integrationConfiguration);
                var resultTyped = result;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(TmeApi))
            {
                var result = CreateGlobalTmeApi(integrationConfiguration);
                var resultTyped = result;
                if (resultTyped != null)
                    return resultTyped;
            }
            throw new NotImplementedException($"Unhandled type '{apiType.Name}'");
        }

        /// <summary>
        /// Create an integration Api for use by users
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userId"></param>
        /// <param name="integrationConfiguration">The integration configuration for the organization</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<T> CreateAsync<T>(int userId, OrganizationIntegrationConfiguration integrationConfiguration)
            where T : class
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            if (integrationConfiguration == null) throw new ArgumentNullException(nameof(integrationConfiguration));
#pragma warning disable CS1998
            var getCredentialsMethod = async () =>
#pragma warning restore CS1998
            {
                // build the credentials list
                var credentials = new List<ApiCredential>();

                // add user defined credentials
                var swarmConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", integrationConfiguration.SwarmEnabled },
                    { "ApiKey", integrationConfiguration.SwarmApiKey ?? string.Empty },
                    { "ApiUrl", integrationConfiguration.SwarmApiUrl },
                    { "Timeout", integrationConfiguration.SwarmTimeout },
                };
                credentials.Add(new ApiCredential(userId, swarmConfiguration, nameof(SwarmApi)));

                var digikeyConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", integrationConfiguration.DigiKeyEnabled },
                    { "Site", (int)integrationConfiguration.DigiKeySite },
                    { "ClientId", integrationConfiguration.DigiKeyClientId ?? string.Empty },
                    { "ClientSecret", integrationConfiguration.DigiKeyClientSecret ?? string.Empty },
                    { "oAuthPostbackUrl", integrationConfiguration.DigiKeyOAuthPostbackUrl },
                    { "ApiUrl", integrationConfiguration.DigiKeyApiUrl }
                };
                credentials.Add(new ApiCredential(userId, digikeyConfiguration, nameof(DigikeyApi)));

                var mouserConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", integrationConfiguration.MouserEnabled },
                    { "SearchApiKey", integrationConfiguration.MouserSearchApiKey ?? string.Empty },
                    { "CartApiKey", integrationConfiguration.MouserCartApiKey ?? string.Empty },
                    { "OrderApiKey", integrationConfiguration.MouserOrderApiKey ?? string.Empty },
                    { "ApiUrl", integrationConfiguration.MouserApiUrl },
                };
                credentials.Add(new ApiCredential(userId, mouserConfiguration, nameof(MouserApi)));

                var arrowConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", integrationConfiguration.ArrowEnabled },
                    { "Username", integrationConfiguration.ArrowUsername ?? string.Empty },
                    { "ApiKey", integrationConfiguration.ArrowApiKey ?? string.Empty },
                    { "ApiUrl", integrationConfiguration.ArrowApiUrl },
                };
                credentials.Add(new ApiCredential(userId, arrowConfiguration, nameof(ArrowApi)));

                var nexarConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", integrationConfiguration.NexarEnabled },
                    { "ClientId", integrationConfiguration.NexarClientId ?? string.Empty },
                    { "ClientSecret", integrationConfiguration.NexarClientSecret ?? string.Empty },
                };
                credentials.Add(new ApiCredential(userId, nexarConfiguration, nameof(NexarApi)));

                var tmeConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", integrationConfiguration.TmeEnabled },
                    { "Country", integrationConfiguration.TmeCountry },
                    { "ApplicationSecret", integrationConfiguration.TmeApplicationSecret ?? string.Empty },
                    { "ApiKey", integrationConfiguration.TmeApiKey ?? string.Empty },
                    { "ApiUrl", integrationConfiguration.TmeApiUrl },
                    { "ResolveExternalLinks", integrationConfiguration.TmeResolveExternalLinks },
                };
                credentials.Add(new ApiCredential(userId, tmeConfiguration, nameof(TmeApi)));

                return new ApiCredentialConfiguration(userId, credentials);
            };
            return await CreateAsync<T>(userId, getCredentialsMethod, true);
        }

        /// <summary>
        /// Create an integration Api for use by users
        /// </summary>
        /// <param name="apiType"></param>
        /// <param name="userId"></param>
        /// <param name="integrationConfiguration">The integration configuration for the specified organization</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IIntegrationApi> CreateAsync(Type apiType, int userId, OrganizationIntegrationConfiguration integrationConfiguration)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            if (integrationConfiguration == null) throw new ArgumentNullException(nameof(integrationConfiguration));
#pragma warning disable CS1998
            var getCredentialsMethod = async () =>
#pragma warning restore CS1998
            {
                // build the credentials list
                var credentials = new List<ApiCredential>();

                // add user defined credentials
                var swarmConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", integrationConfiguration.SwarmEnabled },
                    { "ApiKey", integrationConfiguration.SwarmApiKey ?? string.Empty },
                    { "ApiUrl", integrationConfiguration.SwarmApiUrl },
                    { "Timeout", integrationConfiguration.SwarmTimeout },
                };
                credentials.Add(new ApiCredential(userId, swarmConfiguration, nameof(SwarmApi)));

                var digikeyConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", integrationConfiguration.DigiKeyEnabled },
                    { "Site", (int)integrationConfiguration.DigiKeySite },
                    { "ClientId", integrationConfiguration.DigiKeyClientId ?? string.Empty },
                    { "ClientSecret", integrationConfiguration.DigiKeyClientSecret ?? string.Empty },
                    { "oAuthPostbackUrl", integrationConfiguration.DigiKeyOAuthPostbackUrl },
                    { "ApiUrl", integrationConfiguration.DigiKeyApiUrl }
                };
                credentials.Add(new ApiCredential(userId, digikeyConfiguration, nameof(DigikeyApi)));

                var mouserConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", integrationConfiguration.MouserEnabled },
                    { "SearchApiKey", integrationConfiguration.MouserSearchApiKey ?? string.Empty },
                    { "CartApiKey", integrationConfiguration.MouserCartApiKey ?? string.Empty },
                    { "OrderApiKey", integrationConfiguration.MouserOrderApiKey ?? string.Empty },
                    { "ApiUrl", integrationConfiguration.MouserApiUrl },
                };
                credentials.Add(new ApiCredential(userId, mouserConfiguration, nameof(MouserApi)));

                var arrowConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", integrationConfiguration.ArrowEnabled },
                    { "Username", integrationConfiguration.ArrowUsername ?? string.Empty },
                    { "ApiKey", integrationConfiguration.ArrowApiKey ?? string.Empty },
                    { "ApiUrl", integrationConfiguration.ArrowApiUrl },
                };
                credentials.Add(new ApiCredential(userId, arrowConfiguration, nameof(ArrowApi)));

                var nexarConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", integrationConfiguration.NexarEnabled },
                    { "ClientId", integrationConfiguration.NexarClientId ?? string.Empty },
                    { "ClientSecret", integrationConfiguration.NexarClientSecret ?? string.Empty },
                };
                credentials.Add(new ApiCredential(userId, nexarConfiguration, nameof(NexarApi)));

                var tmeConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", integrationConfiguration.TmeEnabled },
                    { "Country", integrationConfiguration.TmeCountry },
                    { "ApplicationSecret", integrationConfiguration.TmeApplicationSecret ?? string.Empty },
                    { "ApiKey", integrationConfiguration.TmeApiKey ?? string.Empty },
                    { "ApiUrl", integrationConfiguration.TmeApiUrl },
                    { "ResolveExternalLinks", integrationConfiguration.TmeResolveExternalLinks },
                };
                credentials.Add(new ApiCredential(userId, tmeConfiguration, nameof(TmeApi)));

                return new ApiCredentialConfiguration(userId, credentials);
            };
            return await CreateAsync(apiType, userId, getCredentialsMethod, true);
        }

        /// <summary>
        /// Create an integration Api for use by users
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userId"></param>
        /// <param name="getCredentialsMethod"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<T> CreateAsync<T>(int userId, Func<Task<ApiCredentialConfiguration>> getCredentialsMethod, bool cache = true)
            where T : class
        {
            var apiType = typeof(T);
            var credentialKey = ApiCredentialKey.Create(userId);

            if (apiType == typeof(Integrations.SwarmApi))
            {
                var result = await CreateSwarmApiAsync(credentialKey, getCredentialsMethod, cache);
                var resultTyped = result as T;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(DigikeyApi))
            {
                var result = await CreateDigikeyApiAsync(credentialKey, getCredentialsMethod, cache);
                var resultTyped = result as T;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(MouserApi))
            {
                var result = await CreateMouserApiAsync(credentialKey, getCredentialsMethod, cache);
                var resultTyped = result as T;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(NexarApi))
            {
                var result = await CreateNexarApiAsync(credentialKey, getCredentialsMethod, cache);
                var resultTyped = result as T;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(ArrowApi))
            {
                var result = await CreateArrowApiAsync(credentialKey, getCredentialsMethod, cache);
                var resultTyped = result as T;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(TmeApi))
            {
                var result = await CreateTmeApiAsync(credentialKey, getCredentialsMethod, cache);
                var resultTyped = result as T;
                if (resultTyped != null)
                    return resultTyped;
            }
            throw new NotImplementedException($"Unhandled type '{apiType.Name}'");
        }

        /// <summary>
        /// Create an integration Api for use by users
        /// </summary>
        /// <param name="apiType"></param>
        /// <param name="userId"></param>
        /// <param name="getCredentialsMethod"></param>
        /// <param name="cache"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IIntegrationApi> CreateAsync(Type apiType, int userId, Func<Task<ApiCredentialConfiguration>> getCredentialsMethod, bool cache = true)
        {
            var credentialKey = ApiCredentialKey.Create(userId);

            if (apiType == typeof(Integrations.SwarmApi))
            {
                var result = await CreateSwarmApiAsync(credentialKey, getCredentialsMethod, cache);
                var resultTyped = result as IIntegrationApi;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(DigikeyApi))
            {
                var result = await CreateDigikeyApiAsync(credentialKey, getCredentialsMethod, cache);
                var resultTyped = result as IIntegrationApi;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(MouserApi))
            {
                var result = await CreateMouserApiAsync(credentialKey, getCredentialsMethod, cache);
                var resultTyped = result as IIntegrationApi;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(NexarApi))
            {
                var result = await CreateNexarApiAsync(credentialKey, getCredentialsMethod, cache);
                var resultTyped = result as IIntegrationApi;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(ArrowApi))
            {
                var result = await CreateArrowApiAsync(credentialKey, getCredentialsMethod, cache);
                var resultTyped = result as IIntegrationApi;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(TmeApi))
            {
                var result = await CreateTmeApiAsync(credentialKey, getCredentialsMethod, cache);
                var resultTyped = result as IIntegrationApi;
                if (resultTyped != null)
                    return resultTyped;
            }
            throw new NotImplementedException($"Unhandled type '{apiType.Name}'");
        }

        private Integrations.SwarmApi CreateGlobalSwarmApi(IntegrationConfiguration integrationConfig)
        {
            var configuration = new SwarmConfiguration
            {
                // system settings
                Enabled = integrationConfig.Swarm.Enabled,
                ApiKey = integrationConfig.Swarm.ApiKey,
                ApiUrl = integrationConfig.Swarm.ApiUrl,
            };
            var logger = _loggerFactory.CreateLogger<Integrations.SwarmApi>();
            var api = new Integrations.SwarmApi(logger, configuration, _defaultUserConfiguration);
            return api;
        }

        private async Task<Integrations.SwarmApi> CreateSwarmApiAsync(ApiCredentialKey credentialKey, Func<Task<ApiCredentialConfiguration>> getCredentialsMethod, bool cache)
        {
            ApiCredential? credentials = null;
            if (cache)
                credentials = await _credentialProvider.Cache.GetOrAddCredentialAsync(credentialKey, nameof(SwarmApi), getCredentialsMethod);
            else
                credentials = await _credentialProvider.Cache.FetchCredentialAsync(credentialKey, nameof(SwarmApi), getCredentialsMethod);
            var configuration = new SwarmConfiguration
            {
                Enabled = credentials.GetCredentialBool("Enabled"),
                ApiKey= credentials.GetCredentialString("ApiKey"),
                ApiUrl = credentials.GetCredentialString("ApiUrl")
            };
            if (!string.IsNullOrEmpty(credentials.GetCredentialString("Timeout")) && TimeSpan.TryParse(credentials.GetCredentialString("Timeout"), out var timeout))
                configuration.Timeout = timeout;
            var logger = _loggerFactory.CreateLogger<Integrations.SwarmApi>();
            var userConfiguration = _userConfigurationService.GetCachedUserConfiguration();
            var api = new Integrations.SwarmApi(logger, configuration, userConfiguration);
            return api;
        }

        private DigikeyApi CreateGlobalDigikeyApi(IntegrationConfiguration integrationConfig)
        {
            var configuration = new DigikeyConfiguration
            {
                // system settings
                Enabled = integrationConfig.Digikey.Enabled,
                Site = integrationConfig.Digikey.Site,
                ClientId = integrationConfig.Digikey.ClientId,
                ClientSecret = integrationConfig.Digikey.ClientSecret,
                ApiUrl = integrationConfig.Digikey.ApiUrl,
                oAuthPostbackUrl = integrationConfig.Digikey.oAuthPostbackUrl,
            };
            var logger = _loggerFactory.CreateLogger<DigikeyApi>();
            var api = new DigikeyApi(logger, configuration, _defaultUserConfiguration, _credentialService, _httpContextAccessor, _requestContext, _httpClientFactory);
            return api;
        }

        private async Task<DigikeyApi> CreateDigikeyApiAsync(ApiCredentialKey credentialKey, Func<Task<ApiCredentialConfiguration>> getCredentialsMethod, bool cache)
        {
            ApiCredential? credentials = null;
            if (cache)
                credentials = await _credentialProvider.Cache.GetOrAddCredentialAsync(credentialKey, nameof(DigikeyApi), getCredentialsMethod);
            else
                credentials = await _credentialProvider.Cache.FetchCredentialAsync(credentialKey, nameof(DigikeyApi), getCredentialsMethod);
            var configuration = new DigikeyConfiguration
            {
                Enabled = credentials.GetCredentialBool("Enabled"),
                Site = (DigikeyLocaleSite)credentials.GetCredentialInt32("Site"),
                ClientId = credentials.GetCredentialString("ClientId"),
                ClientSecret = credentials.GetCredentialString("ClientSecret"),
                oAuthPostbackUrl = credentials.GetCredentialString("oAuthPostbackUrl"),
                ApiUrl = credentials.GetCredentialString("ApiUrl"),
            };
            var logger = _loggerFactory.CreateLogger<DigikeyApi>();
            var userConfiguration = _userConfigurationService.GetCachedUserConfiguration();
            var api = new DigikeyApi(logger, configuration, userConfiguration, _credentialService, _httpContextAccessor, _requestContext, _httpClientFactory);
            return api;
        }

        private MouserApi CreateGlobalMouserApi(IntegrationConfiguration integrationConfig)
        {
            var configuration = new MouserConfiguration
            {
                // system settings
                Enabled = integrationConfig.Mouser.Enabled,
                ApiKeys = new MouserApiKeys
                {
                    OrderApiKey = integrationConfig.Mouser.ApiKeys.OrderApiKey,
                    CartApiKey = integrationConfig.Mouser.ApiKeys.CartApiKey,
                    SearchApiKey = integrationConfig.Mouser.ApiKeys.SearchApiKey
                },
                ApiUrl = integrationConfig.Mouser.ApiUrl,
            };
            var logger = _loggerFactory.CreateLogger<MouserApi>();
            var api = new MouserApi(logger, configuration, _defaultUserConfiguration, _httpClientFactory);
            return api;
        }

        private async Task<MouserApi> CreateMouserApiAsync(ApiCredentialKey credentialKey, Func<Task<ApiCredentialConfiguration>> getCredentialsMethod, bool cache)
        {
            ApiCredential? credentials = null;
            if (cache)
                credentials = await _credentialProvider.Cache.GetOrAddCredentialAsync(credentialKey, nameof(MouserApi), getCredentialsMethod);
            else
                credentials = await _credentialProvider.Cache.FetchCredentialAsync(credentialKey, nameof(MouserApi), getCredentialsMethod);
            var configuration = new MouserConfiguration
            {
                Enabled = credentials.GetCredentialBool("Enabled"),
                ApiKeys = new MouserApiKeys
                {
                    OrderApiKey = credentials.GetCredentialString("OrderApiKey"),
                    CartApiKey = credentials.GetCredentialString("CartApiKey"),
                    SearchApiKey = credentials.GetCredentialString("SearchApiKey")
                },
                ApiUrl = credentials.GetCredentialString("ApiUrl"),
            };
            var logger = _loggerFactory.CreateLogger<MouserApi>();
            var userConfiguration = _userConfigurationService.GetCachedUserConfiguration();
            var api = new MouserApi(logger, configuration, userConfiguration, _httpClientFactory);
            return api;
        }

        private NexarApi CreateGlobalNexarApi(IntegrationConfiguration integrationConfig)
        {
            var configuration = new OctopartConfiguration
            {
                // system settings
                Enabled = integrationConfig.Nexar.Enabled,
                ClientId = integrationConfig.Nexar.ClientId,
                ClientSecret = integrationConfig.Nexar.ClientSecret,
            };
            var logger = _loggerFactory.CreateLogger<NexarApi>();
            var api = new NexarApi(logger, configuration, _defaultUserConfiguration);
            return api;
        }

        private async Task<NexarApi> CreateNexarApiAsync(ApiCredentialKey credentialKey, Func<Task<ApiCredentialConfiguration>> getCredentialsMethod, bool cache)
        {
            ApiCredential? credentials = null;
            if (cache)
                credentials = await _credentialProvider.Cache.GetOrAddCredentialAsync(credentialKey, nameof(NexarApi), getCredentialsMethod);
            else
                credentials = await _credentialProvider.Cache.FetchCredentialAsync(credentialKey, nameof(NexarApi), getCredentialsMethod);
            var configuration = new OctopartConfiguration
            {
                Enabled = credentials.GetCredentialBool("Enabled"),
                ClientId = credentials.GetCredentialString("ClientId"),
                ClientSecret = credentials.GetCredentialString("ClientSecret"),
            };
            var logger = _loggerFactory.CreateLogger<NexarApi>();
            var userConfiguration = _userConfigurationService.GetCachedUserConfiguration();
            var api = new NexarApi(logger, configuration, userConfiguration);
            return api;
        }

        private ArrowApi CreateGlobalArrowApi(IntegrationConfiguration integrationConfig)
        {
            var configuration = new ArrowConfiguration
            {
                // system settings
                Enabled = integrationConfig.Arrow.Enabled,
                Username = integrationConfig.Arrow.Username,
                ApiKey = integrationConfig.Arrow.ApiKey,
                ApiUrl = integrationConfig.Arrow.ApiUrl,
            };
            var logger = _loggerFactory.CreateLogger<ArrowApi>();
            var api = new ArrowApi(logger, configuration, _defaultUserConfiguration, _httpContextAccessor);
            return api;
        }

        private async Task<ArrowApi> CreateArrowApiAsync(ApiCredentialKey credentialKey, Func<Task<ApiCredentialConfiguration>> getCredentialsMethod, bool cache)
        {
            ApiCredential? credentials = null;
            if (cache)
                credentials = await _credentialProvider.Cache.GetOrAddCredentialAsync(credentialKey, nameof(ArrowApi), getCredentialsMethod);
            else
                credentials = await _credentialProvider.Cache.FetchCredentialAsync(credentialKey, nameof(ArrowApi), getCredentialsMethod);
            var configuration = new ArrowConfiguration
            {
                Enabled = credentials.GetCredentialBool("Enabled"),
                Username = credentials.GetCredentialString("Username"),
                ApiKey = credentials.GetCredentialString("ApiKey"),
                ApiUrl = credentials.GetCredentialString("ApiUrl"),
            };
            var logger = _loggerFactory.CreateLogger<ArrowApi>();
            var userConfiguration = _userConfigurationService.GetCachedUserConfiguration();
            var api = new ArrowApi(logger, configuration, userConfiguration, _httpContextAccessor);
            return api;
        }

        private TmeApi CreateGlobalTmeApi(IntegrationConfiguration integrationConfig)
        {
            var configuration = new TmeConfiguration
            {
                // system settings
                Enabled = integrationConfig.Tme.Enabled,
                Country = integrationConfig.Tme.Country,
                ApplicationSecret = integrationConfig.Tme.ApplicationSecret,
                ApiKey = integrationConfig.Tme.ApiKey,
                ApiUrl = integrationConfig.Tme.ApiUrl,
                ResolveExternalLinks = integrationConfig.Tme.ResolveExternalLinks,
            };
            var logger = _loggerFactory.CreateLogger<TmeApi>();
            var api = new TmeApi(logger, configuration, _defaultUserConfiguration, _httpClientFactory);
            return api;
        }

        private async Task<TmeApi> CreateTmeApiAsync(ApiCredentialKey credentialKey, Func<Task<ApiCredentialConfiguration>> getCredentialsMethod, bool cache)
        {
            ApiCredential? credentials = null;
            if (cache)
                credentials = await _credentialProvider.Cache.GetOrAddCredentialAsync(credentialKey, nameof(TmeApi), getCredentialsMethod);
            else
                credentials = await _credentialProvider.Cache.FetchCredentialAsync(credentialKey, nameof(TmeApi), getCredentialsMethod);
            var configuration = new TmeConfiguration
            {
                Enabled = credentials.GetCredentialBool("Enabled"),
                Country = credentials.GetCredentialString("Country"),
                ApplicationSecret = credentials.GetCredentialString("ApplicationSecret"),
                ApiKey = credentials.GetCredentialString("ApiKey"),
                ApiUrl = credentials.GetCredentialString("ApiUrl"),
                ResolveExternalLinks = credentials.GetCredentialBool("ResolveExternalLinks"),
            };
            var logger = _loggerFactory.CreateLogger<TmeApi>();
            var userConfiguration = _userConfigurationService.GetCachedUserConfiguration();
            var api = new TmeApi(logger, configuration, userConfiguration, _httpClientFactory);
            return api;
        }
    }
}
