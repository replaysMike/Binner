using ApiClient.OAuth2;
using Binner.Common.Integrations.Models.Digikey;
using Binner.Common.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Binner.Common.Integrations
{
    public class DigikeyApi
    {
        public const string Path = "https://sandbox-api.digikey.com/Search/v3/Products";
        private readonly OAuth2Service _oAuth2Service;
        private readonly ICredentialService _credentialService;
        private readonly HttpClient _client;
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            // ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            Converters = new List<JsonConverter> { new StringEnumConverter() }
        };

        public DigikeyApi(OAuth2Service oAuth2Service, ICredentialService credentialService)
        {
            _oAuth2Service = oAuth2Service;
            _credentialService = credentialService;
            _client = new HttpClient();
        }

        public async Task<DigikeyAuthorization> AuthorizeAsync()
        {
            // todo: validate if the token is still valid or needs to be refreshed
            var getAuth = ServerContext.Get<DigikeyAuthorization>(nameof(DigikeyAuthorization));
            if (getAuth != null && getAuth.IsAuthorized)
                return getAuth;

            var credential = await _credentialService.GetOAuthCredentialAsync(nameof(DigikeyApi));
            if (credential == null)
            {
                // request a token if we don't already have one
                var scopes = "";
                var authUrl = _oAuth2Service.GenerateAuthUrl(scopes);
                OpenBrowser(authUrl);
                var authRequest = new DigikeyAuthorization(_oAuth2Service.ClientSettings.ClientId);
                ServerContext.Set(nameof(DigikeyAuthorization), authRequest);

                // wait for authorization or timeout
                var startTime = DateTime.Now;
                while (!_manualResetEvent.WaitOne(100))
                {
                    getAuth = ServerContext.Get<DigikeyAuthorization>(nameof(DigikeyAuthorization));
                    if (getAuth.AuthorizationReceived)
                    {
                        // ok, it either failed or succeeded
                        if (getAuth.IsAuthorized)
                        {
                            // save the credential
                            await _credentialService.SaveOAuthCredentialAsync(new Common.Models.OAuthCredential
                            {
                                Provider = nameof(DigikeyApi),
                                AccessToken = getAuth.AccessToken,
                                RefreshToken = getAuth.RefreshToken,
                                DateCreatedUtc = getAuth.Created
                            });
                        }
                        return getAuth;
                    }
                    else
                    {
                        if (DateTime.Now.Subtract(startTime).TotalSeconds >= 10)
                        {
                            // timeout
                            return null;
                        }
                    }
                }
            } else
            {
                // reuse a saved oAuth credential
                return new DigikeyAuthorization(_oAuth2Service.ClientSettings.ClientId)
                {
                    AccessToken = credential.AccessToken,
                    RefreshToken = credential.RefreshToken,
                    Created = credential.DateCreatedUtc,
                    AuthorizationReceived = true,
                };
            }

            return null;
        }

        public async Task<ICollection<Product>> GetProductInformationAsync(string partNumber)
        {
            var authResponse = await AuthorizeAsync();
            if (authResponse == null || !authResponse.IsAuthorized) throw new UnauthorizedAccessException("Unable to authenticate with DigiKey");

            try
            {
                var includes = new List<string> { "DigiKeyPartNumber", "QuantityAvailable", "Manufacturer", "ManufacturerPartNumber", "PrimaryDatasheet", "ProductDescription", "DetailedDescription", "MinimumOrderQuantity", "NonStock", "UnitPrice", "ProductStatus", "ProductUrl", "Parameters" };
                var values = new Dictionary<string, string>
                {
                    { "Includes", $"Products({string.Join(",", includes)})" },
                };
                var uri = new Uri($"{Path}/Keyword?" + string.Join("&", values.Select(x => $"{x.Key}={x.Value}")));
                var k = uri.ToString();
                var req = new HttpRequestMessage(HttpMethod.Post, uri);
                req.Headers.Add("X-DIGIKEY-Client-Id", authResponse.ClientId);
                req.Headers.Add("Authorization", $"Bearer {authResponse.AccessToken}");
                req.Headers.Add("X-DIGIKEY-Locale-Site", "CA");
                req.Headers.Add("X-DIGIKEY-Locale-Language", "en");
                req.Headers.Add("X-DIGIKEY-Locale-Currency", "CAD");
                var request = new KeywordSearchRequest()
                {
                    Keywords = partNumber
                };
                var json = JsonConvert.SerializeObject(request, _serializerSettings);
                req.Content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.SendAsync(req);
                if (response.IsSuccessStatusCode)
                {
                    var resultString = response.Content.ReadAsStringAsync().Result;
                    var results = JsonConvert.DeserializeObject<KeywordSearchResponse>(resultString, _serializerSettings);
                    return results.Products;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new List<Product>();
        }

        public void OpenBrowser(string url)
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

    }
}
