using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Binner.Web.WebHost
{
    public interface IWebHostFactory
    {
        /// <summary>
        /// Creates a new web host
        /// </summary>
        /// <param name="ipAddress">The IP address of the host</param>
        /// <param name="port">The port of the host</param>
        /// <param name="certificate">The SSL certificate to use</param>
        /// <returns></returns>
        IWebHost CreateHttps(IPAddress ipAddress, int? port = 80, X509Certificate2 certificate = null);
    }
}
