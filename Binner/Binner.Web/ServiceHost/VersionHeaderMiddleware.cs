using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Binner.Web.ServiceHost
{
    /// <summary>
    /// Adds the application version to the response header
    /// </summary>
    public class VersionHeaderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Version _buildVersion;

        public VersionHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
            _buildVersion = Assembly.GetExecutingAssembly().GetName().Version;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.OnStarting(state =>
            {
                context.Response.Headers.Add("X-Version", $"{_buildVersion.ToString(3)}");
                return Task.CompletedTask;
            }, context);

            await _next(context);
        }
    }

    public static class VersionHeaderMiddlewareExtensions
    {
        /// <summary>
        /// Adds the application version to the response header
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseVersionHeader(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<VersionHeaderMiddleware>();
        }
    }
}
