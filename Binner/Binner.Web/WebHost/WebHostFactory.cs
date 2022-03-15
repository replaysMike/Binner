using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using NLog.Web;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Binner.Web.WebHost
{
    public class WebHostFactory : IWebHostFactory
    {
        /// <summary>
        /// Creates a new HTTPS-only web host
        /// </summary>
        /// <param name="ipAddress">The IP address of the host</param>
        /// <param name="port">The port of the host</param>
        /// <param name="environment">Environment to run</param>
        /// <param name="certificate">The name of the SSL certificate to use</param>
        /// <returns></returns>
        public IWebHost CreateHttps(IPAddress ipAddress, int? port = 443, string environment = "Development", X509Certificate2 certificate = null)
        {
            var host = Microsoft.AspNetCore.WebHost
                .CreateDefaultBuilder()
                .ConfigureKestrel(options => {
                    if (certificate != null)
                    {
                        options.ConfigureHttpsDefaults(opt =>
                        {
                            opt.ServerCertificate = certificate;
                            opt.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
                        });
                    }
                    options.Listen(ipAddress, port.Value, c => {
                        if (certificate != null)
                            c.UseHttps(certificate);
                        else
                            c.UseHttps();
                    });
                })
                .UseEnvironment(environment)
                .UseStartup<Startup>()
                .ConfigureLogging(logging => {
                    // todo:
                })
                .UseNLog();
            return host.Build();
        }
    }
}
