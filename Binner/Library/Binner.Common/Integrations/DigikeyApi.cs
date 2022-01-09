using ApiClient.OAuth2;
using Binner.Common.Extensions;
using Binner.Common.Integrations.Models;
using Binner.Common.Integrations.Models.Digikey;
using Binner.Common.Services;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TypeSupport.Extensions;

namespace Binner.Common.Integrations
{
    public class DigikeyApi : IIntegrationApi
    {
        public static readonly TimeSpan MaxAuthorizationWaitTime = TimeSpan.FromSeconds(30);

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
        private readonly string _apiUrl;
        private readonly OAuth2Service _oAuth2Service;
        private readonly ICredentialService _credentialService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _client;
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            // ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            Converters = new List<JsonConverter> { new StringEnumConverter() }
        };

        public bool IsConfigured => !string.IsNullOrEmpty(_oAuth2Service.ClientSettings.ClientId) 
            && !string.IsNullOrEmpty(_oAuth2Service.ClientSettings.ClientSecret) && !string.IsNullOrEmpty(_apiUrl);

        public DigikeyApi(OAuth2Service oAuth2Service, string apiUrl, ICredentialService credentialService, IHttpContextAccessor httpContextAccessor)
        {
            _oAuth2Service = oAuth2Service;
            _apiUrl = apiUrl;
            _credentialService = credentialService;
            _httpContextAccessor = httpContextAccessor;
            _client = new HttpClient();
        }

        public enum MountingTypes
        {
            None = 0,
            SurfaceMount = 3,
            ThroughHole = 80
        }

