using Binner.Global.Common;
using Binner.Model.Authentication;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Binner.Common
{
    /// <summary>
    /// Represents a user created via a token
    /// </summary>
    public class TokenPrincipal : ClaimsPrincipal
    {
        public IUserContext UserContext { get; }
        public string? Token { get; set; }

        public TokenPrincipal(IUserContext userContext, string? token) : base(new ClaimsIdentity(CreateClaims(userContext), "TokenPrincipal"))
        {
            UserContext = userContext;
            Token = token;
        }

        private static IEnumerable<Claim> CreateClaims(IUserContext userContext)
        {
            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, userContext.UserId.ToString()),
                new (JwtClaimTypes.UserId, userContext.UserId.ToString()),
                new (JwtClaimTypes.OrganizationId, userContext.OrganizationId.ToString()),
                new (JwtClaimTypes.FullName, userContext.Name ?? string.Empty),
                new (ClaimTypes.Name, userContext.EmailAddress ?? string.Empty),
                new (ClaimTypes.HomePhone, string.IsNullOrEmpty(userContext.PhoneNumber) ? "" : userContext.PhoneNumber),
                new (JwtClaimTypes.CanLogin, userContext.CanLogin.ToString())
            };
            return claims;
        }
    }
}
