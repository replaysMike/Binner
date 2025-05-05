using AnyMapper;
using Binner.Common.Services;
using Binner.Model;
using Binner.Model.Requests;
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
    public class OrderImportHistoryController : ControllerBase
    {
        private readonly ILogger<OrderImportHistoryController> _logger;
        private readonly IOrderImportHistoryService _orderImportHistoryService;

        public OrderImportHistoryController(ILogger<OrderImportHistoryController> logger, IOrderImportHistoryService orderImportHistoryService)
        {
            _logger = logger;
            _orderImportHistoryService = orderImportHistoryService;
        }

        /// <summary>
        /// Get an existing order import history
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery]GetOrderImportHistoryRequest request)
        {
            var orderImportHistory = await _orderImportHistoryService.GetOrderImportHistoryAsync(request.OrderNumber, request.Supplier, request.IncludeLineItems);
            if (orderImportHistory == null) return NotFound();
            
            return Ok(orderImportHistory);
        }

        /// <summary>
        /// Create a new order import history
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateOrderImportHistoryAsync(CreateOrderImportHistoryRequest request)
        {
            var mappedPartScanHistory = Mapper.Map<CreateOrderImportHistoryRequest, OrderImportHistory>(request);
            mappedPartScanHistory.DateCreatedUtc = DateTime.UtcNow;
            var orderImportHistory = await _orderImportHistoryService.AddOrderImportHistoryAsync(mappedPartScanHistory);
            return Ok(orderImportHistory);
        }

        /// <summary>
        /// Update an existing order import history
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> UpdateOrderImportHistoryAsync(UpdateOrderImportHistoryRequest request)
        {
            var mappedPartScanHistory = Mapper.Map<UpdateOrderImportHistoryRequest, OrderImportHistory>(request);
            var orderImportHistory = await _orderImportHistoryService.UpdateOrderImportHistoryAsync(mappedPartScanHistory);
            return Ok(orderImportHistory);
        }

        /// <summary>
        /// Delete an existing order import history
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteOrderImportHistoryAsync(DeleteOrderImportHistoryRequest request)
        {
            var isDeleted = await _orderImportHistoryService.DeleteOrderImportHistoryAsync(new OrderImportHistory
            {
                OrderImportHistoryId = request.OrderImportHistoryId
            });
            return Ok(isDeleted);
        }
    }
}
