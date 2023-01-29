using Binner.Web.Configuration;
using Binner.Web.WebHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;
using Topshelf.Runtime;
using Binner.Common.IO;
using System.Reflection;
using Binner.Common.Configuration;
using Binner.Common.Extensions;
using NLog;
using Microsoft.Extensions.Hosting;
using NLog.Web;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Binner.Common;
using TypeSupport.Extensions;
using Binner.Model.Common;

namespace Binner.Web.ServiceHost
{
    /// <summary>
    /// The host for the web service
    /// </summary>
    [Description("Hosts the Binner Web Service")]
    [DisplayName("Binner")]
    public class BinnerWebHostService : IHostService, IDisposable
    {
        const string ConfigFile = "appsettings.json";
        private const string CertificatePassword = "password";
        private bool _isDisposed;
        private ILogger<BinnerWebHostService> _logger;
        private WebHostServiceConfiguration _config;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private IWebHost _webHost;

        public bool Start(HostControl hostControl)
        {

            Task.Run(() => InitializeWebHostAsync()).ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    if (t.Exception.InnerException is IOException && t.Exception.InnerException.Message.Contains("already in use"))
                        _logger.LogError($"Error: {typeof(BinnerWebHostService).GetDisplayName()} cannot bind to port {_config.Port}. Please check that the service is not already running.");
                    else if (t.Exception.InnerException is TaskCanceledException)
                    {
                        // do nothing
                    }
                    else
                        _logger.LogError(t.Exception, $"{typeof(BinnerWebHostService).GetDisplayName()} had an error starting up!");
                    ShutdownWebHost();
                }
            }, TaskContinuationOptions.OnlyOnFaulted); ;
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            ShutdownWebHost();
            return true;
        }

        private async Task InitializeWebHostAsync()
        {
            // run without awaiting to avoid service startup delays, and allow the service to shutdown cleanly
            var configPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            var configFile = Path.Combine(configPath, ConfigFile);
            var configuration = Config.GetConfiguration(configFile);
            _config = configuration.GetSection(nameof(WebHostServiceConfiguration)).Get<WebHostServiceConfiguration>();

            // parse the requested IP from the config
            var ipAddress = IPAddress.Any;
            var ipString = _config.IP;
            if (!string.IsNullOrEmpty(ipString) && ipString != "*")
                IPAddress.TryParse(_config.IP, out ipAddress);

            // use embedded certificate
            var certificateBytes = ResourceLoader.LoadResourceBytes(Assembly.GetExecutingAssembly(), @"Certificates.Binner.pfx");
            var certificate = new X509Certificate2(certificateBytes, CertificatePassword);

            var host = Microsoft.AspNetCore.WebHost
            .CreateDefaultBuilder()
            .ConfigureKestrel(options =>
            {
                if (certificate != null)
                {
                    options.ConfigureHttpsDefaults(opt =>
                    {
                        opt.ServerCertificate = certificate;
                        opt.ClientCertificateMode = ClientCertificateMode.NoCertificate;
                        opt.CheckCertificateRevocation = false;
                        opt.AllowAnyClientCertificate();
                    });
                }
                options.Listen(ipAddress, _config.Port, c =>
                {
                    c.UseHttps();
                });
            })
            .UseEnvironment(_config.Environment.ToString())
            .UseStartup<Startup>()
            .ConfigureLogging(logging =>
            {
                logging.AddNLogWeb();
            })
            .UseNLog();
            _webHost = host.Build();
            _logger = _webHost.Services.GetRequiredService<ILogger<BinnerWebHostService>>();
            _logger.LogInformation($"Using SSL Certificate: '{certificate.Subject}' '{certificate.FriendlyName}'");

            await _webHost.RunAsync(_cancellationTokenSource.Token);

            // stop the service
            _logger.LogInformation($"WebHost stopped!");
            // because of the way storage providers are initialized using RegisterInstance(), we must dispose of it manually
            _webHost.Services.GetRequiredService<IStorageProvider>()
                ?.Dispose();
        }

        /// <summary>
        /// Shuts down the current webhost
        /// </summary>
        private void ShutdownWebHost()
        {
            try
            {
                _cancellationTokenSource.Cancel();
                Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to shutdown WebHost!");
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (_isDisposed)
                return;

            if (isDisposing)
            {
                _webHost?.Dispose();
            }
            _isDisposed = true;
        }
    }
}
