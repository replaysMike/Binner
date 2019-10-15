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
using ApiClient.OAuth2.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ApiClient.OAuth2
{
    /// <summary>
    /// Helper functions for OAuth2Service class and other classes calling OAuth2Service functions
    /// </summary>
    public static class OAuth2Helpers
    {
        /// <summary>
        ///     Determines whether response has a unauthorized error message.
        /// </summary>
        /// <param name="content">json response</param>
        /// <returns>
        ///     <c>true</c> if token is stale in the error response; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsTokenStale(string content)
        {
            var errors = JsonConvert.DeserializeObject<OAuth2Error>(content);
            return errors.HttpMessage.ToLower().Contains("unauthorized");
        }

        /// <summary>
        ///     Convert plain text to a base 64 encoded string - http://stackoverflow.com/a/11743162
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Refreshes the token asynchronous.
        /// </summary>
        /// <param name="clientSettings">ApiClientSettings needed for creating a proper refresh token HTTP post call.</param>
        /// <returns>Returns OAuth2AccessToken</returns>
        public static async Task<OAuth2AccessToken> RefreshTokenAsync(ApiClientSettings clientSettings)
        {
            var postUrl = DigiKeyUriConstants.TokenEndpoint;

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>(OAuth2Constants.GrantType, OAuth2Constants.GrantTypes.RefreshToken),
                new KeyValuePair<string, string>(OAuth2Constants.ClientId, clientSettings.ClientId),
                new KeyValuePair<string, string>(OAuth2Constants.ClientSecret, clientSettings.ClientSecret),
                new KeyValuePair<string, string>(OAuth2Constants.GrantTypes.RefreshToken, clientSettings.RefreshToken),
            });

            var httpClient = new HttpClient();

            var response = await httpClient.PostAsync(postUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            var oAuth2AccessTokenResponse = OAuth2Helpers.ParseOAuth2AccessTokenResponse(responseString);

            clientSettings.UpdateAndSave(oAuth2AccessTokenResponse);

            return oAuth2AccessTokenResponse;
        }

        /// <summary>
        ///     Parses the OAuth2 access token response.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>instance of OAuth2AccessToken</returns>
        /// <exception cref="ApiException">ull)</exception>
        public static OAuth2AccessToken ParseOAuth2AccessTokenResponse(string response)
        {
            try
            {
                var oAuth2AccessTokenResponse = JsonConvert.DeserializeObject<OAuth2AccessToken>(response);
                return oAuth2AccessTokenResponse;
            }
            catch (System.Exception e)
            {
                throw new ApiException($"Unable to parse OAuth2 access token response {e.Message}", null);
            }
        }
    }
}
