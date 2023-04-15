using Binner.Model.Common;
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

        public TokenPrincipal(IUserContext userContext, string? token) : base(new ClaimsIdentity("TokenPrincipal"))
        {
            UserContext = userContext;
            Token = token;
        }
    }
}
