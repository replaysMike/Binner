using ApiClient.OAuth2;
using Binner.Common;
using Binner.Common.Extensions;
using Binner.Common.Integrations;
using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations;
using Binner.Model.Integrations.DigiKey;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Web;

namespace Binner.Services.Integrations
{
    /// <summary>
    /// DigiKey api supports both V3 and V4 APIs
    /// </summary>
    public class DigikeyApi : BaseDigikeyApi, IIntegrationApi
    {
        public static readonly TimeSpan MaxAuthorizationWaitTime = TimeSpan.FromSeconds(30);
        public string Name => "DigiKey";

        private readonly ILogger<DigikeyApi> _logger;
        private readonly DigikeyConfiguration _configuration;
        private readonly UserConfiguration _userConfiguration;
        private readonly OAuth2Service _oAuth2Service;
        private readonly ICredentialService _credentialService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRequestContextAccessor _requestContext;
        protected DigiKeyApiVersion _apiVersion = DigiKeyApiVersion.V4; // default api returned
        protected IDigikeyApi? _api;
        protected readonly DigikeyV3Api _v3Api;
        protected readonly DigikeyV4Api _v4Api;

        private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            // ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            Converters = new List<JsonConverter> { new StringEnumConverter() }
        };

        /// <summary>
        /// Get the OAuth2 service associated with this api
        /// </summary>
        public OAuth2Service OAuth2Service => _oAuth2Service;

        public bool IsEnabled => _configuration.Enabled;

        public IApiConfiguration Configuration => _configuration;

        public DigikeyApi(ILogger<DigikeyApi> logger, DigikeyConfiguration configuration, UserConfiguration userConfiguration, ICredentialService credentialService, IHttpContextAccessor httpContextAccessor, IRequestContextAccessor requestContext, IApiHttpClientFactory httpClientFactory)
            : base(logger, configuration, userConfiguration, _serializerSettings, httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _userConfiguration = userConfiguration;
            _oAuth2Service = new OAuth2Service(configuration, logger);
            _credentialService = credentialService;
            _httpContextAccessor = httpContextAccessor;
            _requestContext = requestContext;
            _v3Api = new DigikeyV3Api(_logger, _configuration, _userConfiguration, _serializerSettings, httpClientFactory);
            _v4Api = new DigikeyV4Api(_logger, _configuration, _userConfiguration, _serializerSettings, httpClientFactory);
        }

        private IDigikeyApi UseApi(DigiKeyApiVersion version)
        {
            switch (version)
            {
                case DigiKeyApiVersion.V3:
                    _apiVersion = version;
                    _api = _v3Api;
                    break;
                case DigiKeyApiVersion.V4:
                    _apiVersion = version;
                    _api = _v4Api;
                    break;
                default:
                    throw new NotImplementedException($"API version {version} not implemented.");
            }
            return _api;
        }

        /// <summary>
        /// Get the cached api to use, or get it from the stored OAuthCredential ApiSetting value (last used)
        /// </summary>
        /// <returns></returns>
        private async Task<IDigikeyApi> GetApiAsync()
        {
            if (_api != null) return _api;
            var (existingCredential, apiSettings) = await GetOAuthCredentialAsync();
            return UseApi(apiSettings.ApiVersion);
        }

        private void ValidateConfiguration()
        {
            if (_configuration.Enabled)
            {
                if (string.IsNullOrWhiteSpace(_configuration.ClientId)) throw new BinnerConfigurationException("DigiKey API ClientId cannot be empty!");
                if (string.IsNullOrWhiteSpace(_configuration.ClientSecret)) throw new BinnerConfigurationException("DigiKey API ClientSecret cannot be empty!");
                if (string.IsNullOrWhiteSpace(_configuration.oAuthPostbackUrl)) throw new BinnerConfigurationException("DigiKey API oAuthPostbackUrl cannot be empty!");
                if (string.IsNullOrWhiteSpace(_configuration.ApiUrl)) throw new BinnerConfigurationException("DigiKey API ApiUrl cannot be empty!");
            }
        }

