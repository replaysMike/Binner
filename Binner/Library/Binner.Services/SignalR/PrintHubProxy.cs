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
    public class PrintHubProxy : IPrintHubProxy
    {
        private readonly ILogger<PrintHubProxy> _logger;
        private readonly IHubContext<PrintHub> _hubContext;

        public PrintHubProxy(ILogger<PrintHubProxy> logger, IHubContext<PrintHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Notify of a print job
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task NotifyPrintAsync(Guid printSpoolQueueId)
        {
            if (printSpoolQueueId == Guid.Empty) return;

            try
            {
                var group = _hubContext.Clients.Group($"{printSpoolQueueId}:print");
                await group.SendAsync("PrintQueued", printSpoolQueueId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending print notification.");
            }
        }
    }
}
