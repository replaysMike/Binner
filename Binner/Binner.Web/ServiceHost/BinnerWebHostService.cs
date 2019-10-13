using Binner.Web.Configuration;
using Binner.Web.WebHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using Topshelf;
using Topshelf.Runtime;

namespace Binner.Web.ServiceHost
{
    /// <summary>
    /// The host for the web service
    /// </summary>
    [Description("Hosts the Binner Web Service")]
    [DisplayName("Binner Web Service")]
    public class BinnerWebHostService : IHostService, IDisposable
    {
        private bool _isDisposed;
        private readonly WebHostServiceConfiguration _config;
        private HostSettings _hostSettings;

        /// <summary>
        /// The logging facility used by the service.
        /// </summary>
        public ILogger<BinnerWebHostService> Logger { get; }
        public IServiceProvider ServiceProvider { get; }
        public IWebHostFactory WebHostFactory { get; }
        public IWebHost WebHost { get; private set; }

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
            InitializeWebHost();
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            ShutdownWebHost();
            return true;
        }

        private void InitializeWebHost()
        {
            // run without awaiting to avoid service startup delays
            Task.Run(async () => {
                // parse the requested IP from the config
                var ipAddress = IPAddress.Any;
                var ipString = _config.IP;
                if (!string.IsNullOrEmpty(ipString) && ipString != "*")
                    IPAddress.TryParse(_config.IP, out ipAddress);

                WebHost = WebHostFactory.CreateHttps(ipAddress, _config.Port);

                await WebHost.RunAsync();
            }).ContinueWith(t => {
                if (t.Exception != null)
                {
                    // Logger.LogError(t.Exception, $"{_hostSettings.ServiceName} had an error starting up!");
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
