using Binner.Common.Auth;
using Binner.Common.IO;
using Binner.Data;
using Binner.Data.Model;
using Binner.Global.Common;
using Binner.Model.Authentication;
using Binner.Model.Configuration;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Binner.Common.Services.Authentication
{
    /// <summary>
    /// Jwt Utilities
    /// </summary>
    public class JwtService
    {
        private readonly AuthenticationConfiguration _configuration;

        public JwtService(AuthenticationConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IEnumerable<Claim> GetClaims(UserContext user)
        {
            var claims = new List<Claim> {
                new (ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new (JwtClaimTypes.UserId, user.UserId.ToString()),
                new (JwtClaimTypes.OrganizationId, user.OrganizationId.ToString()),
                new (JwtClaimTypes.FullName, user.Name),
                new (ClaimTypes.Name, user.EmailAddress),
                new (ClaimTypes.HomePhone, user.PhoneNumber),
                new (JwtClaimTypes.CanLogin, user.CanLogin.ToString()),
                new (ClaimTypes.Expiration, DateTime.UtcNow.Add(_configuration.JwtAccessTokenExpiryTime).ToString("MMM ddd dd yyyy HH:mm:ss tt"))
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
            var signingKey = GetJwtSigningKey();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration.JwtIssuer,
                Audience = _configuration.JwtAudience,
                Subject = new ClaimsIdentity(claims, "Password"),
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.Add(_configuration.JwtAccessTokenExpiryTime),
                NotBefore = DateTime.UtcNow,
                SigningCredentials = new SigningCredentials(signingKey, GetSecurityAlgorithm(_configuration.EncryptionBits))
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generate a token that can be used to serve secure images
        /// </summary>
        /// <returns></returns>
        public string? GenerateImagesToken() => ConfirmationTokenGenerator.NewToken();

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
                Expires = DateTime.UtcNow.Add(_configuration.JwtRefreshTokenExpiryTime),
                Created = DateTime.UtcNow
            };

            return refreshToken;

            string GetUniqueToken()
            {
                var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(_configuration.TokenLength));
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
        /// <param name="authConfig"></param>
        /// <returns></returns>
        public TokenValidationParameters GetTokenValidationParameters()
        {
            var signingKey = GetJwtSigningKey();
            return new TokenValidationParameters
            {
                IssuerSigningKey = signingKey,
                ValidateIssuerSigningKey = _configuration.ValidateIssuerSigningKey,
                ValidateIssuer = _configuration.ValidateIssuer,
                ValidIssuer = _configuration.JwtIssuer,
                ValidateAudience = _configuration.ValidateAudience,
                ValidAudience = _configuration.JwtAudience,
                RequireExpirationTime = _configuration.RequireExpirationTime,
                ValidateLifetime = _configuration.ValidateLifetime,
                ClockSkew = _configuration.ClockSkew,
                // LifetimeValidator = TokenLifetimeValidator.Validate
            };
        }

        private static SecurityKey GetJwtSigningKey()
        {
            var keyProvider = new SecurityKeyProvider();
            var key = string.Empty;
            try
            {
                // use the machine's hardware id to sign jwt
                key = HardwareId.Get();
            }
            catch (Exception)
            {
                // use fallback
            }

            if (string.IsNullOrWhiteSpace(key))
                key = keyProvider.LoadOrGenerateKey(SecurityKeyProvider.KeyTypes.Jwt, 40);
            return GetSecurityKey(key);
        }
            

        public static class TokenLifetimeValidator
        {
            public static bool Validate(DateTime? notBefore, DateTime? expires, SecurityToken tokenToValidate, TokenValidationParameters @param)
                => expires > DateTime.UtcNow;
        }
    }
}
