using System;

namespace Binner.Common.Integrations.Models.Digikey
{
    public class DigikeyAuthorization
    {
        /// <summary>
        /// True if authorization was successful
        /// </summary>
        public bool IsAuthorized => AuthorizationReceived && string.IsNullOrEmpty(Error) && string.IsNullOrEmpty(ErrorDescription)
            && !string.IsNullOrEmpty(AccessToken) && !string.IsNullOrEmpty(RefreshToken);

        /// <summary>
        /// Unique Id for the auth request
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// True if an oAuth callback was received from Digikey
        /// </summary>
        public bool AuthorizationReceived { get; set; }

        /// <summary>
        /// Error definition
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Error description
        /// </summary>
        public string ErrorDescription { get; set; }

        /// <summary>
        /// True if the user must authorize
        /// </summary>
        public bool MustAuthorize { get; set; }

        /// <summary>
        /// If the user must authorize, the Authorization Url they need to visit
        /// </summary>
        public string AuthorizationUrl { get; set; }
        
        /// <summary>
        /// Digikey ClientId
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// oAuth Access Token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// oAuth Refresh Token
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Date the AccessToken was created
        /// </summary>
        public DateTime CreatedUtc { get; set; }

        /// <summary>
        /// Time in which the current token expires
        /// </summary>
        public DateTime ExpiresUtc { get; set; }

        public DigikeyAuthorization(bool mustAuthorize, string authorizationUrl)
        {
            MustAuthorize = mustAuthorize;
            AuthorizationUrl = authorizationUrl;
        }

        public DigikeyAuthorization(string clientId)
        {
            ClientId = clientId;
        }
    }
}
