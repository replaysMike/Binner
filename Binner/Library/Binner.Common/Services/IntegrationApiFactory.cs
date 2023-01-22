using Binner.Common.Integrations;
using Binner.Common.Models.Configuration;
using Binner.Common.Models.Configuration.Integrations;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public IntegrationApiFactory(IIntegrationCredentialsCacheProvider credentialProvider, IHttpContextAccessor httpContextAccessor, RequestContextAccessor requestContext, ICredentialService credentialService, IntegrationConfiguration integrationConfiguration)
        {
            _credentialProvider = credentialProvider;
            _httpContextAccessor = httpContextAccessor;
            _requestContext = requestContext;
            _credentialService = credentialService;
            _integrationConfiguration = integrationConfiguration;
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

            if (apiType == typeof(SwarmApi))
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
            var apiType = typeof(T);
            var credentialKey = ApiCredentialKey.Create(userId);
            var getCredentialsMethod = async () =>
            {
                // create a db context
                //using var context = await _contextFactory.CreateDbContextAsync();
                /*var userIntegrationConfiguration = await context.UserIntegrationConfigurations
                    .Where(x => x.UserId.Equals(userId))
                    .FirstOrDefaultAsync()
                    ?? new Data.DataModel.UserIntegrationConfiguration();*/
                var userIntegrationConfiguration = new UserIntegrationConfiguration(); // todo: finish

                // build the credentials list
                var credentials = new List<ApiCredential>();

                // add user defined credentials
                var swarmConfiguration = new Dictionary<string, object?>();
                swarmConfiguration.Add("Enabled", userIntegrationConfiguration.SwarmEnabled);
                credentials.Add(new ApiCredential(userId, swarmConfiguration, nameof(SwarmApi)));

                var digikeyConfiguration = new Dictionary<string, object?>();
                digikeyConfiguration.Add("Enabled", userIntegrationConfiguration.DigiKeyEnabled);
                credentials.Add(new ApiCredential(userId, digikeyConfiguration, nameof(DigikeyApi)));

                var mouserConfiguration = new Dictionary<string, object?>();
                mouserConfiguration.Add("Enabled", userIntegrationConfiguration.MouserEnabled);
                mouserConfiguration.Add("CartApiKey", userIntegrationConfiguration.MouserCartApiKey);
                mouserConfiguration.Add("OrderApiKey", userIntegrationConfiguration.MouserOrderApiKey);
                credentials.Add(new ApiCredential(userId, mouserConfiguration, nameof(MouserApi)));

                var octopartConfiguration = new Dictionary<string, object?>();
                octopartConfiguration.Add("Enabled", userIntegrationConfiguration.OctopartEnabled);
                octopartConfiguration.Add("ApiKey", userIntegrationConfiguration.OctopartApiKey);
                octopartConfiguration.Add("ApiUrl", userIntegrationConfiguration.OctopartApiUrl);
                credentials.Add(new ApiCredential(userId, octopartConfiguration, nameof(OctopartApi)));

                return new ApiCredentialConfiguration(userId, credentials);
            };

            if (apiType == typeof(SwarmApi))
            {
                var result = await CreateSwarmApiAsync(credentialKey, getCredentialsMethod);
                var resultTyped = result as T;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(DigikeyApi))
            {
                var result = await CreateDigikeyApiAsync(credentialKey, getCredentialsMethod);
                var resultTyped = result as T;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(MouserApi))
            {
                var result = await CreateMouserApiAsync(credentialKey, getCredentialsMethod);
                var resultTyped = result as T;
                if (resultTyped != null)
                    return resultTyped;
            }
            if (apiType == typeof(OctopartApi))
            {
                var result = await CreateOctopartApiAsync(credentialKey, getCredentialsMethod);
                var resultTyped = result as T;
                if (resultTyped != null)
                    return resultTyped;
            }
            throw new NotImplementedException($"Unhandled type '{apiType.Name}'");
        }

        private SwarmApi CreateGlobalSwarmApi()
        {
            var configuration = new SwarmConfiguration
            {
                // system settings
                Enabled = _integrationConfiguration.Swarm.Enabled,
            };
            var api = new SwarmApi(configuration, _credentialService, _httpContextAccessor, _requestContext);
            return api;
        }

        private async Task<SwarmApi> CreateSwarmApiAsync(ApiCredentialKey credentialKey, Func<Task<ApiCredentialConfiguration>> getCredentialsMethod)
        {
            var credentials = await _credentialProvider.Cache.GetOrAddCredentialAsync(credentialKey, nameof(SwarmApi), getCredentialsMethod);
            var configuration = new SwarmConfiguration
            {
                // user can disable the api
                Enabled = _integrationConfiguration.Swarm.Enabled && credentials.GetCredentialBool("Enabled"),
            };
            var api = new SwarmApi(configuration, _credentialService, _httpContextAccessor, _requestContext);
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

        private async Task<DigikeyApi> CreateDigikeyApiAsync(ApiCredentialKey credentialKey, Func<Task<ApiCredentialConfiguration>> getCredentialsMethod)
        {
            var credentials = await _credentialProvider.Cache.GetOrAddCredentialAsync(credentialKey, nameof(DigikeyApi), getCredentialsMethod);
            var configuration = new DigikeyConfiguration
            {
                // user can disable the api
                Enabled = _integrationConfiguration.Digikey.Enabled && credentials.GetCredentialBool("Enabled"),

                // system settings
                // Digikey has no user level keys as it supports oAuth
                ClientId = _integrationConfiguration.Digikey.ClientId,
                ClientSecret = _integrationConfiguration.Digikey.ClientSecret,
                oAuthPostbackUrl = _integrationConfiguration.Digikey.oAuthPostbackUrl,
                ApiUrl = _integrationConfiguration.Digikey.ApiUrl,
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

        private async Task<MouserApi> CreateMouserApiAsync(ApiCredentialKey credentialKey, Func<Task<ApiCredentialConfiguration>> getCredentialsMethod)
        {
            var credentials = await _credentialProvider.Cache.GetOrAddCredentialAsync(credentialKey, nameof(MouserApi), getCredentialsMethod);
            var configuration = new MouserConfiguration
            {
                // user can disable the api
                Enabled = _integrationConfiguration.Mouser.Enabled && credentials.GetCredentialBool("Enabled"),

                ApiKeys = new MouserApiKeys
                {
                    // user level keys
                    OrderApiKey = credentials.GetCredentialString("OrderApiKey"),
                    CartApiKey = credentials.GetCredentialString("CartApiKey"),
                    // system key is used
                    SearchApiKey = _integrationConfiguration.Mouser.ApiKeys.SearchApiKey
                },
                // system settings
                ApiUrl = _integrationConfiguration.Mouser.ApiUrl,
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

        private async Task<OctopartApi> CreateOctopartApiAsync(ApiCredentialKey credentialKey, Func<Task<ApiCredentialConfiguration>> getCredentialsMethod)
        {
            var credentials = await _credentialProvider.Cache.GetOrAddCredentialAsync(credentialKey, nameof(OctopartApi), getCredentialsMethod);
            var configuration = new OctopartConfiguration
            {
                // user can disable the api
                Enabled = _integrationConfiguration.Octopart.Enabled && credentials.GetCredentialBool("Enabled"),

                // user level keys
                ApiKey = credentials.GetCredentialString("ApiKey"),
                ApiUrl = credentials.GetCredentialString("ApiUrl"),
            };
            var api = new OctopartApi(configuration, _httpContextAccessor);
            return api;
        }
    }
}
