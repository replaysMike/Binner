using Binner.Model.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Binner.Web.Middleware
{
    /// <summary>
    /// Global error handler for returning 500 errors as json, with body
    /// </summary>
    public class GlobalErrorMiddleware
    {
        private readonly ILogger<GlobalErrorMiddleware> _logger;
        private readonly RequestDelegate _next;

        public GlobalErrorMiddleware(RequestDelegate next, ILogger<GlobalErrorMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                if (context.Request != null)
                    _logger.LogError(ex, $"{nameof(GlobalErrorMiddleware)} exception occurred during request to '{context.Request.Path}'!");
                else
                    _logger.LogError(ex, $"{nameof(GlobalErrorMiddleware)} exception occurred!");
                var response = context.Response;
                if (!response.HasStarted)
                {
                    response.StatusCode = StatusCodes.Status500InternalServerError;
                    response.ContentType = "application/json";
                    var result = JsonSerializer.Serialize(new ExceptionResponse("Unhandled Error! ", ex));
                    await response.WriteAsync(result);
                }
            }
        }
    }
}
