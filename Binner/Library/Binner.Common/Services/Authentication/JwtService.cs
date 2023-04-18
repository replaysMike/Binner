using Binner.Common.IO;
using Binner.Common.Models;
using Binner.Data;
using Binner.Data.Model;
using Binner.Model.Authentication;
using Binner.Model.Configuration;
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
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(JwtClaimTypes.UserId, user.UserId.ToString()),
                new Claim(JwtClaimTypes.OrganizationId, user.OrganizationId.ToString()),
                new Claim(JwtClaimTypes.FullName, user.Name),
                new Claim(ClaimTypes.Name, user.EmailAddress),
                new Claim(ClaimTypes.HomePhone, user.PhoneNumber),
                new Claim(JwtClaimTypes.CanLogin, user.CanLogin.ToString()),
                new Claim(ClaimTypes.Expiration, DateTime.UtcNow.Add(_configuration.JwtAccessTokenExpiryTime).ToString("MMM ddd dd yyyy HH:mm:ss tt"))
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
                Issuer = _configuration.JwtIssuer,
                Audience = _configuration.JwtAudience,
                Subject = new ClaimsIdentity(claims, "Password"),
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.Add(_configuration.JwtAccessTokenExpiryTime),
                NotBefore = DateTime.UtcNow,
                SigningCredentials = new SigningCredentials(GetSecurityKey(HardwareId.Get()), GetSecurityAlgorithm(_configuration.EncryptionBits))
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generate a token that can be used to serve secure images
        /// </summary>
        /// <param name="context"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<string?> GenerateImagesTokenAsync(BinnerContext context, UserContext user)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (user == null) throw new ArgumentNullException(nameof(user));

            var userToken = new UserToken()
            {
                Token = ConfirmationTokenGenerator.NewToken(),
                TokenTypeId = TokenTypes.ImagesToken,
                UserId = user.UserId,
                OrganizationId = user.OrganizationId,
                DateCreatedUtc = DateTime.UtcNow,
                DateModifiedUtc = DateTime.UtcNow,
                DateExpiredUtc = DateTime.UtcNow.Add(_configuration.JwtAccessTokenExpiryTime).AddMinutes(5),
            };
            context.UserTokens.Add(userToken);
            await context.SaveChangesAsync();

            return userToken.Token;
        }

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
            => new TokenValidationParameters
            {
                IssuerSigningKey = GetSecurityKey(HardwareId.Get()),
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

        public static class TokenLifetimeValidator
        {
            public static bool Validate(DateTime? notBefore, DateTime? expires, SecurityToken tokenToValidate, TokenValidationParameters @param)
                => expires > DateTime.UtcNow;
        }
    }
}