        /// <summary>
        /// Get a DigiKey order
        /// V3 & V4 supported
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="additionalOptions"></param>
        /// <returns></returns>
        public async Task<IApiResponse> GetOrderAsync(string orderId, Dictionary<string, string>? additionalOptions = null)
        {
            ValidateConfiguration();

            var authResponse = await AuthorizeAsync();
            if (!authResponse.IsAuthorized)
                return ApiResponse.Create(true, authResponse.AuthorizationUrl, $"User must authorize", nameof(DigikeyApi));

            return await WrapApiRequestAsync(authResponse, async (authenticationResponse) =>
            {
                /* important reminder - don't reference authResponse in here! */
                return await (await GetApiAsync()).GetOrderAsync(authenticationResponse, orderId);
            });
        }

        /// <summary>
        /// Get details about a single part
        /// V3 & V4 supported
        /// </summary>
        /// <param name="partNumber"></param>
        /// <param name="additionalOptions"></param>
        /// <returns></returns>
        public async Task<IApiResponse> GetProductDetailsAsync(string partNumber, Dictionary<string, string>? additionalOptions = null)
        {
            ValidateConfiguration();

            var authResponse = await AuthorizeAsync();
            if (!authResponse.IsAuthorized)
            {
                _logger.LogInformation($"[{nameof(GetProductDetailsAsync)}] User must authorize.");
                return ApiResponse.Create(true, authResponse.AuthorizationUrl, $"User must authorize", nameof(DigikeyApi));
            }

            return await WrapApiRequestAsync(authResponse, async (authenticationResponse) =>
            {
                /* important reminder - don't reference authResponse in here! */
                return await (await GetApiAsync()).GetProductDetailsAsync(authenticationResponse, partNumber);
            });
        }

        /// <summary>
        /// Get information about a DigiKey product via a barcode value.
        /// V3 only - there is no V4 Barcode api
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="barcodeType"></param>
        /// <returns></returns>
        public async Task<IApiResponse> GetBarcodeDetailsAsync(string barcode, ScannedLabelType barcodeType)
        {
            ValidateConfiguration();

            var authResponse = await AuthorizeAsync();
            if (!authResponse.IsAuthorized)
                return ApiResponse.Create(true, authResponse.AuthorizationUrl, $"User must authorize", nameof(DigikeyApi));

            return await WrapApiRequestAsync(authResponse, async (authenticationResponse) =>
            {
                /* important reminder - don't reference authResponse in here! */
                return await (await GetApiAsync()).GetBarcodeDetailsAsync(authenticationResponse, barcode, barcodeType);
            });
        }

        /// <summary>
        /// Get DigiKey categories.
        /// V3 & V4 supported
        /// </summary>
        /// <returns></returns>
        public async Task<IApiResponse> GetCategoriesAsync()
        {
            ValidateConfiguration();

            var authResponse = await AuthorizeAsync();
            if (!authResponse.IsAuthorized)
            {
                _logger.LogInformation($"[{nameof(GetCategoriesAsync)}] User must authorize.");
                return ApiResponse.Create(true, authResponse.AuthorizationUrl, $"User must authorize", nameof(DigikeyApi));
            }

            return await WrapApiRequestAsync(authResponse, async (authenticationResponse) =>
            {
                /* important reminder - don't reference authResponse in here! */
                return await (await GetApiAsync()).GetCategoriesAsync(authenticationResponse);
            });
        }

        public Task<IApiResponse> SearchAsync(string partNumber, int recordCount = ApiConstants.MaxRecords, Dictionary<string, string>? additionalOptions = null) => SearchAsync(partNumber, string.Empty, string.Empty, recordCount, additionalOptions);

        public Task<IApiResponse> SearchAsync(string partNumber, string? partType, int recordCount = ApiConstants.MaxRecords, Dictionary<string, string>? additionalOptions = null) => SearchAsync(partNumber, partType, string.Empty, recordCount, additionalOptions);

