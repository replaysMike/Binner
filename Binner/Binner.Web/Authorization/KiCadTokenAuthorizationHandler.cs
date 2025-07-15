using Binner.Common.Extensions;
using Binner.Services;
using Binner.Data;
using Binner.Global.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Binner.Web.Authorization
{
    /// <summary>
    /// Authorizes requests using a Token header for the KiCad API
    /// </summary>
    public class KiCadTokenAuthorizationHandler : AuthorizationHandler<KiCadTokenAuthorizeAttribute>
    {
        private readonly ILogger<KiCadTokenAuthorizationHandler> _logger;
        private readonly IDbContextFactory<BinnerContext> _contextFactory;
        private readonly IRequestContextAccessor _requestContext;
        private readonly IAuthenticationService _authenticationService;

        public KiCadTokenAuthorizationHandler(ILogger<KiCadTokenAuthorizationHandler> logger, IDbContextFactory<BinnerContext> contextFactory, IRequestContextAccessor requestContext, IAuthenticationService authenticationService)
        {
            _logger = logger;
            _contextFactory = contextFactory;
            _requestContext = requestContext;
            _authenticationService = authenticationService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, KiCadTokenAuthorizeAttribute requirement)
        {
            var headerValue = _requestContext.GetHeader(requirement.HeaderName);
            var parts = headerValue?.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            var token = string.Empty;
            if (parts?.Length == 2 && parts[0] == requirement.TokenName)
            {
                token = parts[1];
                _logger.LogDebug($"[{nameof(KiCadTokenAuthorizationHandler)}] Authorizing KiCad token '{token.Sanitize()}'...");
                await using var dbContext = await _contextFactory.CreateDbContextAsync();
                var tokenEntity = await dbContext.UserTokens.Where(x => x.Token == token && x.DateRevokedUtc == null && (x.DateExpiredUtc > DateTime.UtcNow || x.DateExpiredUtc == null))
                    .FirstOrDefaultAsync();
                if (tokenEntity != null)
                {
                    // authorized
                    var userId = tokenEntity.UserId.Value;
                    var organizationId = tokenEntity.OrganizationId;
                    var principal = await _authenticationService.SetCurrentUserFromIdAsync(userId);
                    var identity = principal.Identity as ClaimsIdentity;
                    context.User.AddIdentity(identity);
                    _logger.LogDebug($"[{nameof(KiCadTokenAuthorizationHandler)}] KiCad token '{token.Sanitize()}' authorized.");
                    if (context.Requirements.Count() > 1)
                        context.Succeed(context.Requirements.First()); // bypass the need for claims based authentication
                    context.Succeed(requirement);
                    return;
                }
            }
            // not authorized
            var message = $"KiCad token '{token.Sanitize()}' is invalid. Please check that you have generated a KiCad token (Account Settings => User Tokens) and it is specified in your 'Binner.kicad_httplib' configuration file.";
            _logger.LogError(message);
            context.Fail(new AuthorizationFailureReason(this, message));
        }
    }
}
