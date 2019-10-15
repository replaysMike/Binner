using System;

namespace Binner.Common.Models
{
    public class OAuthCredential
    {
        /// <summary>
        /// The provider credential
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// The access token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// The refresh token
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// The date the token was created
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }
    }
}
