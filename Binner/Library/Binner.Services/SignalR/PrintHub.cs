using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Binner.Services.SignalR
{
    /// <summary>
    /// SignalR/websocket hub for UI client messaging
    /// </summary>
    public class PrintHub : Hub
    {
        private readonly ILogger<PrintHub> _logger;

        public PrintHub(ILogger<PrintHub> logger)
        {
            _logger = logger;
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
        /// Subscribe to printing messages
        /// </summary>
        /// <returns></returns>
        public async Task SubscribePrint(Guid printSpoolQueueId)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"{printSpoolQueueId}:print");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to subscribe to printing as '{printSpoolQueueId}:print'");
            }
        }

        /// <summary>
        /// Unsubscribe from printing messages
        /// </summary>
        /// <returns></returns>
        public async Task UnsubscribePrint(Guid printSpoolQueueId)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{printSpoolQueueId}:print");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to unsubscribe from printing as '{printSpoolQueueId}:print'");
            }
        }
    }
}
