using Binner.Global.Common;
using Binner.Global.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Binner.Services.SignalR
{
    /// <summary>
    /// Client for sending messages via SignalR/websocket
    /// </summary>
    [Authorize]
    public class SystemHubProxy : ISystemHubProxy
    {
        private readonly ILogger<SystemHubProxy> _logger;
        private readonly IHubContext<SystemHub> _hubContext;

        public SystemHubProxy(ILogger<SystemHubProxy> logger, IHubContext<SystemHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Notify of a subscription level change
        /// </summary>
        /// <param name="subscriptionLevel"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public async Task NotifySubscriptionChangeAsync(SubscriptionLevel subscriptionLevel, int organizationId)
        {
            try
            {
                var group = _hubContext.Clients.Group($"{organizationId}:subscriptions");
                await group.SendAsync("SubscriptionLevelChange", subscriptionLevel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending subscription change notification.");
            }
        }

        /// <summary>
        /// Notify of a print completed
        /// </summary>
        /// <param name="partName"></param>
        /// <param name="userId"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public async Task NotifyPrintCompleteAsync(string partName, int userId, int organizationId)
        {
            try
            {
                var group = _hubContext.Clients.Group($"{organizationId}:{userId}:system");
                await group.SendAsync("PrintComplete", partName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending print complete notification.");
            }
        }
    }
}