        public async Task<IApiResponse> GetOrderAsync(string orderId)
        {
            var authResponse = await AuthorizeAsync();
            if (authResponse == null || !authResponse.IsAuthorized)
                return ApiResponse.Create(true, authResponse.AuthorizationUrl, $"User must authorize", nameof(DigikeyApi));
            return await WrapApiRequestAsync(authResponse, async (authenticationResponse) =>
            {
                try
                {
                    // set what fields we want from the API
                    var uri = Url.Combine(_apiUrl, "OrderDetails/v3/Status/", orderId);
                    var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Get, uri);
                    // perform a keywords API search
                    var response = await _client.SendAsync(requestMessage);
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        throw new DigikeyUnauthorizedException(authenticationResponse);
                    if (response.IsSuccessStatusCode)
                    {
                        var resultString = response.Content.ReadAsStringAsync().Result;
                        var results = JsonConvert.DeserializeObject<OrderSearchResponse>(resultString, _serializerSettings);
                        return new ApiResponse(results, nameof(DigikeyApi));
                    }
                    return ApiResponse.Create($"Api returned error status code {response.StatusCode}: {response.ReasonPhrase}", nameof(DigikeyApi));
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }

        /// <summary>
        /// Get information about a DigiKey product
        /// </summary>
        /// <param name="digikeyPartNumber"></param>
        /// <returns></returns>
        public async Task<IApiResponse> GetProductDetailsAsync(string digikeyPartNumber)
        {
            var authResponse = await AuthorizeAsync();
            if (authResponse == null || !authResponse.IsAuthorized)
                return ApiResponse.Create(true, authResponse.AuthorizationUrl, $"User must authorize", nameof(DigikeyApi));
            return await WrapApiRequestAsync(authResponse, async (authenticationResponse) =>
            {
                try
                {
                    // set what fields we want from the API
                    var uri = Url.Combine(_apiUrl, "Search/v3/Products/", digikeyPartNumber);
                    var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Get, uri);
                    // perform a keywords API search
                    var response = await _client.SendAsync(requestMessage);
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        throw new DigikeyUnauthorizedException(authenticationResponse);
                    if (response.IsSuccessStatusCode)
                    {
                        var resultString = response.Content.ReadAsStringAsync().Result;
                        var results = JsonConvert.DeserializeObject<Product>(resultString, _serializerSettings);
                        return new ApiResponse(results, nameof(DigikeyApi));
                    }
                    return ApiResponse.Create($"Api returned error status code {response.StatusCode}: {response.ReasonPhrase}", nameof(DigikeyApi));
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
        /// <returns></returns>
        public async Task<IApiResponse> GetBarcodeDetailsAsync(string barcode)
        {
            var authResponse = await AuthorizeAsync();
            if (authResponse == null || !authResponse.IsAuthorized)
                return ApiResponse.Create(true, authResponse.AuthorizationUrl, $"User must authorize", nameof(DigikeyApi));
            return await WrapApiRequestAsync(authResponse, async (authenticationResponse) =>
            {
                try
                {
                    // set what fields we want from the API
                    var uri = Url.Combine(_apiUrl, "Barcoding/v3/ProductBarcodes/", barcode);
                    var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Get, uri);
                    // perform a keywords API search
                    var response = await _client.SendAsync(requestMessage);
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        throw new DigikeyUnauthorizedException(authenticationResponse);
                    if (response.IsSuccessStatusCode)
                    {
                        var resultString = response.Content.ReadAsStringAsync().Result;
                        var results = JsonConvert.DeserializeObject<ProductBarcodeResponse>(resultString, _serializerSettings);
                        return new ApiResponse(results, nameof(DigikeyApi));
                    }
                    return ApiResponse.Create($"Api returned error status code {response.StatusCode}: {response.ReasonPhrase}", nameof(DigikeyApi));
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }

        public async Task<IApiResponse> SearchAsync(string partNumber, string partType = "", string mountingType = "")
        {
            var authResponse = await AuthorizeAsync();
            if (authResponse == null || !authResponse.IsAuthorized)
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

            return await WrapApiRequestAsync(authResponse, async(authenticationResponse) =>
            {
                try
                {
                    // set what fields we want from the API
                    var includes = new List<string> { "DigiKeyPartNumber", "QuantityAvailable", "Manufacturer", "ManufacturerPartNumber", "PrimaryDatasheet", "ProductDescription", "DetailedDescription", "MinimumOrderQuantity", "NonStock", "UnitPrice", "ProductStatus", "ProductUrl", "Parameters" };
                    var values = new Dictionary<string, string>
                    {
                        { "Includes", $"Products({string.Join(",", includes)})" },
                    };
                    var uri = Url.Combine(_apiUrl, "/Search/v3/Products", $"/Keyword?" + string.Join("&", values.Select(x => $"{x.Key}={x.Value}")));
                    var requestMessage = CreateRequest(authenticationResponse, HttpMethod.Post, uri);
                    var taxonomies = MapTaxonomies(partType, packageTypeEnum);
                    var parametricFilters = MapParametricFilters(keywords, packageTypeEnum, taxonomies);
                    var request = new KeywordSearchRequest
                    {
                        Keywords = string.Join(" ", keywords),
                        Filters = new Filters
                        {
                            TaxonomyIds = taxonomies.Select(x => (int)x).ToList(),
                            ParametricFilters = parametricFilters
                        }
                    };
                    var json = JsonConvert.SerializeObject(request, _serializerSettings);
                    requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    // perform a keywords API search
                    var response = await _client.SendAsync(requestMessage);
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        throw new DigikeyUnauthorizedException(authenticationResponse);
                    if (response.IsSuccessStatusCode)
                    {
                        var resultString = response.Content.ReadAsStringAsync().Result;
                        var results = JsonConvert.DeserializeObject<KeywordSearchResponse>(resultString, _serializerSettings);
                        return new ApiResponse(results, nameof(DigikeyApi));
                    }
                    return ApiResponse.Create($"Api returned error status code {response.StatusCode}: {response.ReasonPhrase}", nameof(DigikeyApi));
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
            if (!string.IsNullOrEmpty(partType))
            {
                if (Enum.TryParse<Taxonomies>(partType, true, out taxonomy))
                {
                    var addBaseType = true;
                    // also map all the alternates
                    var memberInfos = typeof(Taxonomies).GetMember(taxonomy.ToString());
                    var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == typeof(Taxonomies));
                    var valueAttributes = enumValueMemberInfo.GetCustomAttributes(typeof(AlternatesAttribute), false);
                    var alternateIds = ((AlternatesAttribute)valueAttributes[0]).Ids;
                    // taxonomies.AddRange(alternateIds);
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
        /// Wraps an API request - if the request is unauthorized it will refresh the Auth token and re-issue the request
        /// </summary>
        /// <typeparam name="T"></typeparam>
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
                _oAuth2Service.ClientSettings.RefreshToken = ex.Authorization.RefreshToken;
                var token = await _oAuth2Service.RefreshTokenAsync();
                var refreshTokenResponse = new OAuthAuthorization(nameof(DigikeyApi), _oAuth2Service.ClientSettings.ClientId, _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString())
                {
                    AccessToken = token.AccessToken,
                    RefreshToken = token.RefreshToken,
                    CreatedUtc = DateTime.UtcNow,
                    ExpiresUtc = DateTime.UtcNow.Add(TimeSpan.FromSeconds(token.ExpiresIn)),
                    AuthorizationReceived = true,
                };
                var contextKey = $"{nameof(DigikeyApi)}-{_httpContextAccessor.HttpContext.User.Identity.Name}";
                ServerContext.Set(contextKey, refreshTokenResponse);
                if (refreshTokenResponse.IsAuthorized)
                {
                    // save the credential
                    await _credentialService.SaveOAuthCredentialAsync(new Common.Models.OAuthCredential
                    {
                        Provider = contextKey,
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
                throw new UnauthorizedAccessException("Unable to authenticate with Digikey!");
            }
        }

        private async Task ForgetAuthenticationTokens()
        {
            ServerContext.Remove<OAuthAuthorization>(nameof(OAuthAuthorization));
            await _credentialService.RemoveOAuthCredentialAsync(nameof(DigikeyApi));
        }

        private async Task<OAuthAuthorization> AuthorizeAsync()
        {
            var contextKey = $"{nameof(DigikeyApi)}-{_httpContextAccessor.HttpContext.User.Identity.Name}";
            // check if we have an in-memory auth credential
            var getAuth = ServerContext.Get<OAuthAuthorization>(contextKey);
            if (getAuth != null && getAuth.IsAuthorized)
                return getAuth;

            // check if we have a saved to disk auth credential
            OAuthAuthorization authRequest;
            var credential = await _credentialService.GetOAuthCredentialAsync(contextKey);
            if (credential != null)
            {
                // reuse a saved oAuth credential
                authRequest = new OAuthAuthorization(nameof(DigikeyApi), _oAuth2Service.ClientSettings.ClientId, _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString())
                {
                    AccessToken = credential.AccessToken,
                    RefreshToken = credential.RefreshToken,
                    CreatedUtc = credential.DateCreatedUtc,
                    ExpiresUtc = credential.DateExpiresUtc,
                    AuthorizationReceived = true,
                };
                // also store it in memory
                ServerContext.Set(contextKey, authRequest);
                return authRequest;

            }

            // user must authorize
            // request a token if we don't already have one
            var scopes = "";
            var authUrl = _oAuth2Service.GenerateAuthUrl(scopes);
            // OpenBrowser(authUrl);
            authRequest = new OAuthAuthorization(nameof(DigikeyApi), _oAuth2Service.ClientSettings.ClientId, _httpContextAccessor.HttpContext.Request.Headers["Referer"].ToString());
            ServerContext.Set(contextKey, authRequest);

            return new OAuthAuthorization(nameof(DigikeyApi), true, authUrl);
        }

        private HttpRequestMessage CreateRequest(OAuthAuthorization authResponse, HttpMethod method, Uri uri)
        {
            var message = new HttpRequestMessage(method, uri);
            message.Headers.Add("X-DIGIKEY-Client-Id", authResponse.ClientId);
            message.Headers.Add("Authorization", $"Bearer {authResponse.AccessToken}");
            message.Headers.Add("X-DIGIKEY-Locale-Site", "CA");
            message.Headers.Add("X-DIGIKEY-Locale-Language", "en");
            message.Headers.Add("X-DIGIKEY-Locale-Currency", "CAD");
            return message;
        }

        private void OpenBrowser(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url.Replace("&", "^&")}")); // Works ok on windows and escape need for cmd.exe
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);  // Works ok on linux
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url); // Not tested
            }
            else
                throw new InvalidOperationException("Failed to launch default web browser - I don't know how to do this on your platform!");
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

        private T GetEnumByDescription<T>(string description)
        {
            var type = typeof(T).GetExtendedType();
            foreach(var val in type.EnumValues)
            {
                var memberInfos = type.Type.GetMember(val.Value);
                var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == type.Type);
                var valueAttributes = enumValueMemberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                var descriptionVal = ((DescriptionAttribute)valueAttributes[0]).Description;
                if (descriptionVal.Equals(description))
                    return (T)val.Key;

            }
            return default(T);
        }

    }

    public class DigikeyUnauthorizedException : Exception
    {
        public OAuthAuthorization Authorization { get; }
        public DigikeyUnauthorizedException(OAuthAuthorization authorization)
        {
            Authorization = authorization;
        }
    }
}
