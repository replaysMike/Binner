using ApiClient.OAuth2;
using Binner.Common.Extensions;
using Binner.Common.Integrations.Models;
using Binner.Common.Services;
using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations.DigiKey;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using TypeSupport.Extensions;

namespace Binner.Common.Integrations
{
    public class DigikeyApi : IIntegrationApi
    {
        public static readonly TimeSpan MaxAuthorizationWaitTime = TimeSpan.FromSeconds(30);
        public string Name => "DigiKey";

        #region Regex Matching
        private readonly Regex PercentageRegex = new Regex("^\\d{0,4}(\\.\\d{0,4})? *%?$", RegexOptions.Compiled);
        private readonly Regex PowerRegex = new Regex("^(\\d+[\\/\\d. ]*[Ww]$|\\d*[Ww]$)", RegexOptions.Compiled);
        private readonly Regex ResistanceRegex = new Regex("^(\\d+[\\d. ]*[KkMm]$|\\d*[KkMm]$|\\d*(?i)ohm(?-i)$)", RegexOptions.Compiled);
        private readonly Regex CapacitanceRegex = new Regex("^\\d+\\.?\\d*(uf|pf|mf|f)$", RegexOptions.Compiled);
        private readonly Regex VoltageRegex = new Regex("^\\d+\\.?\\d*(v|mv)$", RegexOptions.Compiled);
        private readonly Regex CurrentRegex = new Regex("^\\d+\\.?\\d*(a|ma)$", RegexOptions.Compiled);
        private readonly Regex InductanceRegex = new Regex("^\\d+\\.?\\d*(nh|uh|h)$", RegexOptions.Compiled);
        #endregion

        // the full url to the Api
        private readonly DigikeyConfiguration _configuration;
        private readonly LocaleConfiguration _localeConfiguration;
        private readonly OAuth2Service _oAuth2Service;
        private readonly ICredentialService _credentialService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly RequestContextAccessor _requestContext;
        private readonly HttpClient _client;
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
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

        public DigikeyApi(DigikeyConfiguration configuration, LocaleConfiguration localeConfiguration, ICredentialService credentialService, IHttpContextAccessor httpContextAccessor, RequestContextAccessor requestContext)
        {
            _configuration = configuration;
            _localeConfiguration = localeConfiguration;
            _oAuth2Service = new OAuth2Service(configuration);
            _credentialService = credentialService;
            _httpContextAccessor = httpContextAccessor;
            _client = new HttpClient();
            _requestContext = requestContext;
        }

        public enum MountingTypes
        {
            None = 0,
            SurfaceMount = 3,
            ThroughHole = 80
        }

