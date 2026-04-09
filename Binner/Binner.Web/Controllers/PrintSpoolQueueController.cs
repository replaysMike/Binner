using Binner.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Binner.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json)]
    public class PrintSpoolQueueController : ControllerBase
    {
        private readonly ILogger<PrintSpoolQueueController> _logger;
        private readonly IPrintSpoolQueueService _printSpoolQueueService;

        public PrintSpoolQueueController(ILogger<PrintSpoolQueueController> logger, IPrintSpoolQueueService printSpoolQueueService)
        {
            _logger = logger;
            _printSpoolQueueService = printSpoolQueueService;
        }

        /// <summary>
        /// Get the print configuration
        /// </summary>
        /// <param name="printSpoolQueueId"></param>
        /// <returns></returns>
        [HttpGet("configuration")]
        [AllowAnonymous]
        public async Task<IActionResult> GetConfigurationAsync([FromQuery] Guid printSpoolQueueId)
        {
            var response = await _printSpoolQueueService.GetConfigurationAsync(printSpoolQueueId);
            if (response == null) return BadRequest();
            return Ok(response);
        }

        /// <summary>
        /// Get the pending print spool queue
        /// </summary>
        /// <param name="printSpoolQueueId"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> GetAsync([FromQuery] Guid printSpoolQueueId)
        {
            var response = await _printSpoolQueueService.GetPendingAsync(printSpoolQueueId);
            if (response == null) return BadRequest();
            return Ok(response);
        }

        /// <summary>
        /// Delete a pending print spool queue item
        /// </summary>
        /// <param name="printSpoolQueueId"></param>
        /// <returns></returns>
        [HttpDelete]
        [AllowAnonymous]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> DeleteAsync([FromQuery] Guid printSpoolQueueId, [FromQuery] Guid globalId)
        {
            var isSuccess = await _printSpoolQueueService.DeletePrintSpoolQueueAsync(printSpoolQueueId, globalId);
            if (isSuccess)
                return Ok();
            return BadRequest();
        }
    }
}
