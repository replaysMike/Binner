//-----------------------------------------------------------------------
//
// THE SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES OF ANY KIND, EXPRESS, IMPLIED, STATUTORY, 
// OR OTHERWISE. EXPECT TO THE EXTENT PROHIBITED BY APPLICABLE LAW, DIGI-KEY DISCLAIMS ALL WARRANTIES, 
// INCLUDING, WITHOUT LIMITATION, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, 
// SATISFACTORY QUALITY, TITLE, NON-INFRINGEMENT, QUIET ENJOYMENT, 
// AND WARRANTIES ARISING OUT OF ANY COURSE OF DEALING OR USAGE OF TRADE. 
// 
// DIGI-KEY DOES NOT WARRANT THAT THE SOFTWARE WILL FUNCTION AS DESCRIBED, 
// WILL BE UNINTERRUPTED OR ERROR-FREE, OR FREE OF HARMFUL COMPONENTS.
// 
//-----------------------------------------------------------------------

using ApiClient.Constants;
using ApiClient.Exception;
using ApiClient.Models;
using ApiClient.OAuth2;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ApiClient
{
    public class ApiClientService
    {
        private const string CustomHeader = "Api-StaleTokenRetry";

        private ApiClientSettings _clientSettings;

        public ApiClientSettings ClientSettings
        {
            get => _clientSettings;
            set => _clientSettings = value;
        }

        /// <summary>
        ///     The httpclient which will be used for the api calls through the this instance
        /// </summary>
        public HttpClient HttpClient { get; private set; }

        public ApiClientService(ApiClientSettings clientSettings)
        {
            ClientSettings = clientSettings ?? throw new ArgumentNullException(nameof(clientSettings));
            Initialize();
        }

        private void Initialize()
        {
            HttpClient = new HttpClient();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var authenticationHeaderValue = new AuthenticationHeaderValue("Bearer", ClientSettings.AccessToken);
            HttpClient.DefaultRequestHeaders.Authorization = authenticationHeaderValue;

            HttpClient.DefaultRequestHeaders.Add("X-Digikey-Client-Id", ClientSettings.ClientId);
            HttpClient.BaseAddress = DigiKeyUriConstants.BaseAddress;
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task ResetExpiredAccessTokenIfNeeded()
        {
            if (_clientSettings.ExpirationDateTime < DateTime.Now)
            {
                // Let's refresh the token
                var oAuth2Service = new OAuth2Service(_clientSettings);
                var oAuth2AccessToken = await oAuth2Service.RefreshTokenAsync();
                if (oAuth2AccessToken.IsError)
                {
                    // Current Refresh token is invalid or expired 
                    return;
                }

                // Update the clientSettings
                _clientSettings.UpdateAndSave(oAuth2AccessToken);

                // Reset the Authorization header value with the new access token.
                var authenticationHeaderValue = new AuthenticationHeaderValue("Bearer", _clientSettings.AccessToken);
                HttpClient.DefaultRequestHeaders.Authorization = authenticationHeaderValue;
            }
        }

        public async Task<string> KeywordSearch(string keyword)
        {
            var resourcePath = "/Search/v3/Products/Keyword";

            var request = new KeywordSearchRequest
            {
                Keywords = keyword ?? "P5555-ND",
                RecordCount = 25
            };

            await ResetExpiredAccessTokenIfNeeded();
            var postResponse = await PostAsJsonAsync(resourcePath, request);

            return GetServiceResponse(postResponse).Result;
        }

        public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string resourcePath, T postRequest)
        {
            try
            {
                var httpContent = new StringContent(JsonConvert.SerializeObject(postRequest), Encoding.UTF8, "application/json");
                var response = await HttpClient.PostAsync(resourcePath, httpContent);

                //Unauthorized, then there is a chance token is stale
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (OAuth2Helpers.IsTokenStale(responseBody))
                    {
                        await OAuth2Helpers.RefreshTokenAsync(_clientSettings);

                        //Only retry the first time.
                        if (!response.RequestMessage.Headers.Contains(CustomHeader))
                        {
                            HttpClient.DefaultRequestHeaders.Add(CustomHeader, CustomHeader);
                            HttpClient.DefaultRequestHeaders.Authorization =
                                new AuthenticationHeaderValue("Authorization", _clientSettings.AccessToken);
                            return await PostAsJsonAsync(resourcePath, postRequest);
                        }
                        else if (response.RequestMessage.Headers.Contains(CustomHeader))
                        {
                            throw new ApiException($"Inside method {nameof(PostAsJsonAsync)} we received an unexpected stale token response - during the retry for a call whose token we just refreshed {response.StatusCode}");
                        }
                    }
                }

                return response;
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (ApiException)
            {
                throw;
            }
        }

        protected async Task<string> GetServiceResponse(HttpResponseMessage response)
        {
            var postResponse = string.Empty;

            if (response.IsSuccessStatusCode)
            {
                if (response.Content != null)
                {
                    postResponse = await response.Content.ReadAsStringAsync();
                }
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                var resp = new HttpResponseMessage(response.StatusCode)
                {
                    Content = response.Content,
                    ReasonPhrase = response.ReasonPhrase
                };
                var ex = new System.Exception($"Invalid response received: {await resp.Content.ReadAsStreamAsync()}");
                throw ex;
            }
            return postResponse;
        }
    }
}
