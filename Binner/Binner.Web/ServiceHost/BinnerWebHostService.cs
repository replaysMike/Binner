using Binner.Common;
using Binner.Common.Configuration;
using Binner.Common.Extensions;
using Binner.Common.IO;
using Binner.Common.StorageProviders;
using Binner.Data;
using Binner.Model.Common;
using Binner.Model.Configuration;
using Binner.Web.Configuration;
using Binner.Web.WebHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Web;
using NPOI.OpenXmlFormats.Dml;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;

namespace Binner.Web.ServiceHost
{
    /// <summary>
    /// The host for the web service
    /// </summary>
    [Description("Hosts the Binner Web Service")]
    [DisplayName("Binner")]
    public partial class BinnerWebHostService : IHostService, IDisposable
    {
        const string ConfigFile = "appsettings.json";
        private const string CertificatePassword = "password";
        const string LogManagerConfigFile = "nlog.config"; // TODO: Inject from appsettings
        private static readonly string _logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogManagerConfigFile);
        private static readonly Logger _nlogLogger = NLog.Web.NLogBuilder.ConfigureNLog(_logFile).GetCurrentClassLogger();

        private bool _isDisposed;
        private ILogger<BinnerWebHostService>? _logger;
        private WebHostServiceConfiguration? _config;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private IWebHost? _webHost;

        public bool Start(HostControl hostControl)
        {
            Task.Run(InitializeWebHostAsync).ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    if (t.Exception.InnerException is IOException && t.Exception.InnerException.Message.Contains("already in use"))
                    {
                        var message = $"Error: {typeof(BinnerWebHostService).GetDisplayName()} cannot bind to port '{_config?.Port}'. Please check that the service is not already running.";
                        if (_logger == null)
                            _nlogLogger.Error(t.Exception, message);
                        else
                            _logger.LogError(t.Exception, message);
                    }
                    else if (t.Exception.InnerException is TaskCanceledException)
                    {
                        // do nothing
                    }
                    else
                    {
                        var message = $"{typeof(BinnerWebHostService).GetDisplayName()} had an error starting up!";
                        if (_logger == null)
                            _nlogLogger.Error(t.Exception, message);
                        else
                            _logger.LogError(t.Exception, message);
                    }
                    hostControl.Stop();
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
            var configPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName) ?? string.Empty;
            var configFile = Path.Combine(configPath, ConfigFile);
            var configuration = Config.GetConfiguration(configFile);
            _nlogLogger.Info($"Loading configuration at {configFile}");
            _config = configuration.GetSection(nameof(WebHostServiceConfiguration)).Get<WebHostServiceConfiguration>() ??
                      throw new BinnerConfigurationException($"Configuration section '{nameof(WebHostServiceConfiguration)}' does not exist!");

            var configValidator = new ConfigurationValidator(_nlogLogger);

            configValidator.ValidateConfiguration(_config);

            // parse the requested IP from the config
            var ipAddress = IPAddress.Any;
            var ipString = _config.IP;
            if (!string.IsNullOrEmpty(ipString) && ipString != "*")
                if (!IPAddress.TryParse(_config.IP, out ipAddress))
                    throw new BinnerConfigurationException($"Failed to parse IpAddress '{ipString}'");

            X509Certificate2? certificate = null;
            var certFilename = Path.GetFullPath(_config.SslCertificate);
            try
            {
                var result = Binner.Common.Security.CertificateLoader.LoadCertificate(certFilename, _config.SslCertificatePassword);
                certificate = result.Certificate;
                if (certificate != null)
                {
                    _nlogLogger.Info($"{result.CertType} Certificate loaded from '{certFilename}'");
                    _nlogLogger.Info($"Using SSL Certificate: '{certificate.Subject}' '{certificate.FriendlyName}'");
                }
            }
            catch (Exception ex)
            {
                _nlogLogger.Error(ex, "Failed to load SSL certificate.");
                throw;
            }

            // storage provider config
            var storageConfig = configuration.GetSection(nameof(StorageProviderConfiguration)).Get<StorageProviderConfiguration>() ??
                                throw new BinnerConfigurationException($"Configuration section '{nameof(StorageProviderConfiguration)}' does not exist!");

            configValidator.ValidateConfiguration(storageConfig);

            var migrationHostBuilder = HostBuilderFactory.Create(storageConfig);
            var migrationHost = migrationHostBuilder.Build();
            var contextFactory = migrationHost.Services.GetRequiredService<IDbContextFactory<BinnerContext>>();
            var migrationHandler = new MigrationHandler(_nlogLogger, storageConfig, contextFactory);
            if (migrationHandler.TryDetectMigrateNeeded(out var db))
            {
                if (migrationHandler.MigrateDatabase(db))
                {
                    _nlogLogger.Info("Database was successfully migrated to Sqlite!");
                }
                else
                {
                    _nlogLogger.Error("Failed to migrate Binner Database!");
                    return;
                }
            }

            // ensure the creation of important paths
            if (!Directory.Exists(storageConfig.UserUploadedFilesPath))
            {
                try
                {
                    Directory.CreateDirectory(storageConfig.UserUploadedFilesPath);
                }
                catch (Exception ex)
                {
                    _nlogLogger.Error($"Failed to create directory at ${storageConfig.UserUploadedFilesPath}. {ex.GetBaseException().Message}");
                    return;
                }
            }

            _nlogLogger.Info($"Building the WebHost on {ipAddress}:{_config.Port} ...");
            var host = Microsoft.AspNetCore.WebHost
                .CreateDefaultBuilder()
                .ConfigureKestrel(options =>
                {
                    options.Listen(ipAddress, 7000, c => { c.UseHttps(certificate); });
                })
                .UseEnvironment(_config.Environment.ToString())
                .UseStartup<Startup>()
                .ConfigureLogging(logging => { logging.AddNLogWeb(); })
                .UseNLog();
            _webHost = host.Build();
            ApplicationLogging.LoggerFactory = _webHost.Services.GetRequiredService<ILoggerFactory>();
            _logger = _webHost.Services.GetRequiredService<ILogger<BinnerWebHostService>>();

            await using (var context = migrationHost.Services.GetRequiredService<BinnerContext>())
            {
                try
                {
                    _logger.LogInformation($"Applying EF migrations to {storageConfig.Provider} database...");
                    await context.Database.MigrateAsync();

                    // apply a Users patch for any customers that were affected
                    await ApplyOrganizationIdPatchAsync(context);
                    // end patch

                    _logger.LogInformation($"{storageConfig.Provider} EF migrations successfully applied!");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to migrate {storageConfig.Provider} database!");
                    return;
                }
            }

            await _webHost.RunAsync(_cancellationTokenSource.Token);

            // stop the service
            _logger.LogInformation($"WebHost stopped!");
            // because of the way storage providers are initialized using RegisterInstance(), we must dispose of it manually
            _webHost.Services.GetRequiredService<IStorageProvider>()
                ?.Dispose();
        }

        /// <summary>
        /// Shuts down the current web host
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
                _logger?.LogError(ex, "Failed to shutdown WebHost!");
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
