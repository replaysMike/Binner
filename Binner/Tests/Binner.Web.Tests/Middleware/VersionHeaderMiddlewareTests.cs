using Binner.Web.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Binner.Web.Tests.Middleware
{
    [TestFixture]
    public class VersionHeaderMiddlewareTests
    {
        [Test]
        public async Task VersionMiddleware_ShouldMatch()
        {
            using var host = await new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.Configure(app =>
                    {
                        app.UseMiddleware<VersionHeaderMiddleware>();
                    });
                    webBuilder.UseTestServer();
                }).StartAsync();

            var server = host.GetTestServer();
            var context = await server.SendAsync(context =>
            {
                context.Request.Method = HttpMethods.Get;
                context.Request.Path = "/fake";
            });

            Assert.That(context.Response.Headers.ContainsKey("X-Version"), Is.True);
        }
    }
}