        /// <summary>
        /// Search for a part by part number
        /// V3 & V4 supported
        /// </summary>
        /// <param name="partNumber"></param>
        /// <param name="partType"></param>
        /// <param name="mountingType"></param>
        /// <param name="recordCount"></param>
        /// <param name="additionalOptions"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public async Task<IApiResponse> SearchAsync(string partNumber, string? partType, string? mountingType, int recordCount = ApiConstants.MaxRecords, Dictionary<string, string>? additionalOptions = null)
        {
            ValidateConfiguration();

            if (!(recordCount > 0)) throw new ArgumentOutOfRangeException(nameof(recordCount));
            var authResponse = await AuthorizeAsync();
            if (!authResponse.IsAuthorized)
            {
                _logger.LogInformation($"[{nameof(SearchAsync)}] User must authorize - '{authResponse.AccessToken.Sanitize()}' is invalid.");
                return ApiResponse.Create(true, authResponse.AuthorizationUrl, $"User must authorize", nameof(DigikeyApi));
            }

            return await WrapApiRequestAsync(authResponse, async (authenticationResponse) =>
            {
                /* important reminder - don't reference authResponse in here! */
                return await (await GetApiAsync()).SearchAsync(authenticationResponse, partNumber, partType, mountingType, recordCount, additionalOptions);
            });
        }

        private async Task SetOAuthCredentialApiSettingsAsync(DigiKeyApiVersion? apiVersion = null)
        {
            var (credential, apiSettings) = await GetOAuthCredentialAsync();
            if (credential != null)
            {
                // set the stored oauth credential to the specified api version
                if (apiVersion != null)
                    credential.ApiSettings = CreateApiSettingsJson(apiVersion.Value);
                await _credentialService.SaveOAuthCredentialAsync(credential);
            }
        }

        private async Task<(OAuthCredential? oAuthCredential, DigiKeyCredentialApiSettings apiSettings)> GetOAuthCredentialAsync()
        {
            var apiSettings = new DigiKeyCredentialApiSettings
            {
                ApiVersion = _apiVersion,
            };
            var credential = await _credentialService.GetOAuthCredentialAsync(nameof(DigikeyApi));
            if (credential != null)
            {
                if (!string.IsNullOrEmpty(credential.ApiSettings))
                {
                    try
                    {
                        var serializedValue = JsonConvert.DeserializeObject<DigiKeyCredentialApiSettings?>(credential.ApiSettings);
                        if (serializedValue != null)
                            apiSettings = serializedValue;
                    }
                    catch (Exception)
                    {
                        _logger.LogWarning($"[{nameof(GetOAuthCredentialAsync)}] Failed to deserialize DigiKey OAuthCredentials.ApiSettings value. '{credential.ApiSettings}'");
                    }
                }
                return (credential, apiSettings);
            }
            return (null, apiSettings);
        }

        /// <summary>
        /// Wraps an API request - if the request is unauthorized it will refresh the Auth token and re-issue the request
        /// </summary>
        /// <param name="authResponse"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        private async Task<IApiResponse> WrapApiRequestAsync(OAuthAuthorization authResponse, Func<OAuthAuthorization, Task<IApiResponse>> func)
        {
            try
            {
                return await func(authResponse);
            }
            catch (DigikeyUnsubscribedException ex)
            {
                // the api key used is not subscribed to the requested endpoint. Could be a version mismatch, try the other api version.
                //var attemptedErrorApiVersion = new Version(ex.ErrorResponseVersion);

                //if (VersionEquals(attemptedApiVersion, DigiKeyApiVersion.V3))
                if (ex.ApiVersion == DigiKeyApiVersion.V3)
                {
                    // try the V4 API
                    UseApi(DigiKeyApiVersion.V4);
                    try
                    {
                        var response = await func(authResponse);
                        // success using this api, save it as the default
                        await SetOAuthCredentialApiSettingsAsync(_apiVersion);
                        return response;
                    }
                    catch (DigikeyUnauthorizedException)
                    {
                        // get refresh token, retry
                        return await RefreshTokenAsync(ex.Authorization, func);
                    }
                    catch (DigikeyUnsubscribedException)
                    {
                        // flow through to the error
                    }
                }
                //else if (VersionEquals(attemptedApiVersion, DigiKeyApiVersion.V4))
                else if (ex.ApiVersion == DigiKeyApiVersion.V4)
                {
                    // try the V3 API
                    UseApi(DigiKeyApiVersion.V3);
                    try
                    {
                        var response = await func(authResponse);
                        // success using this api, save it as the default
                        await SetOAuthCredentialApiSettingsAsync(_apiVersion);
                        return response;
                    }
                    catch (DigikeyUnauthorizedException)
                    {
                        // get refresh token, retry
                        return await RefreshTokenAsync(ex.Authorization, func);
                    }
                    catch (DigikeyUnsubscribedException)
                    {
                        // flow through to the error
                    }
                }

                // return unsubscribed error, user's api key is not subscribed to this endpoint
                _logger.LogInformation($"[{nameof(WrapApiRequestAsync)}] Request using token '{ex.Authorization.AccessToken.Sanitize()}' is unsubscribed for endpoint.");
                return ApiResponse.Create($"{ex.Message} Api v{(int)ex.ApiVersion}", nameof(DigikeyApi));
            }
            catch (DigikeyUnauthorizedException ex)
            {
                // get refresh token, retry
                return await RefreshTokenAsync(ex.Authorization, func);
            }
        }

