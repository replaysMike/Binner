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
        private readonly WebHostServiceConfiguration _webHostServiceConfiguration;
        private readonly IUserConfigurationService _userConfigurationService;

        public IntegrationApiFactory(ILoggerFactory loggerFactory, IMapper mapper, IIntegrationCredentialsCacheProvider credentialProvider, IHttpContextAccessor httpContextAccessor, IRequestContextAccessor requestContext, ICredentialService credentialService, WebHostServiceConfiguration webHostServiceConfiguration, IApiHttpClientFactory httpClientFactory, IUserConfigurationService userConfigurationService)
        {
            _loggerFactory = loggerFactory;
            _mapper = mapper;
            _credentialProvider = credentialProvider;
            _httpContextAccessor = httpContextAccessor;
            _requestContext = requestContext;
            _credentialService = credentialService;
            _webHostServiceConfiguration = webHostServiceConfiguration;
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
        public T CreateGlobal<T>()
            where T : class
        {
            var apiType = typeof(T);

            if (apiType == typeof(Integrations.SwarmApi))
            {
                var result = CreateGlobalSwarmApi();
                var resultTyped = result as T;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(DigikeyApi))
            {
                var result = CreateGlobalDigikeyApi();
                var resultTyped = result as T;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(MouserApi))
            {
                var result = CreateGlobalMouserApi();
                var resultTyped = result as T;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(NexarApi))
            {
                var result = CreateGlobalNexarApi();
                var resultTyped = result as T;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(ArrowApi))
            {
                var result = CreateGlobalArrowApi();
                var resultTyped = result as T;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(TmeApi))
            {
                var result = CreateGlobalTmeApi();
                var resultTyped = result as T;
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
                // create a db context
                //await using var context = await _contextFactory.CreateDbContextAsync();
                /*var userIntegrationConfiguration = await context.UserIntegrationConfigurations
                    .Where(x => x.UserId.Equals(userId))
                    .FirstOrDefaultAsync()
                    ?? new Data.DataModel.UserIntegrationConfiguration();*/
                // todo: temporary until we move integration configuration to the UI
                /*var userIntegrationConfiguration = new UserIntegrationConfiguration
                {
                    SwarmEnabled = _webHostServiceConfiguration.Integrations.Swarm.Enabled,
                    SwarmApiKey = _webHostServiceConfiguration.Integrations.Swarm.ApiKey,
                    SwarmApiUrl = _webHostServiceConfiguration.Integrations.Swarm.ApiUrl,
                    SwarmTimeout = _webHostServiceConfiguration.Integrations.Swarm.Timeout,

                    DigiKeyEnabled = _webHostServiceConfiguration.Integrations.Digikey.Enabled,
                    DigiKeySite = _webHostServiceConfiguration.Integrations.Digikey.Site,
                    DigiKeyClientId = _webHostServiceConfiguration.Integrations.Digikey.ClientId,
                    DigiKeyClientSecret = _webHostServiceConfiguration.Integrations.Digikey.ClientSecret,
                    DigiKeyOAuthPostbackUrl = _webHostServiceConfiguration.Integrations.Digikey.oAuthPostbackUrl,
                    DigiKeyApiUrl = _webHostServiceConfiguration.Integrations.Digikey.ApiUrl,
                    
                    MouserEnabled = _webHostServiceConfiguration.Integrations.Mouser.Enabled,
                    MouserSearchApiKey = _webHostServiceConfiguration.Integrations.Mouser.ApiKeys.SearchApiKey,
                    MouserCartApiKey = _webHostServiceConfiguration.Integrations.Mouser.ApiKeys.CartApiKey,
                    MouserOrderApiKey = _webHostServiceConfiguration.Integrations.Mouser.ApiKeys.OrderApiKey,
                    MouserApiUrl = _webHostServiceConfiguration.Integrations.Mouser.ApiUrl,

                    ArrowEnabled = _webHostServiceConfiguration.Integrations.Arrow.Enabled,
                    ArrowUsername = _webHostServiceConfiguration.Integrations.Arrow.Username,
                    ArrowApiKey = _webHostServiceConfiguration.Integrations.Arrow.ApiKey,
                    ArrowApiUrl = _webHostServiceConfiguration.Integrations.Arrow.ApiUrl,
                    
                    NexarEnabled = _webHostServiceConfiguration.Integrations.Nexar.Enabled,
                    NexarClientId = _webHostServiceConfiguration.Integrations.Nexar.ClientId,
                    NexarClientSecret = _webHostServiceConfiguration.Integrations.Nexar.ClientSecret,

                    TmeEnabled = _webHostServiceConfiguration.Integrations.Tme.Enabled,
                    TmeApplicationSecret = _webHostServiceConfiguration.Integrations.Tme.ApplicationSecret,
                    TmeApiKey = _webHostServiceConfiguration.Integrations.Tme.ApiKey,
                    TmeApiUrl = _webHostServiceConfiguration.Integrations.Tme.ApiUrl,
                    TmeResolveExternalLinks = _webHostServiceConfiguration.Integrations.Tme.ResolveExternalLinks,
                };*/

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
                // create a db context
                //await using var context = await _contextFactory.CreateDbContextAsync();
                /*var userIntegrationConfiguration = await context.UserIntegrationConfigurations
                    .Where(x => x.UserId.Equals(userId))
                    .FirstOrDefaultAsync()
                    ?? new Data.DataModel.UserIntegrationConfiguration();*/
                // todo: temporary until we move integration configuration to the UI
                /*var userIntegrationConfiguration = new UserIntegrationConfiguration
                {
                    SwarmEnabled = _webHostServiceConfiguration.Integrations.Swarm.Enabled,
                    SwarmApiKey = _webHostServiceConfiguration.Integrations.Swarm.ApiKey,
                    SwarmApiUrl = _webHostServiceConfiguration.Integrations.Swarm.ApiUrl,
                    SwarmTimeout = _webHostServiceConfiguration.Integrations.Swarm.Timeout,

                    DigiKeyEnabled = _webHostServiceConfiguration.Integrations.Digikey.Enabled,
                    DigiKeySite = _webHostServiceConfiguration.Integrations.Digikey.Site,
                    DigiKeyClientId = _webHostServiceConfiguration.Integrations.Digikey.ClientId,
                    DigiKeyClientSecret = _webHostServiceConfiguration.Integrations.Digikey.ClientSecret,
                    DigiKeyOAuthPostbackUrl = _webHostServiceConfiguration.Integrations.Digikey.oAuthPostbackUrl,
                    DigiKeyApiUrl = _webHostServiceConfiguration.Integrations.Digikey.ApiUrl,

                    MouserEnabled = _webHostServiceConfiguration.Integrations.Mouser.Enabled,
                    MouserSearchApiKey = _webHostServiceConfiguration.Integrations.Mouser.ApiKeys.SearchApiKey,
                    MouserCartApiKey = _webHostServiceConfiguration.Integrations.Mouser.ApiKeys.CartApiKey,
                    MouserOrderApiKey = _webHostServiceConfiguration.Integrations.Mouser.ApiKeys.OrderApiKey,
                    MouserApiUrl = _webHostServiceConfiguration.Integrations.Mouser.ApiUrl,

                    ArrowEnabled = _webHostServiceConfiguration.Integrations.Arrow.Enabled,
                    ArrowUsername = _webHostServiceConfiguration.Integrations.Arrow.Username,
                    ArrowApiKey = _webHostServiceConfiguration.Integrations.Arrow.ApiKey,
                    ArrowApiUrl = _webHostServiceConfiguration.Integrations.Arrow.ApiUrl,

                    NexarEnabled = _webHostServiceConfiguration.Integrations.Nexar.Enabled,
                    NexarClientId = _webHostServiceConfiguration.Integrations.Nexar.ClientId,
                    NexarClientSecret = _webHostServiceConfiguration.Integrations.Nexar.ClientSecret,

                    TmeEnabled = _webHostServiceConfiguration.Integrations.Tme.Enabled,
                    TmeApplicationSecret = _webHostServiceConfiguration.Integrations.Tme.ApplicationSecret,
                    TmeApiKey = _webHostServiceConfiguration.Integrations.Tme.ApiKey,
                    TmeApiUrl = _webHostServiceConfiguration.Integrations.Tme.ApiUrl,
                    TmeResolveExternalLinks = _webHostServiceConfiguration.Integrations.Tme.ResolveExternalLinks,
                };*/

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

        private Integrations.SwarmApi CreateGlobalSwarmApi()
        {
            var configuration = new SwarmConfiguration
            {
                // system settings
                Enabled = _webHostServiceConfiguration.Integrations.Swarm.Enabled,
                ApiKey = _webHostServiceConfiguration.Integrations.Swarm.ApiKey,
                ApiUrl = _webHostServiceConfiguration.Integrations.Swarm.ApiUrl,
            };
            var logger = _loggerFactory.CreateLogger<Integrations.SwarmApi>();
            var userConfiguration = _userConfigurationService.GetCachedUserConfiguration();
            var api = new Integrations.SwarmApi(logger, configuration, userConfiguration);
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
                ApiUrl = credentials.GetCredentialString("ApiUrl"),
                Timeout = TimeSpan.Parse(credentials.GetCredentialString("Timeout"))
            };
            var logger = _loggerFactory.CreateLogger<Integrations.SwarmApi>();
            var userConfiguration = _userConfigurationService.GetCachedUserConfiguration();
            var api = new Integrations.SwarmApi(logger, configuration, userConfiguration);
            return api;
        }

        private DigikeyApi CreateGlobalDigikeyApi()
        {
            var integrationConfig = _userConfigurationService.GetCachedOrganizationIntegrationConfiguration();
            var configuration = new DigikeyConfiguration
            {
                // system settings
                Enabled = integrationConfig.DigiKeyEnabled,
                Site = integrationConfig.DigiKeySite,
                ClientId = integrationConfig.DigiKeyClientId,
                ClientSecret = integrationConfig.DigiKeyClientSecret,
                ApiUrl = integrationConfig.DigiKeyApiUrl,
                oAuthPostbackUrl = integrationConfig.DigiKeyOAuthPostbackUrl,
            };
            var logger = _loggerFactory.CreateLogger<DigikeyApi>();
            var userConfiguration = _userConfigurationService.GetCachedUserConfiguration();
            var api = new DigikeyApi(logger, configuration, userConfiguration, _credentialService, _httpContextAccessor, _requestContext, _httpClientFactory);
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

        private MouserApi CreateGlobalMouserApi()
        {
            var integrationConfiguration = _mapper.Map<IntegrationConfiguration>(_userConfigurationService.GetCachedOrganizationIntegrationConfiguration());
            var configuration = new MouserConfiguration
            {
                // system settings
                Enabled = integrationConfiguration.Mouser.Enabled,
                ApiKeys = new MouserApiKeys
                {
                    OrderApiKey = integrationConfiguration.Mouser.ApiKeys.OrderApiKey,
                    CartApiKey = integrationConfiguration.Mouser.ApiKeys.CartApiKey,
                    SearchApiKey = integrationConfiguration.Mouser.ApiKeys.SearchApiKey
                },
                ApiUrl = integrationConfiguration.Mouser.ApiUrl,
            };
            var logger = _loggerFactory.CreateLogger<MouserApi>();
            var userConfiguration = _userConfigurationService.GetCachedUserConfiguration();
            var api = new MouserApi(logger, configuration, userConfiguration, _httpClientFactory);
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

        private NexarApi CreateGlobalNexarApi()
        {
            var integrationConfiguration = _mapper.Map<IntegrationConfiguration>(_userConfigurationService.GetCachedOrganizationIntegrationConfiguration());
            var configuration = new OctopartConfiguration
            {
                // system settings
                Enabled = integrationConfiguration.Nexar.Enabled,
                ClientId = integrationConfiguration.Nexar.ClientId,
                ClientSecret = integrationConfiguration.Nexar.ClientSecret,
            };
            var logger = _loggerFactory.CreateLogger<NexarApi>();
            var userConfiguration = _userConfigurationService.GetCachedUserConfiguration();
            var api = new NexarApi(logger, configuration, userConfiguration);
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

        private ArrowApi CreateGlobalArrowApi()
        {
            var integrationConfiguration = _mapper.Map<IntegrationConfiguration>(_userConfigurationService.GetCachedOrganizationIntegrationConfiguration());
            var configuration = new ArrowConfiguration
            {
                // system settings
                Enabled = integrationConfiguration.Arrow.Enabled,
                Username = integrationConfiguration.Arrow.Username,
                ApiKey = integrationConfiguration.Arrow.ApiKey,
                ApiUrl = integrationConfiguration.Arrow.ApiUrl,
            };
            var logger = _loggerFactory.CreateLogger<ArrowApi>();
            var userConfiguration = _userConfigurationService.GetCachedUserConfiguration();
            var api = new ArrowApi(logger, configuration, userConfiguration, _httpContextAccessor);
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

        private TmeApi CreateGlobalTmeApi()
        {
            var integrationConfiguration = _mapper.Map<IntegrationConfiguration>(_userConfigurationService.GetCachedOrganizationIntegrationConfiguration());
            var configuration = new TmeConfiguration
            {
                // system settings
                Enabled = integrationConfiguration.Tme.Enabled,
                Country = integrationConfiguration.Tme.Country,
                ApplicationSecret = integrationConfiguration.Tme.ApplicationSecret,
                ApiKey = integrationConfiguration.Tme.ApiKey,
                ApiUrl = integrationConfiguration.Tme.ApiUrl,
                ResolveExternalLinks = integrationConfiguration.Tme.ResolveExternalLinks,
            };
            var logger = _loggerFactory.CreateLogger<TmeApi>();
            var userConfiguration = _userConfigurationService.GetCachedUserConfiguration();
            var api = new TmeApi(logger, configuration, userConfiguration, _httpClientFactory);
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
