﻿using Binner.Common.Extensions;
using Binner.Common.Integrations.Models;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Integrations.DigiKey;
using Binner.Model.Integrations.DigiKey.V3;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TypeSupport.Extensions;
using static Binner.Common.Integrations.DigikeyApi;

namespace Binner.Common.Integrations
{
    public class BaseDigikeyApi : IBaseDigikeyApi
    {
        #region Regex Matching
        private readonly Regex PercentageRegex = new Regex("\\b(?<!\\.)(?!0+(?:\\.0+)?%)(?:\\d|[1-9]\\d|100)(?:(?<!100)\\.\\d+)?%", RegexOptions.Compiled);
        private readonly Regex PowerRegex = new Regex("^(\\d+[\\/\\d. ]*[Ww]$|\\d*[Ww]$)", RegexOptions.Compiled);
        private readonly Regex ResistanceRegex = new Regex("^(\\d+[\\d. ]*[KkMm]$|\\d*[KkMm]$|\\d*(?i)ohm(?-i)$)", RegexOptions.Compiled);
        private readonly Regex CapacitanceRegex = new Regex("^\\d+\\.?\\d*(uf|pf|mf|f)$", RegexOptions.Compiled);
        private readonly Regex VoltageRegex = new Regex("^\\d+\\.?\\d*(v|mv)$", RegexOptions.Compiled);
        private readonly Regex CurrentRegex = new Regex("^\\d+\\.?\\d*(a|ma)$", RegexOptions.Compiled);
        private readonly Regex InductanceRegex = new Regex("^\\d+\\.?\\d*(nh|uh|h)$", RegexOptions.Compiled);
        #endregion

        private readonly ILogger<DigikeyApi> _logger;
        private readonly DigikeyConfiguration _configuration;
        protected readonly IApiHttpClient _client;
        private readonly LocaleConfiguration _localeConfiguration;
        private readonly JsonSerializerSettings _serializerSettings;

        public BaseDigikeyApi(ILogger<DigikeyApi> logger, DigikeyConfiguration configuration, LocaleConfiguration localeConfiguration, JsonSerializerSettings serializerSettings, IApiHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _client = httpClientFactory.Create();
            _localeConfiguration = localeConfiguration;
            _serializerSettings = serializerSettings;
        }

        /// <summary>
        /// Handle known error conditions first, if response is OK false will be returned
        /// </summary>
        /// <param name="response"></param>
        /// <param name="authenticationResponse"></param>
        /// <returns></returns>
        /// <exception cref="DigikeyUnauthorizedException"></exception>
        protected async Task<(bool IsSuccessful, IApiResponse Response)> TryHandleResponseAsync(HttpResponseMessage response, OAuthAuthorization authenticationResponse)
        {
            // check for error status codes before we return a response
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // process a possible error message
                var resultString = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(resultString))
                {
                    var unauthorizedResponse = JsonConvert.DeserializeObject<UnauthorizedErrorResponse>(resultString.Trim());
                    if (unauthorizedResponse?.ErrorDetails?.Contains("not subscribed", StringComparison.InvariantCultureIgnoreCase) == true)
                    {
                        // access denied to endpoint. Either wrong api version or not subscribed to the endpoint
                        _logger.LogInformation($"[{nameof(TryHandleResponseAsync)}] Received 401 Unauthorized - Api Key used is not valid for this endpoint. Version: {unauthorizedResponse.ErrorResponseVersion} Response: {unauthorizedResponse.ErrorDetails}");
                        throw new DigikeyUnsubscribedException(authenticationResponse, unauthorizedResponse.ErrorResponseVersion, unauthorizedResponse.ErrorDetails);
                    }
                    // auth token invalid or expired
                    var tokenErrorResponse = JsonConvert.DeserializeObject<UnauthorizedTokenResponse>(resultString.Trim());
                    if (tokenErrorResponse?.Detail?.Contains("bearer token is expired", StringComparison.InvariantCultureIgnoreCase) == true)
                    {
                        // token expired, refresh or reauthenticate
                        _logger.LogInformation($"[{nameof(TryHandleResponseAsync)}] Received 401 Unauthorized - Bearer token is expired. accesstoken='{authenticationResponse.AccessToken}' Response: {tokenErrorResponse.Detail}");
                        throw new DigikeyUnauthorizedException(authenticationResponse);
                    }
                }

                // unauthorized token
                _logger.LogInformation($"[{nameof(TryHandleResponseAsync)}] Received 401 Unauthorized - user must authenticate. accesstoken='{authenticationResponse.AccessToken}'");
                throw new DigikeyUnauthorizedException(authenticationResponse);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                if (response.Headers.Contains("X-RateLimit-Limit"))
                {
                    // throttled
                    var remainingTime = TimeSpan.Zero;
                    if (response.Headers.Contains("X-RateLimit-Remaining"))
                        remainingTime = TimeSpan.FromSeconds(int.Parse(response.Headers.GetValues("X-RateLimit-Remaining").First()));
                    _logger.LogWarning($"[{nameof(TryHandleResponseAsync)}] Request is rate limited. Try again in {remainingTime}");
                    var throttledResponse = ApiResponse.Create($"{nameof(DigikeyApi)} request throttled. Try again in {remainingTime}", nameof(DigikeyApi));
                    return (false, throttledResponse);
                }

