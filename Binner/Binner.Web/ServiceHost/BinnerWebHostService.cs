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

namespace Binner.Web.ServiceHost
{
    /// <summary>
    /// The host for the web service
    /// </summary>
    [Description("Hosts the Binner Web Service")]
    [DisplayName("Binner Web Service")]
    public class BinnerWebHostService : IHostService, IDisposable
    {
        private const string CertificatePassword = "password";
        private bool _isDisposed;
        private readonly WebHostServiceConfiguration _config;
        private HostSettings _hostSettings;
        public static CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        /// <summary>
        /// The logging facility used by the service.
        /// </summary>
        public ILogger<BinnerWebHostService> Logger { get; }
        public IServiceProvider ServiceProvider { get; }
        public IWebHostFactory WebHostFactory { get; }
        public IWebHost WebHost { get; private set; }
        public static HostControl Host { get; private set; }

        public BinnerWebHostService(HostSettings hostSettings, WebHostServiceConfiguration config, IWebHostFactory webHostFactory,
            ILogger<BinnerWebHostService> logger, IServiceProvider serviceProvider)
        {
            _hostSettings = hostSettings;
            _config = config ?? throw new ArgumentNullException(nameof(config));
            WebHostFactory = webHostFactory ?? throw new ArgumentNullException(nameof(webHostFactory));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }


        public bool Start(HostControl hostControl)
        {
            Host = hostControl;
            InitializeWebHost();
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            Host = hostControl;
            ShutdownWebHost();
            return true;
        }

        private void InitializeWebHost()
        {
            // run without awaiting to avoid service startup delays
            Task.Run(async () =>
            {
                // parse the requested IP from the config
                var ipAddress = IPAddress.Any;
                var ipString = _config.IP;
                if (!string.IsNullOrEmpty(ipString) && ipString != "*")
                    IPAddress.TryParse(_config.IP, out ipAddress);

                // use embedded certificate
                var certificateBytes = ResourceLoader.LoadResourceBytes(Assembly.GetExecutingAssembly(), @"Certificates.Binner.pfx");
                var certificate = new X509Certificate2(certificateBytes, CertificatePassword);

                Logger.LogInformation($"Using SSL Certificate: '{certificate.Subject}' '{certificate.FriendlyName}'");
                WebHost = WebHostFactory.CreateHttps(ipAddress, _config.Port, _config.Environment.ToString(), certificate);
                await WebHost.RunAsync(CancellationTokenSource.Token);
            }).ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    Logger.LogError(t.Exception, $"{_hostSettings.ServiceName} had an error starting up!");
                    Stop(null);
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// Shuts down the current webhost
        /// </summary>
        private void ShutdownWebHost()
        {
            Dispose();
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
                WebHost?.Dispose();
                WebHost = null;
            }
            _isDisposed = true;
        }
    }
}
