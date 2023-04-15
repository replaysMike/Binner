using Binner.Model.Configuration;
using System;

namespace Binner.Common.Configuration
{
    /// <summary>
    /// Authentication configuration
    /// </summary>
    public class AuthenticationConfiguration
    {
        /// <summary>
        /// The key used to generate Jwt tokens from
        /// </summary>
        public string JwtSecretKey { get; set; } = string.Empty;

        /// <summary>
        /// The jwt issuer url
        /// </summary>
        public string JwtIssuer { get; set; } = "https://localhost:8090";

        /// <summary>
        /// The jwt audience url
        /// </summary>
        public string JwtAudience { get; set; } = "https://localhost:8090";

        public bool ValidateIssuerSigningKey { get; set; } = true;
        public bool ValidateIssuer { get; set; } = true;
        public bool ValidateAudience { get; set; } = true;
        public bool RequireExpirationTime { get; set; } = true;
        public bool ValidateLifetime { get; set; } = true;

        /// <summary>
        /// Set the clock skew to apply for validating jwt
        /// </summary>
        public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Length in bytes of the generated token
        /// </summary>
        public int TokenLength { get; set; } = 64;

        /// <summary>
        /// Get the length of time an Jwt Access token (short-lived) is valid for. Default: 15 minutes
        /// </summary>
        public TimeSpan JwtAccessTokenExpiryTime { get; set; } = TimeSpan.FromMinutes(15);

        /// <summary>
        /// Get the length of time a Jwt Refresh token (long-lived) is valid for. Default: 3 days
        /// </summary>
        public TimeSpan JwtRefreshTokenExpiryTime { get; set; } = TimeSpan.FromDays(3);

        /// <summary>
        /// The bits strength to use for encryption
        /// </summary>
        public EncryptionBits EncryptionBits { get; set; } = EncryptionBits.Bits256;
    }
}
