using Binner.Global.Common;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Binner.Services.SignalR
{
    /// <summary>
    /// SignalR/websocket hub for UI client messaging
    /// </summary>
    public class SystemHub : Hub
    {
        private readonly ILogger<SystemHub> _logger;
        private readonly IRequestContextAccessor _requestContext;

        public SystemHub(ILogger<SystemHub> logger, IRequestContextAccessor requestContext)
        {
            _logger = logger;
            _requestContext = requestContext;
        }

        public override Task OnConnectedAsync()
        {
            var context = Context;
            var user = context.User;
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Subscribe to approval request counts
        /// </summary>
        /// <returns></returns>
        public async Task SubscribeSubscriptions()
        {
            var user = _requestContext.GetUserContext();
            try
            {
                if (user != null)
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"{user.OrganizationId}:subscriptions");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to subscribe to subscriptions as '{user?.OrganizationId}:subscriptions'");
            }
        }

        /// <summary>
        /// Unsubscribe from approval request counts
        /// </summary>
        /// <returns></returns>
        public async Task UnsubscribeSubscriptions()
        {
            var user = _requestContext.GetUserContext();
            try
            {
                if (user != null)
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{user.OrganizationId}:subscriptions");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to unsubscribe from subscriptions as '{user?.OrganizationId}:subscriptions'");
            }
        }
    }
}