        public async Task<IApiResponse> GetOrderAsync(string orderId, Dictionary<string, string>? additionalOptions = null)
        {
            var authResponse = await AuthorizeAsync();
            if (!authResponse.IsAuthorized)
                return ApiResponse.Create(true, authResponse.AuthorizationUrl, $"User must authorize", nameof(DigikeyApi));
            return await WrapApiRequestAsync(authResponse, async (authenticationResponse) =>
            {
                try
                {
                    // set what fields we want from the API
                    var uri = Url.Combine(_configuration.ApiUrl, "OrderDetails/v3/Status/", orderId);
                    var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Get, uri);
                    // perform a keywords API search
                    var response = await _client.SendAsync(requestMessage);
                    if (TryHandleResponse(response, authenticationResponse, out var apiResponse))
                    {
                        return apiResponse;
                    }

                    // 200 OK
                    var resultString = response.Content.ReadAsStringAsync().Result;
                    var results = JsonConvert.DeserializeObject<OrderSearchResponse>(resultString, _serializerSettings) ?? new();
                    return new ApiResponse(results, nameof(DigikeyApi));
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }

        public async Task<IApiResponse> GetProductDetailsAsync(string partNumber, Dictionary<string, string>? additionalOptions = null)
        {
            var authResponse = await AuthorizeAsync();
            if (!authResponse.IsAuthorized)
                return ApiResponse.Create(true, authResponse.AuthorizationUrl, $"User must authorize", nameof(DigikeyApi));
            return await WrapApiRequestAsync(authResponse, async (authenticationResponse) =>
            {
                try
                {
                    // set what fields we want from the API
                    var uri = Url.Combine(_configuration.ApiUrl, "Search/v3/Products/", HttpUtility.UrlEncode(partNumber));
                    var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Get, uri);
                    // perform a keywords API search
                    var response = await _client.SendAsync(requestMessage);
                    if (TryHandleResponse(response, authenticationResponse, out var apiResponse))
                    {
                        return apiResponse;
                    }

                    // 200 OK
                    var resultString = response.Content.ReadAsStringAsync().Result;
                    var results = JsonConvert.DeserializeObject<Product>(resultString, _serializerSettings) ?? new();
                    return new ApiResponse(results, nameof(DigikeyApi));
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }

        /// <summary>
        /// Get information about a DigiKey product via a barcode value
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="barcodeType"></param>
        /// <returns></returns>
        public async Task<IApiResponse> GetBarcodeDetailsAsync(string barcode, ScannedBarcodeType barcodeType)
        {
            var authResponse = await AuthorizeAsync();
            if (!authResponse.IsAuthorized)
                return ApiResponse.Create(true, authResponse.AuthorizationUrl, $"User must authorize", nameof(DigikeyApi));
            return await WrapApiRequestAsync(authResponse, async (authenticationResponse) =>
            {
                try
                {
                    // set what fields we want from the API
                    // https://developer.digikey.com/products/barcode/barcoding/productbarcode

                    var is2dBarcode = barcode.StartsWith("[)>");
                    var endpoint = "Barcoding/v3/ProductBarcodes/";
                    switch (barcodeType)
                    {
                        case ScannedBarcodeType.Product:
                        default:
                            endpoint = "Barcoding/v3/ProductBarcodes/";
                            if (is2dBarcode)
                                endpoint = "Barcoding/v3/Product2DBarcodes/";
                            break;
                        case ScannedBarcodeType.Packlist:
                            endpoint = "Barcoding/v3/PackListBarcodes/";
                            if (is2dBarcode)
                                endpoint = "Barcoding/v3/PackList2DBarcodes/";
                            break;
                    }

                    var barcodeFormatted = barcode.ToString();
                    if (is2dBarcode)
                    {
                        // DigiKey requires the GS (Group separator) to be \u241D, and the RS (Record separator) to be \u241E
                        // GS
                        var gsReplacement = "\u241D";
                        barcodeFormatted = barcodeFormatted.Replace("\u001d", gsReplacement);
                        barcodeFormatted = barcodeFormatted.Replace("\u005d", gsReplacement);
                        // RS
                        var rsReplacement = "\u241E";
                        barcodeFormatted = barcodeFormatted.Replace("\u001e", rsReplacement);
                        barcodeFormatted = barcodeFormatted.Replace("\u005e", rsReplacement);

                    }
                    var barcodeFormattedPath = HttpUtility.UrlEncode(barcodeFormatted);

                    //var uri = Url.Combine(_configuration.ApiUrl, endpoint, barcodeFormatted);
                    //var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Get, uri);
                    var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Get, string.Join("/", _configuration.ApiUrl, endpoint) + barcodeFormattedPath);
                    // perform a keywords API search
                    var response = await _client.SendAsync(requestMessage);
                    if (TryHandleResponse(response, authenticationResponse, out var apiResponse))
                    {
                        var contentString = response.Content.ReadAsStringAsync().Result;
                        apiResponse.Errors.Add(contentString);
                        return apiResponse;
                    }

                    // 200 OK
                    var resultString = response.Content.ReadAsStringAsync().Result;
                    var results = JsonConvert.DeserializeObject<ProductBarcodeResponse>(resultString, _serializerSettings) ?? new();
                    return new ApiResponse(results, nameof(DigikeyApi));
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }

        public async Task<IApiResponse> GetCategoriesAsync()
        {
            var authResponse = await AuthorizeAsync();
            if (!authResponse.IsAuthorized)
                return ApiResponse.Create(true, authResponse.AuthorizationUrl, $"User must authorize", nameof(DigikeyApi));
            return await WrapApiRequestAsync(authResponse, async (authenticationResponse) =>
            {
                try
                {
                    // set what fields we want from the API
                    var uri = Url.Combine(_configuration.ApiUrl, "Search/v3/Categories");
                    var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Get, uri);
                    // perform a keywords API search
                    var response = await _client.SendAsync(requestMessage);
                    if (TryHandleResponse(response, authenticationResponse, out var apiResponse))
                    {
                        return apiResponse;
                    }

                    // 200 OK
                    var resultString = response.Content.ReadAsStringAsync().Result;
                    var results = JsonConvert.DeserializeObject<CategoriesResponse>(resultString, _serializerSettings) ?? new();
                    return new ApiResponse(results, nameof(DigikeyApi));
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }

        public Task<IApiResponse> SearchAsync(string partNumber, int recordCount = 25, Dictionary<string, string>? additionalOptions = null) => SearchAsync(partNumber, string.Empty, string.Empty, recordCount, additionalOptions);

        public Task<IApiResponse> SearchAsync(string partNumber, string? partType, int recordCount = 25, Dictionary<string, string>? additionalOptions = null) => SearchAsync(partNumber, partType, string.Empty, recordCount, additionalOptions);

        public async Task<IApiResponse> SearchAsync(string partNumber, string? partType, string? mountingType, int recordCount = 25, Dictionary<string, string>? additionalOptions = null)
        {
            if (!(recordCount > 0)) throw new ArgumentOutOfRangeException(nameof(recordCount));
            var authResponse = await AuthorizeAsync();
            if (!authResponse.IsAuthorized)
                return ApiResponse.Create(true, authResponse.AuthorizationUrl, $"User must authorize", nameof(DigikeyApi));

            var keywords = new List<string>();
            if (!string.IsNullOrEmpty(partNumber))
                keywords = partNumber.ToLower().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var packageTypeEnum = MountingTypes.None;
            if (!string.IsNullOrEmpty(mountingType))
            {
                switch (mountingType.ToLower())
                {
                    case "surface mount":
                        packageTypeEnum = MountingTypes.SurfaceMount;
                        break;
                    case "through hole":
                        packageTypeEnum = MountingTypes.ThroughHole;
                        break;
                }
            }

            return await WrapApiRequestAsync(authResponse, async (authenticationResponse) =>
            {
                try
                {
                    // set what fields we want from the API
                    var includes = new List<string> {
                        "DigiKeyPartNumber",
                        "QuantityAvailable",
                        "Manufacturer",
                        "ManufacturerPartNumber",
                        "PrimaryDatasheet",
                        "ProductDescription",
                        "DetailedDescription",
                        "MinimumOrderQuantity",
                        "NonStock",
                        "UnitPrice",
                        "ProductStatus",
                        "ProductUrl",
                        "PrimaryPhoto",
                        "PrimaryVideo",
                        "Packaging",
                        "AlternatePackaging",
                        "Family",
                        "Category",
                        "Parameters"
                        };
                    var values = new Dictionary<string, string>
                    {
                        { "Includes", $"Products({string.Join(",", includes)})" },
                    };
                    var uri = Url.Combine(_configuration.ApiUrl, "/Search/v3/Products", $"/Keyword?" + string.Join("&", values.Select(x => $"{x.Key}={x.Value}")));
                    var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Post, uri);
                    var taxonomies = MapTaxonomies(partType, packageTypeEnum);
                    var parametricFilters = MapParametricFilters(keywords, packageTypeEnum, taxonomies);
                    var request = new KeywordSearchRequest
                    {
                        Keywords = string.Join(" ", keywords),
                        RecordCount = recordCount,
                        Filters = new Filters
                        {
                            TaxonomyIds = taxonomies.Select(x => (int)x).ToList(),
                            ParametricFilters = parametricFilters
                        },
                        SearchOptions = new List<SearchOptions> { }
                    };
                    var json = JsonConvert.SerializeObject(request, _serializerSettings);
                    requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    // perform a keywords API search
                    var response = await _client.SendAsync(requestMessage);
                    if (TryHandleResponse(response, authenticationResponse, out var apiResponse))
                    {
                        return apiResponse;
                    }

                    // 200 OK
                    var resultString = response.Content.ReadAsStringAsync().Result;
                    var results = JsonConvert.DeserializeObject<KeywordSearchResponse>(resultString, _serializerSettings) ?? new();
                    return new ApiResponse(results, nameof(DigikeyApi));
                }
                catch (UnauthorizedAccessException)
                {
                    // refresh token likely expired, need to re-authenticate
                    throw new DigikeyUnauthorizedException(authenticationResponse);
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }

        private ICollection<ParametricFilter> MapParametricFilters(ICollection<string> keywords, MountingTypes packageType, ICollection<Taxonomies> taxonomies)
        {
            var filters = new List<ParametricFilter>();
            var percent = "";
            var power = "";
            var resistance = "";
            var capacitance = "";
            var voltageRating = "";
            var currentRating = "";
            var inductance = "";
            foreach (var keyword in keywords)
            {
                if (PercentageRegex.IsMatch(keyword))
                    percent = keyword;
                if (PowerRegex.IsMatch(keyword))
                    power = keyword;
                if (ResistanceRegex.IsMatch(keyword))
                    resistance = keyword;
                if (CapacitanceRegex.IsMatch(keyword))
                    capacitance = keyword;
                if (VoltageRegex.IsMatch(keyword))
                    voltageRating = keyword;
                if (CurrentRegex.IsMatch(keyword))
                    currentRating = keyword;
                if (InductanceRegex.IsMatch(keyword))
                    inductance = keyword;
            }
            // add tolerance parameter
            if (keywords.Contains("precision") || !string.IsNullOrEmpty(percent))
            {
                if (keywords.Contains("precision"))
                    keywords.Remove("precision");
                if (keywords.Contains(percent))
                    keywords.Remove(percent);
                else
                    percent = "1%";
                var filter = new ParametricFilter
                {
                    ParameterId = (int)Parametrics.Tolerance,
                    ValueId = ((int)GetTolerance(percent)).ToString()
                };
                filters.Add(filter);
            }
            if (!string.IsNullOrEmpty(power))
            {
                keywords.Remove(power);
                var filter = new ParametricFilter
                {
                    ParameterId = (int)Parametrics.Power,
                    ValueId = GetPower(power)
                };
                filters.Add(filter);
            }
            if (!string.IsNullOrEmpty(resistance))
            {
                keywords.Remove(resistance);
                var filter = new ParametricFilter
                {
                    ParameterId = (int)Parametrics.Resistance,
                    ValueId = GetResistance(resistance)
                };
                filters.Add(filter);
            }
            if (!string.IsNullOrEmpty(capacitance))
            {
                keywords.Remove(capacitance);
                var filter = new ParametricFilter
                {
                    ParameterId = (int)Parametrics.Capacitance,
                    ValueId = GetCapacitance(capacitance)
                };
                filters.Add(filter);
            }
            if (!string.IsNullOrEmpty(voltageRating))
            {
                keywords.Remove(voltageRating);
                var filter = new ParametricFilter
                {
                    ParameterId = (int)Parametrics.VoltageRating,
                    ValueId = GetVoltageRating(voltageRating)
                };
                filters.Add(filter);
            }
            if (!string.IsNullOrEmpty(currentRating))
            {
                keywords.Remove(currentRating);
                var filter = new ParametricFilter
                {
                    ParameterId = (int)Parametrics.CurrentRating,
                    ValueId = GetVoltageRating(currentRating)
                };
                filters.Add(filter);
            }
            if (!string.IsNullOrEmpty(inductance))
            {
                keywords.Remove(inductance);
                var filter = new ParametricFilter
                {
                    ParameterId = (int)Parametrics.Inductance,
                    ValueId = GetInductance(inductance)
                };
                filters.Add(filter);
            }
            // dont add mounting type to resistors, they dont seem to be mapped
            if (!taxonomies.ContainsAny(new List<Taxonomies> { Taxonomies.Resistor, Taxonomies.SurfaceMountResistor, Taxonomies.ThroughHoleResistor }))
            {
                if (packageType != MountingTypes.None)
                    filters.Add(new ParametricFilter
                    {
                        ParameterId = (int)Parametrics.MountingType,
                        ValueId = ((int)packageType).ToString()
                    });
            }
            return filters;
        }

        private ICollection<Taxonomies> MapTaxonomies(string partType, MountingTypes packageType)
        {
            var taxonomies = new List<Taxonomies>();
            var taxonomy = Taxonomies.None;
            if (!string.IsNullOrEmpty(partType) && partType != "-1")
            {
                if (Enum.TryParse<Taxonomies>(partType, true, out taxonomy))
                {
                    var addBaseType = true;
                    // also map all the alternates
                    var memberInfos = typeof(Taxonomies).GetMember(taxonomy.ToString());
                    var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == typeof(Taxonomies));
                    if (enumValueMemberInfo != null)
                    {
                        var valueAttributes = enumValueMemberInfo.GetCustomAttributes(typeof(AlternatesAttribute), false);
                        if (valueAttributes.Any())
                        {
                            var alternateIds = ((AlternatesAttribute)valueAttributes[0]).Ids;
                            // taxonomies.AddRange(alternateIds);
                        }
                    }

                    switch (taxonomy)
                    {
                        case Taxonomies.Resistor:
                            if (packageType == MountingTypes.ThroughHole)
                            {
                                taxonomies.Add(Taxonomies.ThroughHoleResistor);
                                addBaseType = false;
                            }
                            if (packageType == MountingTypes.SurfaceMount)
                            {
                                taxonomies.Add(Taxonomies.SurfaceMountResistor);
                                addBaseType = false;
                            }
                            break;
                    }
                    if (addBaseType)
                        taxonomies.Add(taxonomy);
                }
            }

            return taxonomies;
        }

        /// <summary>
        /// Handle known error conditions first, if response is OK false will be returned
        /// </summary>
        /// <param name="response"></param>
        /// <param name="authenticationResponse"></param>
        /// <param name="apiResponse"></param>
        /// <returns></returns>
        /// <exception cref="DigikeyUnauthorizedException"></exception>
        private bool TryHandleResponse(HttpResponseMessage response, OAuthAuthorization authenticationResponse, out IApiResponse apiResponse)
        {
            apiResponse = ApiResponse.Create($"Api returned error status code {response.StatusCode}: {response.ReasonPhrase}", nameof(DigikeyApi));
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new DigikeyUnauthorizedException(authenticationResponse);
            else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                if (response.Headers.Contains("X-RateLimit-Limit"))
                {
                    // throttled
                    var remainingTime = TimeSpan.Zero;
                    if (response.Headers.Contains("X-RateLimit-Remaining"))
                        remainingTime = TimeSpan.FromSeconds(int.Parse(response.Headers.GetValues("X-RateLimit-Remaining").First()));
                    apiResponse = ApiResponse.Create($"{nameof(DigikeyApi)} request throttled. Try again in {remainingTime}", nameof(DigikeyApi));
                    return true;
                }

                // return generic error
                return true;
            }
            else if (response.IsSuccessStatusCode)
            {
                // allow processing of response
                return false;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                apiResponse = ApiResponse.Create($"Api returned error status code {response.StatusCode}: {response.ReasonPhrase}", nameof(DigikeyApi));
                var resultString = response.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(resultString))
                    apiResponse.Errors.Add(resultString);
                else
                    apiResponse.Errors.Add($"Api returned error status code {response.StatusCode}: {response.ReasonPhrase}");
                return true;
            }

            // return generic error
            return true;
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
            catch (DigikeyUnauthorizedException ex)
            {
                // get refresh token, retry
                _oAuth2Service.AccessTokens.RefreshToken = ex.Authorization.RefreshToken;
                var token = await _oAuth2Service.RefreshTokenAsync();
                if (_httpContextAccessor.HttpContext == null)
                    throw new Exception($"HttpContext cannot be null!");
                var referer = _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString();
                var refreshTokenResponse = new OAuthAuthorization(nameof(DigikeyApi), _configuration.ClientId ?? string.Empty, referer)
                {
                    AccessToken = token.AccessToken ?? string.Empty,
                    RefreshToken = token.RefreshToken ?? string.Empty,
                    CreatedUtc = DateTime.UtcNow,
                    ExpiresUtc = DateTime.UtcNow.Add(TimeSpan.FromSeconds(token.ExpiresIn)),
                    AuthorizationReceived = true,
                    UserId = _requestContext.GetUserContext()?.UserId,
                };
                if (refreshTokenResponse.IsAuthorized)
                {
                    // save the credential
                    await _credentialService.SaveOAuthCredentialAsync(new OAuthCredential
                    {
                        Provider = nameof(DigikeyApi),
                        AccessToken = refreshTokenResponse.AccessToken,
                        RefreshToken = refreshTokenResponse.RefreshToken,
                        DateCreatedUtc = refreshTokenResponse.CreatedUtc,
                        DateExpiresUtc = refreshTokenResponse.ExpiresUtc,
                    });
                    try
                    {
                        // call the API again using the refresh token
                        return await func(refreshTokenResponse);
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
                // user must authorize
                // request a token if we don't already have one
                var authRequest = await CreateOAuthAuthorizationRequestAsync(_requestContext.GetUserContext()?.UserId);
                return ApiResponse.Create(true, authRequest.AuthorizationUrl, $"User must authorize", nameof(DigikeyApi));
            }
        }

        private async Task ForgetAuthenticationTokens()
        {
            var user = _requestContext.GetUserContext();
            await _credentialService.RemoveOAuthCredentialAsync(nameof(DigikeyApi));
        }

        private async Task<OAuthAuthorization> AuthorizeAsync()
        {
            var user = _requestContext.GetUserContext();
            if (user != null && user.UserId <= 0)
                throw new AuthenticationException("User is not authenticated!");

            // check if we have saved an existing auth credential in the database
            var credential = await _credentialService.GetOAuthCredentialAsync(nameof(DigikeyApi));
            if (credential != null)
            {
                // reuse a saved oAuth credential
                var referer = _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString();
                var authRequest = new OAuthAuthorization(nameof(DigikeyApi), _configuration.ClientId ?? string.Empty, referer)
                {
                    AccessToken = credential.AccessToken ?? string.Empty,
                    RefreshToken = credential.RefreshToken ?? string.Empty,
                    CreatedUtc = credential.DateCreatedUtc,
                    ExpiresUtc = credential.DateExpiresUtc,
                    AuthorizationReceived = true,
                    UserId = user?.UserId
                };

                return authRequest;
            }

            // user must authorize
            // request a token if we don't already have one
            return await CreateOAuthAuthorizationRequestAsync(user?.UserId);
        }

        private async Task<OAuthAuthorization> CreateOAuthAuthorizationRequestAsync(int? userId)
        {
            var referer = _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString();
            var uriBuilder = new UriBuilder(referer);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["api-authenticate"] = "true";
            query["api"] = "DigiKey";
            uriBuilder.Query = query.ToString();
            var authRequest = new OAuthAuthorization(nameof(DigikeyApi), _configuration.ClientId ?? string.Empty, uriBuilder.ToString())
            {
                UserId = userId
            };
            authRequest = await _credentialService.CreateOAuthRequestAsync(authRequest);
            // no scopes necessary
            var scopes = "";
            // state will be send as the RequestId
            var state = authRequest.Id.ToString();
            var authUrl = _oAuth2Service.GenerateAuthUrl(scopes, state);

            return new OAuthAuthorization(nameof(DigikeyApi), true, authUrl);
        }

        private HttpRequestMessage CreateRequest(OAuthAuthorization authResponse, HttpMethod method, string url)
        {
            var message = new HttpRequestMessage(method, url);
            message.Headers.Add("X-DIGIKEY-Client-Id", authResponse.ClientId);
            message.Headers.Add("Authorization", $"Bearer {authResponse.AccessToken}");
            message.Headers.Add("X-DIGIKEY-Locale-Site", _configuration.Site.ToString());
            message.Headers.Add("X-DIGIKEY-Locale-Language", _localeConfiguration.Language.ToString().ToLower());
            message.Headers.Add("X-DIGIKEY-Locale-Currency", _localeConfiguration.Currency.ToString().ToUpper());
            return message;
        }

        private HttpRequestMessage CreateRequest(OAuthAuthorization authResponse, HttpMethod method, Uri uri)
        {
            var message = new HttpRequestMessage(method, uri);
            message.Headers.Add("X-DIGIKEY-Client-Id", authResponse.ClientId);
            message.Headers.Add("Authorization", $"Bearer {authResponse.AccessToken}");
            message.Headers.Add("X-DIGIKEY-Locale-Site", _configuration.Site.ToString());
            message.Headers.Add("X-DIGIKEY-Locale-Language", _localeConfiguration.Language.ToString().ToLower());
            message.Headers.Add("X-DIGIKEY-Locale-Currency", _localeConfiguration.Currency.ToString().ToUpper());
            return message;
        }

        private Tolerances GetTolerance(string perc)
        {
            return GetEnumByDescription<Tolerances>(perc);
        }

        private string GetPower(string power)
        {
            power = new Regex("[Ww]|").Replace(power, "");
            // convert decimal percentages to fractions
            if (power.Contains("."))
            {
                var fraction = MathExtensions.RealToFraction(double.Parse(power), 0.01);
                return ((int)GetEnumByDescription<Power>($"{fraction.Numerator}/{fraction.Denominator}")).ToString();
            }
            return ((int)GetEnumByDescription<Power>(power)).ToString();
        }

        private string GetResistance(string resistance)
        {
            var val = new String(resistance.Where(x => Char.IsDigit(x) || Char.IsPunctuation(x)).ToArray());
            if (string.IsNullOrEmpty(val))
                val = "0";
            var unitsParsed = resistance.Replace(val, "").ToLower();
            var units = "ohms";
            switch (unitsParsed)
            {
                case "k":
                case "kohms":
                    units = "kOhms";
                    break;
                case "m":
                case "mohms":
                    units = "mOhms";
                    break;
            }
            var result = $"u{val} {units}";
            return result;
        }

        private string GetCapacitance(string capacitance)
        {
            var val = new String(capacitance.Where(x => Char.IsDigit(x) || Char.IsPunctuation(x)).ToArray());
            var unitsParsed = capacitance.Replace(val, "").ToLower();
            var units = "µF";
            switch (unitsParsed)
            {
                case "uf":
                    units = "µF";
                    break;
                case "nf":
                    // convert to uf, api doesn't seem to handle it?
                    val = (decimal.Parse(val) * 0.001M).ToString();
                    units = "µF";
                    break;
                case "pf":
                    units = "pF";
                    break;
                case "f":
                    units = "F";
                    break;
            }
            var result = $"u{val}{units}";
            return result;
        }

        private string GetVoltageRating(string voltage)
        {
            var val = new String(voltage.Where(x => Char.IsDigit(x) || Char.IsPunctuation(x)).ToArray());
            var unitsParsed = voltage.Replace(val, "").ToLower();
            var units = "V";
            switch (unitsParsed)
            {
                case "v":
                    units = "V";
                    break;
            }
            var result = $"u{val}{units}";
            return result;
        }

        private string GetCurrentRating(string current)
        {
            var val = new String(current.Where(x => Char.IsDigit(x) || Char.IsPunctuation(x)).ToArray());
            var unitsParsed = current.Replace(val, "").ToLower();
            var units = "A";
            switch (unitsParsed)
            {
                case "a":
                    units = "A";
                    break;
                case "ma":
                    units = "mA";
                    break;
            }
            var result = $"u{val}{units}";
            return result;
        }

        private string GetInductance(string inductance)
        {
            var val = new String(inductance.Where(x => Char.IsDigit(x) || Char.IsPunctuation(x)).ToArray());
            var unitsParsed = inductance.Replace(val, "").ToLower();
            var units = "µH";
            switch (unitsParsed)
            {
                case "uh":
                    units = "µH";
                    break;
                case "nh":
                    units = "nH";
                    break;
                case "mh":
                    units = "mH";
                    break;
                case "h":
                    units = "H";
                    break;
            }
            var result = $"u{val}{units}";
            return result;
        }

        private T? GetEnumByDescription<T>(string description)
        {
            var type = typeof(T).GetExtendedType();
            foreach (var val in type.EnumValues)
            {
                var memberInfos = type.Type.GetMember(val.Value);
                var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == type.Type);
                var valueAttributes = enumValueMemberInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (valueAttributes != null)
                {
                    var descriptionVal = ((DescriptionAttribute)valueAttributes[0]).Description;
                    if (descriptionVal.Equals(description))
                        return (T)val.Key;
                }
            }
            return default(T);
        }

    }

    public class DigikeyUnauthorizedException : Exception
    {
        public OAuthAuthorization Authorization { get; }
        public DigikeyUnauthorizedException(OAuthAuthorization authorization) : base("User must authorize")
        {
            Authorization = authorization;
        }
    }
}