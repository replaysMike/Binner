using Binner.Common.Configuration;
using Binner.Common.Integrations;
using Binner.Common.Models.Configuration.Integrations;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public class IntegrationApiFactory : IIntegrationApiFactory
    {
        private readonly IIntegrationCredentialsCacheProvider _credentialProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly RequestContextAccessor _requestContext;
        private readonly ICredentialService _credentialService;
        private readonly IntegrationConfiguration _integrationConfiguration;
        private readonly WebHostServiceConfiguration _webHostServiceConfiguration;

        public IntegrationApiFactory(IIntegrationCredentialsCacheProvider credentialProvider, IHttpContextAccessor httpContextAccessor, RequestContextAccessor requestContext, ICredentialService credentialService, IntegrationConfiguration integrationConfiguration, WebHostServiceConfiguration webHostServiceConfiguration)
        {
            _credentialProvider = credentialProvider;
            _httpContextAccessor = httpContextAccessor;
            _requestContext = requestContext;
            _credentialService = credentialService;
            _integrationConfiguration = integrationConfiguration;
            _webHostServiceConfiguration = webHostServiceConfiguration;
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
            if (apiType == typeof(OctopartApi))
            {
                var result = CreateGlobalOctopartApi();
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
            throw new NotImplementedException($"Unhandled type '{apiType.Name}'");
        }

        /// <summary>
        /// Create an integration Api for use by users
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<T> CreateAsync<T>(int userId)
            where T : class
        {
            var getCredentialsMethod = async () =>
            {
                // create a db context
                //using var context = await _contextFactory.CreateDbContextAsync();
                /*var userIntegrationConfiguration = await context.UserIntegrationConfigurations
                    .Where(x => x.UserId.Equals(userId))
                    .FirstOrDefaultAsync()
                    ?? new Data.DataModel.UserIntegrationConfiguration();*/
                // todo: temporary until we move integration configuration to the UI
                var userIntegrationConfiguration = new UserIntegrationConfiguration
                {
                    SwarmEnabled = _webHostServiceConfiguration.Integrations.Swarm.Enabled,
                    SwarmApiKey = _webHostServiceConfiguration.Integrations.Swarm.ApiKey,
                    SwarmApiUrl = _webHostServiceConfiguration.Integrations.Swarm.ApiUrl,
                    SwarmTimeout = _webHostServiceConfiguration.Integrations.Swarm.Timeout,

                    DigiKeyEnabled = _webHostServiceConfiguration.Integrations.Digikey.Enabled,
                    DigiKeyClientId = _webHostServiceConfiguration.Integrations.Digikey.ClientId,
                    DigiKeyClientSecret = _webHostServiceConfiguration.Integrations.Digikey.ClientSecret,
                    DigiKeyOAuthPostbackUrl = _webHostServiceConfiguration.Integrations.Digikey.oAuthPostbackUrl,
                    DigiKeyApiUrl = _webHostServiceConfiguration.Integrations.Digikey.ApiUrl,
                    
                    MouserEnabled = _webHostServiceConfiguration.Integrations.Mouser.Enabled,
                    MouserSearchApiKey = _webHostServiceConfiguration.Integrations.Mouser.ApiKeys.SearchApiKey,
                    MouserCartApiKey = _webHostServiceConfiguration.Integrations.Mouser.ApiKeys.CartApiKey,
                    MouserOrderApiKey = _webHostServiceConfiguration.Integrations.Mouser.ApiKeys.OrderApiKey,
                    MouserApiUrl = _webHostServiceConfiguration.Integrations.Mouser.ApiUrl,
                    
                    OctopartEnabled = _webHostServiceConfiguration.Integrations.Octopart.Enabled,
                    OctopartApiKey = _webHostServiceConfiguration.Integrations.Octopart.ApiKey,
                    OctopartApiUrl = _webHostServiceConfiguration.Integrations.Octopart.ApiUrl,
                };

                // build the credentials list
                var credentials = new List<ApiCredential>();

                // add user defined credentials
                var swarmConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", userIntegrationConfiguration.SwarmEnabled },
                    { "ApiKey", userIntegrationConfiguration.SwarmApiKey ?? string.Empty },
                    { "ApiUrl", userIntegrationConfiguration.SwarmApiUrl },
                    { "Timeout", userIntegrationConfiguration.SwarmTimeout },
                };
                credentials.Add(new ApiCredential(userId, swarmConfiguration, nameof(SwarmApi)));

                var digikeyConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", userIntegrationConfiguration.DigiKeyEnabled },
                    { "ClientId", userIntegrationConfiguration.DigiKeyClientId ?? string.Empty },
                    { "ClientSecret", userIntegrationConfiguration.DigiKeyClientSecret ?? string.Empty },
                    { "oAuthPostbackUrl", userIntegrationConfiguration.DigiKeyOAuthPostbackUrl },
                    { "ApiUrl", userIntegrationConfiguration.DigiKeyApiUrl }
                };
                credentials.Add(new ApiCredential(userId, digikeyConfiguration, nameof(DigikeyApi)));

                var mouserConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", userIntegrationConfiguration.MouserEnabled },
                    { "SearchApiKey", userIntegrationConfiguration.MouserSearchApiKey ?? string.Empty },
                    { "CartApiKey", userIntegrationConfiguration.MouserCartApiKey ?? string.Empty },
                    { "OrderApiKey", userIntegrationConfiguration.MouserOrderApiKey ?? string.Empty },
                    { "ApiUrl", userIntegrationConfiguration.MouserApiUrl },
                };
                credentials.Add(new ApiCredential(userId, mouserConfiguration, nameof(MouserApi)));

                var arrowConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", userIntegrationConfiguration.ArrowEnabled },
                    { "Username", userIntegrationConfiguration.ArrowUsername ?? string.Empty },
                    { "ApiKey", userIntegrationConfiguration.ArrowApiKey ?? string.Empty },
                    { "ApiUrl", userIntegrationConfiguration.ArrowApiUrl },
                };
                credentials.Add(new ApiCredential(userId, arrowConfiguration, nameof(ArrowApi)));

                var octopartConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", userIntegrationConfiguration.OctopartEnabled },
                    { "ApiKey", userIntegrationConfiguration.OctopartApiKey ?? string.Empty },
                    { "ApiUrl", userIntegrationConfiguration.OctopartApiUrl }
                };
                credentials.Add(new ApiCredential(userId, octopartConfiguration, nameof(OctopartApi)));

                return new ApiCredentialConfiguration(userId, credentials);
            };
            return await CreateAsync<T>(userId, getCredentialsMethod, true);
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
            if (apiType == typeof(OctopartApi))
            {
                var result = await CreateOctopartApiAsync(credentialKey, getCredentialsMethod, cache);
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
            throw new NotImplementedException($"Unhandled type '{apiType.Name}'");
        }

        private Integrations.SwarmApi CreateGlobalSwarmApi()
        {
            var configuration = new SwarmConfiguration
            {
                // system settings
                Enabled = _integrationConfiguration.Swarm.Enabled,
                ApiKey = _integrationConfiguration.Swarm.ApiKey,
                ApiUrl = _integrationConfiguration.Swarm.ApiUrl,
            };
            var api = new Integrations.SwarmApi(configuration, _credentialService, _httpContextAccessor, _requestContext);
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
            var api = new Integrations.SwarmApi(configuration, _credentialService, _httpContextAccessor, _requestContext);
            return api;
        }

        private DigikeyApi CreateGlobalDigikeyApi()
        {
            var configuration = new DigikeyConfiguration
            {
                // system settings
                Enabled = _integrationConfiguration.Digikey.Enabled,
                ClientId = _integrationConfiguration.Digikey.ClientId,
                ClientSecret = _integrationConfiguration.Digikey.ClientSecret,
                ApiUrl = _integrationConfiguration.Digikey.ApiUrl,
                oAuthPostbackUrl = _integrationConfiguration.Digikey.oAuthPostbackUrl,
            };
            var api = new DigikeyApi(configuration, _credentialService, _httpContextAccessor, _requestContext);
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
                ClientId = credentials.GetCredentialString("ClientId"),
                ClientSecret = credentials.GetCredentialString("ClientSecret"),
                oAuthPostbackUrl = credentials.GetCredentialString("oAuthPostbackUrl"),
                ApiUrl = credentials.GetCredentialString("ApiUrl"),
            };
            var api = new DigikeyApi(configuration, _credentialService, _httpContextAccessor, _requestContext);
            return api;
        }

        private MouserApi CreateGlobalMouserApi()
        {
            var configuration = new MouserConfiguration
            {
                // system settings
                Enabled = _integrationConfiguration.Mouser.Enabled,
                ApiKeys = new MouserApiKeys
                {
                    OrderApiKey = _integrationConfiguration.Mouser.ApiKeys.OrderApiKey,
                    CartApiKey = _integrationConfiguration.Mouser.ApiKeys.CartApiKey,
                    SearchApiKey = _integrationConfiguration.Mouser.ApiKeys.SearchApiKey
                },
                ApiUrl = _integrationConfiguration.Mouser.ApiUrl,
            };
            var api = new MouserApi(configuration, _httpContextAccessor);
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
            var api = new MouserApi(configuration, _httpContextAccessor);
            return api;
        }

        private OctopartApi CreateGlobalOctopartApi()
        {
            var configuration = new OctopartConfiguration
            {
                // system settings
                Enabled = _integrationConfiguration.Octopart.Enabled,
                ApiKey = _integrationConfiguration.Octopart.ApiKey,
                ApiUrl = _integrationConfiguration.Octopart.ApiUrl,
            };
            var api = new OctopartApi(configuration, _httpContextAccessor);
            return api;
        }

        private async Task<OctopartApi> CreateOctopartApiAsync(ApiCredentialKey credentialKey, Func<Task<ApiCredentialConfiguration>> getCredentialsMethod, bool cache)
        {
            ApiCredential? credentials = null;
            if (cache)
                credentials = await _credentialProvider.Cache.GetOrAddCredentialAsync(credentialKey, nameof(OctopartApi), getCredentialsMethod);
            else
                credentials = await _credentialProvider.Cache.FetchCredentialAsync(credentialKey, nameof(OctopartApi), getCredentialsMethod);
            var configuration = new OctopartConfiguration
            {
                Enabled = credentials.GetCredentialBool("Enabled"),
                ApiKey = credentials.GetCredentialString("ApiKey"),
                ApiUrl = credentials.GetCredentialString("ApiUrl"),
            };
            var api = new OctopartApi(configuration, _httpContextAccessor);
            return api;
        }

        private ArrowApi CreateGlobalArrowApi()
        {
            var configuration = new ArrowConfiguration
            {
                // system settings
                Enabled = _integrationConfiguration.Arrow.Enabled,
                Username = _integrationConfiguration.Arrow.Username,
                ApiKey = _integrationConfiguration.Arrow.ApiKey,
                ApiUrl = _integrationConfiguration.Arrow.ApiUrl,
            };
            var api = new ArrowApi(configuration, _httpContextAccessor);
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
            var api = new ArrowApi(configuration, _httpContextAccessor);
            return api;
        }
    }
}