        private async Task<IApiResponse> RefreshTokenAsync(OAuthAuthorization authorization, Func<OAuthAuthorization, Task<IApiResponse>> func)
        {
            _logger.LogInformation($"[{nameof(WrapApiRequestAsync)}] Request using token '{authorization.AccessToken.Sanitize()}' is unauthorized, trying to Refresh token using '{authorization.RefreshToken.Sanitize()}'");
            _oAuth2Service.AccessTokens.RefreshToken = authorization.RefreshToken;
            var refreshedTokens = await _oAuth2Service.RefreshTokenAsync();
            if (refreshedTokens.IsError)
            {
                _logger.LogError($"[{nameof(WrapApiRequestAsync)}] Refresh token failed.");
            }
            else
            {
                // refresh token successfully got a new token. Let's use it.
                if (_httpContextAccessor.HttpContext == null)
                    throw new Exception($"HttpContext cannot be null!");
                var referer = GetReferer();
                var refreshedTokenResponse = new OAuthAuthorization(nameof(DigikeyApi), _configuration.ClientId ?? string.Empty, referer)
                {
                    AccessToken = refreshedTokens.AccessToken ?? string.Empty,
                    RefreshToken = refreshedTokens.RefreshToken ?? string.Empty,
                    CreatedUtc = DateTime.UtcNow,
                    ExpiresUtc = DateTime.UtcNow.Add(TimeSpan.FromSeconds(refreshedTokens.ExpiresIn)),
                    AuthorizationReceived = true,
                    UserId = _requestContext.GetUserContext()?.UserId,
                    OrganizationId = _requestContext.GetUserContext()?.OrganizationId,
                };
                if (refreshedTokenResponse.IsAuthorized) // IsAuthorized is a computed field based on the response
                {
                    _logger.LogInformation($"[{nameof(WrapApiRequestAsync)}] Refresh token succeeded, new token '{refreshedTokenResponse.AccessToken.Sanitize()}', old was '{authorization.AccessToken.Sanitize()}'");
                    // save the credential
                    var (existingCredential, apiSettings) = await GetOAuthCredentialAsync();
                    await _credentialService.SaveOAuthCredentialAsync(new OAuthCredential
                    {
                        Provider = nameof(DigikeyApi),
                        AccessToken = refreshedTokenResponse.AccessToken,
                        RefreshToken = refreshedTokenResponse.RefreshToken,
                        DateCreatedUtc = refreshedTokenResponse.CreatedUtc,
                        DateExpiresUtc = refreshedTokenResponse.ExpiresUtc,
                        // update api settings to use the last successful api version
                        ApiSettings = CreateApiSettingsJson(_apiVersion)
                    });
                    try
                    {
                        _logger.LogInformation($"[{nameof(WrapApiRequestAsync)}] Re-calling method with refreshed accesstoken='{refreshedTokenResponse.AccessToken.Sanitize()}'");

                        // call the API again using the newly refreshed token
                        return await func(refreshedTokenResponse);
                    }
                    catch (DigikeyUnauthorizedException)
                    {
                        // refresh token failed, restart access token retrieval process
                        await ForgetAuthenticationTokens();
                        var freshResponse = await AuthorizeAsync();
                        if (freshResponse.MustAuthorize)
                            return ApiResponse.Create(true, freshResponse.AuthorizationUrl, $"User must authorize", nameof(DigikeyApi));

                        // call the API again
                        return await func(freshResponse);
                    }
                }
            }

            // user must authorize
            // request a token if we don't already have one
            var authRequest = await CreateOAuthAuthorizationRequestAsync(_requestContext.GetUserContext());
            _logger.LogInformation($"[{nameof(WrapApiRequestAsync)}] Refresh token failed, User must authorize");
            return ApiResponse.Create(true, authRequest.AuthorizationUrl, $"User must authorize", nameof(DigikeyApi));
        }

