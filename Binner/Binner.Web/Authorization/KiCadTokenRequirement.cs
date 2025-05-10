using Microsoft.AspNetCore.Authorization;

namespace Binner.Web.Authorization
{
    public class KiCadTokenRequirement : IAuthorizationRequirement
    {
        public string Token { get; private set; }
        public KiCadTokenRequirement(string token) { Token = token; }
    }
}
