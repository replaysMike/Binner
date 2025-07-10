using Binner.Global.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace Binner.Web.Authorization
{
    /// <summary>
    /// Checks if a subscription policy requirement fails, and attaches a special message in the header.
    /// Doesn't perform any actual policy checks, they have already taken place.
    /// </summary>
    public class SubscriptionPolicyEvaluator : PolicyEvaluator
    {
        public SubscriptionPolicyEvaluator(IAuthorizationService authorization) : base(authorization)
        {
        }

        public override async Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context, object? resource)
        {
            var result = await base.AuthorizeAsync(policy, authenticationResult, context, resource);
            if (result.Forbidden)
            {
                // If user is authenticated but not allowed, attach a special message in the header.
                // We aren't able to write to the response body without exceptions, so a header will have to do.
                if (context.User.Identity.IsAuthenticated)
                {
                    var message = "Access forbidden by policy.";
                    var claimsPolicy = policy.Requirements
                        .Where(x => typeof(ClaimsAuthorizationRequirement).IsAssignableFrom(x.GetType()))
                        .FirstOrDefault() as ClaimsAuthorizationRequirement;
                    if (claimsPolicy != null)
                    {
                        if (claimsPolicy.ClaimType == JwtClaimTypes.CanLogin)
                        {
                            message = $"Authenticated user must confirm their email address, and account must not be locked.";
                        }
                        if (claimsPolicy.ClaimType == JwtClaimTypes.SubscriptionLevel)
                        {
                            message = $"{JwtClaimTypes.SubscriptionLevel} must be of type(s) '{string.Join(", ", claimsPolicy.AllowedValues)}' to access this resource.";
                        }
                        if (claimsPolicy.ClaimType == JwtClaimTypes.Admin)
                        {
                            message = $"Authenticated user must an admin to access this resource.";
                        }
                    }
                    context.Response.Headers.Add("X-Policy-Failure", message);
                    return PolicyAuthorizationResult.Forbid();
                }

            }
            return result;
        }
    }
}