                // return generic error
                return (false, ApiResponse.Create($"Api returned error status code {response.StatusCode}: {response.ReasonPhrase}", nameof(DigikeyApi)));
            }
            else if (response.IsSuccessStatusCode)
            {
                // allow processing of response
                return (true, ApiResponse.Create(nameof(DigikeyApi)));
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                _logger.LogError($"[{nameof(TryHandleResponseAsync)}] Bad request. Api returned error status code {response.StatusCode}: {response.ReasonPhrase}. accesstoken='{authenticationResponse.AccessToken}'");
                var badRequestResponse = ApiResponse.Create($"Api returned error status code {response.StatusCode}: {response.ReasonPhrase}", nameof(DigikeyApi));
                var resultString = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(resultString) && resultString != "{}")
                    badRequestResponse.Errors.Add(resultString);
                return (false, badRequestResponse);
            }

            // return generic error
            var errResponse = ApiResponse.Create($"Api returned error status code {response.StatusCode}: {response.ReasonPhrase}", nameof(DigikeyApi));
            return (false, errResponse);
        }

        protected HttpRequestMessage CreateRequest(OAuthAuthorization authResponse, HttpMethod method, Uri uri)
        {
            var message = new HttpRequestMessage(method, uri);
            message.Headers.Add("X-DIGIKEY-Client-Id", authResponse.ClientId);
            message.Headers.Add("Authorization", $"Bearer {authResponse.AccessToken}");
            message.Headers.Add("X-DIGIKEY-Locale-Site", _configuration.Site.ToString());
            message.Headers.Add("X-DIGIKEY-Locale-Language", _localeConfiguration.Language.ToString().ToLower());
            message.Headers.Add("X-DIGIKEY-Locale-Currency", _localeConfiguration.Currency.ToString().ToUpper());
            return message;
        }

        /// <summary>
        /// Detect certain keywords/regex combinations to move those keywords into a parametric filter, to get a more defined result from DigiKey
        /// </summary>
        /// <param name="keywordCollection"></param>
        /// <param name="packageType"></param>
        /// <param name="taxonomies"></param>
        /// <returns></returns>
        protected (ICollection<ParametricFilter> ParametricFilters, ICollection<string> FilteredKeywords) MapParametricFilters(ICollection<string> keywordCollection, MountingTypes packageType, ICollection<Taxonomies> taxonomies)
        {
            // create a copy that we will modify and return for parametric search behavior
            var filteredKeywords = new List<string>(keywordCollection);
            var filters = new List<ParametricFilter>();
            var percent = "";
            var power = "";
            var resistance = "";
            var capacitance = "";
            var voltageRating = "";
            var currentRating = "";
            var inductance = "";
            foreach (var keyword in filteredKeywords)
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
            if (filteredKeywords.Contains("precision") || !string.IsNullOrEmpty(percent))
            {
                if (filteredKeywords.Contains("precision"))
                    filteredKeywords.Remove("precision");
                if (filteredKeywords.Contains(percent))
                    filteredKeywords.Remove(percent);
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
                filteredKeywords.Remove(power);
                var filter = new ParametricFilter
                {
                    ParameterId = (int)Parametrics.Power,
                    ValueId = GetPower(power)
                };
                filters.Add(filter);
            }
            if (!string.IsNullOrEmpty(resistance))
            {
                filteredKeywords.Remove(resistance);
                var filter = new ParametricFilter
                {
                    ParameterId = (int)Parametrics.Resistance,
                    ValueId = GetResistance(resistance)
                };
                filters.Add(filter);
            }
            if (!string.IsNullOrEmpty(capacitance))
            {
                filteredKeywords.Remove(capacitance);
                var filter = new ParametricFilter
                {
                    ParameterId = (int)Parametrics.Capacitance,
                    ValueId = GetCapacitance(capacitance)
                };
                filters.Add(filter);
            }
            if (!string.IsNullOrEmpty(voltageRating))
            {
                filteredKeywords.Remove(voltageRating);
                var filter = new ParametricFilter
                {
                    ParameterId = (int)Parametrics.VoltageRating,
                    ValueId = GetVoltageRating(voltageRating)
                };
                filters.Add(filter);
            }
            if (!string.IsNullOrEmpty(currentRating))
            {
                filteredKeywords.Remove(currentRating);
                var filter = new ParametricFilter
                {
                    ParameterId = (int)Parametrics.CurrentRating,
                    ValueId = GetVoltageRating(currentRating)
                };
                filters.Add(filter);
            }
            if (!string.IsNullOrEmpty(inductance))
            {
                filteredKeywords.Remove(inductance);
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
            return (filters, filteredKeywords);
        }

        /// <summary>
        /// Maps a part type and package type to a DigiKey category (taxonomy)
        /// </summary>
        /// <param name="partType"></param>
        /// <param name="packageType"></param>
        /// <returns></returns>
        protected ICollection<Taxonomies> MapTaxonomies(string partType, MountingTypes packageType)
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
            var result = $"{val} {units}";
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
            var result = $"{val}{units}";
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

        internal bool VersionEquals(Version version, DigiKeyApiVersion enumVersion)
        {
            if (version.Major == 4 && enumVersion == DigiKeyApiVersion.V4) return true;
            if (version.Major == 3 && enumVersion == DigiKeyApiVersion.V3) return true;
            return false;
        }

        public virtual void Dispose()
        {
            _client.Dispose();
        }
    }
}
