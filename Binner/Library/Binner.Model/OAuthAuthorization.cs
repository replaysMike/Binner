using System.Net.Sockets;

namespace Binner.Model
{
    public class OAuthAuthorization
    {
        /// <summary>
        /// True if authorization was successful
        /// </summary>
        public bool IsAuthorized => AuthorizationReceived
            && !MustAuthorize
            && string.IsNullOrEmpty(Error)
            && string.IsNullOrEmpty(ErrorDescription)
            && !string.IsNullOrEmpty(AccessToken)
            && !string.IsNullOrEmpty(RefreshToken);

        /// <summary>
        /// Unique Id for the auth request
        /// </summary>
        public Guid Id { get; private set; } = Guid.NewGuid();

        /// <summary>
        /// Name of the provider used
        /// </summary>
        public string Provider { get; set; } = string.Empty;

        /// <summary>
        /// The UserId associated with this authorization
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// True if an oAuth callback was received from Digikey
        /// </summary>
        public bool AuthorizationReceived { get; set; }

        /// <summary>
        /// Authorization code
        /// </summary>
        public string? AuthorizationCode { get; set; }

        /// <summary>
        /// Error definition
        /// </summary>
        public string Error { get; set; } = string.Empty;

        /// <summary>
        /// Error description
        /// </summary>
        public string ErrorDescription { get; set; } = string.Empty;

        /// <summary>
        /// True if the user must authorize
        /// </summary>
        public bool MustAuthorize { get; set; }

        /// <summary>
        /// If the user must authorize, the Authorization Url they need to visit
        /// </summary>
        public string AuthorizationUrl { get; set; } = string.Empty;

        /// <summary>
        /// Digikey ClientId
        /// </summary>
        public string ClientId { get; } = string.Empty;

        /// <summary>
        /// oAuth Access Token
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// oAuth Refresh Token
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Date the AccessToken was created
        /// </summary>
        public DateTime CreatedUtc { get; set; }

        /// <summary>
        /// Time in which the current token expires
        /// </summary>
        public DateTime ExpiresUtc { get; set; }

        /// <summary>
        /// The full url to return the user to
        /// </summary>
        public string ReturnToUrl { get; set; } = string.Empty;

        public OAuthAuthorization()
        {
        }

        public OAuthAuthorization(string provider, Guid id)
        {
            Provider = provider;
            Id = id;
        }

        public OAuthAuthorization(string provider, bool mustAuthorize, string authorizationUrl)
        {
            Provider = provider;
            MustAuthorize = mustAuthorize;
            AuthorizationUrl = authorizationUrl;
        }

        public OAuthAuthorization(string provider, string clientId, string returnToUrl)
        {
            Provider = provider;
            ClientId = clientId;
            ReturnToUrl = returnToUrl;
        }
    }
}
