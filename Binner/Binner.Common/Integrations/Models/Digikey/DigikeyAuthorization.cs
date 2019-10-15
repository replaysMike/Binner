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

        public bool AuthorizationReceived { get; set; }
        public string Error { get; set; }
        public string ErrorDescription { get; set; }
        public string ClientId { get; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Created { get; set; }
        public TimeSpan ExpiresIn { get; set; }

        public DigikeyAuthorization(string clientId)
        {
            ClientId = clientId;
        }
    }
}