        private DigiKeyCredentialApiSettings CreateApiSettings(DigiKeyApiVersion version)
        {
            return new DigiKeyCredentialApiSettings(version);
        }

        private string CreateApiSettingsJson(DigiKeyApiVersion version)
        {
            return JsonConvert.SerializeObject(CreateApiSettings(version));
        }

        private async Task ForgetAuthenticationTokens()
        {
            var user = _requestContext.GetUserContext();
            await _credentialService.RemoveOAuthCredentialAsync(nameof(DigikeyApi));
        }

        private async Task<OAuthAuthorization> AuthorizeAsync()
        {
            var user = _requestContext.GetUserContext() ?? throw new System.Security.Authentication.AuthenticationException("User is not authenticated!");

            // check if we have saved an existing auth credential in the database
            var (credential, apiSettings) = await GetOAuthCredentialAsync();
            if (credential != null)
            {
                // set the appropriate api (if saved)
                if (apiSettings.ApiVersion != _apiVersion)
                {
                    UseApi(apiSettings.ApiVersion);
                    _logger.LogInformation($"[{nameof(AuthorizeAsync)}] Using saved {nameof(DigikeyApi)} API version '{apiSettings.ApiVersion}'");
                }

                // reuse a saved oAuth credential
                var referer = GetReferer();
                var authRequest = new OAuthAuthorization(nameof(DigikeyApi), _configuration.ClientId ?? string.Empty, referer)
                {
                    AccessToken = credential.AccessToken ?? string.Empty,
                    RefreshToken = credential.RefreshToken ?? string.Empty,
                    CreatedUtc = credential.DateCreatedUtc,
                    ExpiresUtc = credential.DateExpiresUtc,
                    AuthorizationReceived = true,
                    UserId = user?.UserId,
                    OrganizationId = user?.OrganizationId
                };
                _logger.LogInformation($"[{nameof(AuthorizeAsync)}] Reusing a saved oAuth credential '{credential.AccessToken.Sanitize()}'!");

                return authRequest;
            }

            // user must authorize
            // request a token if we don't already have one
            return await CreateOAuthAuthorizationRequestAsync(user);
        }

        private async Task<OAuthAuthorization> CreateOAuthAuthorizationRequestAsync(IUserContext userContext)
        {
            var referer = GetReferer();
            var uriBuilder = new UriBuilder(referer);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["api-authenticate"] = "true";
            query["api"] = "DigiKey";
            uriBuilder.Query = query.ToString();
            var authRequest = new OAuthAuthorization(nameof(DigikeyApi), _configuration.ClientId ?? string.Empty, uriBuilder.ToString())
            {
                UserId = userContext.UserId,
                OrganizationId = userContext.OrganizationId
            };
            authRequest = await _credentialService.CreateOAuthRequestAsync(authRequest);
            // no scopes necessary
            var scopes = "";
            // state will be send as the RequestId
            var state = authRequest.Id.ToString();
            var authUrl = _oAuth2Service.GenerateAuthUrl(scopes, state);
            _logger.LogInformation($"[{nameof(CreateOAuthAuthorizationRequestAsync)}] Creating a new OAuthRequest '{state}'. No existing OAuthCredential was found.");
            return new OAuthAuthorization(nameof(DigikeyApi), true, authUrl);
        }

        private string GetReferer()
        {
            var uri = new Uri(_configuration.oAuthPostbackUrl);
            var referer = $"{uri.Scheme}://{uri.Host}{(uri.Port > 0 ? ":" + uri.Port : "")}";
            if (_httpContextAccessor.HttpContext?.Request?.Headers?.ContainsKey("Referer") == true)
                referer = _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString();
            return referer;
        }

        public override string ToString()
            => $"{nameof(DigikeyApi)} {_apiVersion}";

        public override void Dispose()
        {
            base.Dispose();
            _client.Dispose();
        }
    }
}