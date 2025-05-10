using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Authentication;
using Binner.Model.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Binner.Common.Services.Authentication
{
    /// <summary>
    /// Jwt Utilities
    /// </summary>
    public class JwtService
    {
        private static readonly string _appSettingsFilename = EnvironmentVarConstants.GetEnvOrDefault(EnvironmentVarConstants.Config, AppConstants.AppSettings);

        private readonly WebHostServiceConfiguration _configuration;
        private readonly ISettingsService _settingsService;

        public JwtService(WebHostServiceConfiguration configuration, ISettingsService settingsService)
        {
            _configuration = configuration;
            _settingsService = settingsService;
        }

        public IEnumerable<Claim> GetClaims(UserContext user)
        {
            var claims = new List<Claim> {
                new (ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new (JwtClaimTypes.UserId, user.UserId.ToString()),
                new (JwtClaimTypes.OrganizationId, user.OrganizationId.ToString()),
                new (JwtClaimTypes.FullName, user.Name ?? string.Empty),
                new (ClaimTypes.Name, user.EmailAddress ?? string.Empty),
                new (ClaimTypes.HomePhone, string.IsNullOrEmpty(user.PhoneNumber) ? "" : user.PhoneNumber),
                new (JwtClaimTypes.CanLogin, user.CanLogin.ToString()),
                new (ClaimTypes.Expiration, DateTime.UtcNow.Add(_configuration.Authentication.JwtAccessTokenExpiryTime).ToString("MMM ddd dd yyyy HH:mm:ss tt"))
            };
            if (user.IsAdmin)
                claims.Add(new Claim(JwtClaimTypes.Admin, true.ToString()));
            return claims;
        }

        /// <summary>
        /// Generate a Jwt access token from a user context
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public string GenerateJwtToken(UserContext user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            // generate token that is valid for 15 minutes
            var tokenHandler = new JwtSecurityTokenHandler();
            var claims = GetClaims(user);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration.Authentication.JwtIssuer,
                Audience = _configuration.Authentication.JwtAudience,
                Subject = new ClaimsIdentity(claims, "Password"),
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.Add(_configuration.Authentication.JwtAccessTokenExpiryTime),
                NotBefore = DateTime.UtcNow,
                SigningCredentials = new SigningCredentials(GetOrCreateJwtSigningKey(), GetSecurityAlgorithm(_configuration.Authentication.EncryptionBits))
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generate a token that can be used to serve secure images
        /// </summary>
        /// <returns></returns>
        public string? GenerateImagesToken() => TokenGenerator.NewToken();

        /// <summary>
        /// Get the security key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static SecurityKey GetSecurityKey(string key)
            => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));

        /// <summary>
        /// Generate a new refresh token
        /// </summary>
        /// <returns></returns>
        public RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = GetUniqueToken(),
                // token expires after configured value
                Expires = DateTime.UtcNow.Add(_configuration.Authentication.JwtRefreshTokenExpiryTime),
                Created = DateTime.UtcNow
            };

            return refreshToken;

            string GetUniqueToken()
            {
                var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(_configuration.Authentication.TokenLength));
                return token;
            }
        }

        /// <summary>
        /// Get the security algorithm from encryption bit length
        /// </summary>
        /// <param name="encryptionBits"></param>
        /// <returns></returns>
        public string GetSecurityAlgorithm(EncryptionBits encryptionBits)
            => encryptionBits switch
            {
                EncryptionBits.Bits128 => SecurityAlgorithms.Aes128CbcHmacSha256,
                EncryptionBits.Bits256 => SecurityAlgorithms.HmacSha256Signature,
                EncryptionBits.Bits512 => SecurityAlgorithms.HmacSha512Signature,
                _ => SecurityAlgorithms.HmacSha256Signature,
            };

        /// <summary>
        /// Get the token validation parameters that indicate how a Jwt token should be validated
        /// </summary>
        /// <returns></returns>
        public TokenValidationParameters GetTokenValidationParameters()
        {
            return new TokenValidationParameters
            {
                IssuerSigningKey = GetOrCreateJwtSigningKey(),
                ValidateIssuerSigningKey = _configuration.Authentication.ValidateIssuerSigningKey,
                ValidateIssuer = _configuration.Authentication.ValidateIssuer,
                ValidIssuer = _configuration.Authentication.JwtIssuer,
                ValidateAudience = _configuration.Authentication.ValidateAudience,
                ValidAudience = _configuration.Authentication.JwtAudience,
                RequireExpirationTime = _configuration.Authentication.RequireExpirationTime,
                ValidateLifetime = _configuration.Authentication.ValidateLifetime,
                ClockSkew = _configuration.Authentication.ClockSkew,
                // LifetimeValidator = TokenLifetimeValidator.Validate
            };
        }

        private SecurityKey GetOrCreateJwtSigningKey()
        {
            var signingKey = _configuration.Authentication.JwtSecretKey;
            if (string.IsNullOrEmpty(signingKey))
            {
                signingKey = TokenGenerator.NewSecurityToken(40);
                _configuration.Authentication.JwtSecretKey = signingKey;
                // save to appsettings
                _settingsService.SaveSettingsAsAsync(_configuration, nameof(WebHostServiceConfiguration), _appSettingsFilename, true);
            }

            return GetSecurityKey(signingKey);
        }

        public static class TokenLifetimeValidator
        {
            public static bool Validate(DateTime? notBefore, DateTime? expires, SecurityToken tokenToValidate, TokenValidationParameters @param)
                => expires > DateTime.UtcNow;
        }
    }
}
