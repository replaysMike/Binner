using ApiClient.OAuth2.Models;
using System;

namespace Binner.Common.Integrations
{
    public class AccessTokens
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime ExpirationDateTime { get; set; }

        public void UpdateAndSave(OAuth2AccessToken oAuth2AccessToken)
        {
            AccessToken = oAuth2AccessToken.AccessToken;
            RefreshToken = oAuth2AccessToken.RefreshToken;
            ExpirationDateTime = DateTime.Now.AddSeconds(oAuth2AccessToken.ExpiresIn);
        }
    }
}
