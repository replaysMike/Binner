using System.Security.Claims;

namespace Binner.Global.Common
{
    /// <summary>
    /// Used for backend service calls that don't require a user context
    /// </summary>
    public class ServiceMockPrincipal : ClaimsPrincipal
    {
        public ServiceMockPrincipal() : base(new ClaimsIdentity("Mock"))
        {
        }
    }
}
