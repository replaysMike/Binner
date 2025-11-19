using Binner.Common;
using Binner.Common.Configuration;
using Binner.Common.Extensions;
using Binner.Data;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Services.Security;
using Binner.Web.Configuration;
using Binner.Web.Database;
using Binner.Web.WebHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
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
        private static readonly string _configFile = EnvironmentVarConstants.GetEnvOrDefault(EnvironmentVarConstants.Config, AppConstants.AppSettings);
        private static readonly string _logManagerConfigFile = EnvironmentVarConstants.GetEnvOrDefault(EnvironmentVarConstants.NlogConfig, AppConstants.NLogConfig);
        private static readonly string _logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _logManagerConfigFile);
        private static readonly Logger _nlogLogger = NLog.Web.NLogBuilder.ConfigureNLog(_logFile).GetCurrentClassLogger();
        private static readonly CertificateUtility _certificateUtility = new CertificateUtility(_nlogLogger, _configFile);

        private bool _isDisposed;
        private ILogger<BinnerWebHostService>? _logger;
        private WebHostServiceConfiguration? _webHostConfig;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private IWebHost? _webHost;

        public bool Start(HostControl hostControl)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            RunStartAsync(hostControl);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            _nlogLogger.Info("Start completed()");
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            ShutdownWebHost();
            return true;
        }

        private async Task RunStartAsync(HostControl hostControl)
        {
            _nlogLogger.Info("Start()");
            await Task.Run(InitializeWebHostAsync).ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    if (t.Exception.InnerException is IOException && t.Exception.InnerException.Message.Contains("already in use"))
                    {
                        var message = $"Error: {typeof(BinnerWebHostService).GetDisplayName()} cannot bind to port '{_webHostConfig?.Port}'. Please check that the service is not already running.";
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
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private async Task InitializeWebHostAsync()
        {
            var version = Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString(3) ?? "0.0.0";
            _nlogLogger.Info($"Binner Service v{version} starting...");
            // run without awaiting to avoid service startup delays, and allow the service to shutdown cleanly
            var configPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName) ?? string.Empty;
            var configFile = Path.Combine(configPath, _configFile);
            var configuration = Config.GetConfiguration(configFile);
            _nlogLogger.Info($"Loading configuration at {configFile}");
            _webHostConfig = configuration.GetSection(nameof(WebHostServiceConfiguration)).Get<WebHostServiceConfiguration>() ??
                      throw new BinnerConfigurationException($"Configuration section '{nameof(WebHostServiceConfiguration)}' does not exist in '{configFile}'!");

            var configValidator = new ConfigurationValidator(_nlogLogger);

            configValidator.ValidateConfiguration(_webHostConfig);

            // parse the requested IP from the config
            var ipAddress = IPAddress.Any;
            var ipString = _webHostConfig.IP;
            if (!string.IsNullOrEmpty(ipString) && ipString != "*")
                if (!IPAddress.TryParse(_webHostConfig.IP, out ipAddress))
                    throw new BinnerConfigurationException($"Failed to parse IpAddress '{ipString}'");

            // load the SSL certificate, or generate a new one if configured
            var certificate = _certificateUtility.LoadOrGenerateSelfSignedCertificate(_webHostConfig);

            // storage provider config
            var storageConfig = configuration.GetSection(nameof(StorageProviderConfiguration)).Get<StorageProviderConfiguration>() ??
                                throw new BinnerConfigurationException($"Configuration section '{nameof(StorageProviderConfiguration)}' does not exist!");
            // inject configuration from environment variables (if set)
            EnvironmentVarConstants.SetConfigurationFromEnvironment(storageConfig);

            // log paths
            _nlogLogger.Info($"Application Paths:");
            try
            {
                _nlogLogger.Info($"  Database: {(storageConfig.Provider.Equals("Binner", StringComparison.InvariantCultureIgnoreCase) || storageConfig.Provider.Equals("Sqlite", StringComparison.InvariantCultureIgnoreCase) ? Path.GetDirectoryName(Path.GetFullPath(storageConfig.ProviderConfiguration["Filename"])) : "External")}");
            }
            catch (Exception)
            {
                _nlogLogger.Info($"  Database: Unavailable");
            }
            _nlogLogger.Info($"  Userfiles: {Path.GetFullPath(SystemPaths.GetUserFilesPath(storageConfig))}");
            _nlogLogger.Info($"  Certificates: {Path.GetDirectoryName(Path.GetFullPath(SystemPaths.GetCerficiatePath(_webHostConfig)))}");
            try
            {
                var fileTarget = LogManager.Configuration?.FindTargetByName<NLog.Targets.FileTarget>("file");
                var logEventInfo = new LogEventInfo();
                string? fileName = fileTarget?.FileName.Render(logEventInfo);
                _nlogLogger.Info($"  Logs: {Path.GetDirectoryName(Path.GetFullPath(fileName ?? "Unavailable"))}");
            }
            catch (Exception)
            {
                _nlogLogger.Info($"  Logs: Unavailable");
            }

            configValidator.ValidateConfiguration(storageConfig);

            // ensure the creation of important paths
            var userFilesPath = SystemPaths.GetUserFilesPath(storageConfig);
            if (!Directory.Exists(userFilesPath))
            {
                try
                {
                    Directory.CreateDirectory(userFilesPath);
                }
                catch (Exception ex)
                {
                    _nlogLogger.Error($"Failed to create directory at ${userFilesPath}. {ex.GetBaseException().Message}");
                    return;
                }
            }

            _nlogLogger.Info($"Building the WebHost on {(_webHostConfig.UseHttps ? "https://" : "http://")}{ipAddress}:{_webHostConfig.Port}...");
            var hostBuilder = Microsoft.AspNetCore.WebHost
                .CreateDefaultBuilder()
                .ConfigureKestrel(options =>
                {
                    if (_webHostConfig.UseHttps && certificate != null)
                    {
                        // use https
                        Environment.SetEnvironmentVariable(EnvironmentVarConstants.SpaProtocol, "https"); // used in the UI build to support https
                        options.Listen(ipAddress, _webHostConfig.Port, c => { c.UseHttps(certificate); });
                    }
                    else
                    {
                        // use http
                        Environment.SetEnvironmentVariable(EnvironmentVarConstants.SpaProtocol, "http"); // used in the UI build to support http
                        options.Listen(ipAddress, _webHostConfig.Port);
                    }
                })
                .UseStartup<Startup>()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConfiguration(configuration);
                    logging.AddEventSourceLogger();
                    //if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                    //logging.AddConsole();
                    logging.AddNLogWeb();
                })
                .UseNLog();
            _webHost = hostBuilder.Build();
            ApplicationLogging.LoggerFactory = _webHost.Services.GetRequiredService<ILoggerFactory>();
            _logger = _webHost.Services.GetRequiredService<ILogger<BinnerWebHostService>>();


            var migrationHostBuilder = HostBuilderFactory.Create(storageConfig);
            var migrationHost = migrationHostBuilder.Build();

            // create a custom migration handler to handle migrations for different database providers
            var contextFactory = migrationHost.Services.GetRequiredService<IDbContextFactory<BinnerContext>>();
            var migrationHandler = new MigrationHandler(_nlogLogger, storageConfig, contextFactory);
            if (migrationHandler.TryDetectMigrateNeeded(out var db))
            {
                if (migrationHandler.MigrateDatabase(db))
                {
                    _nlogLogger.Info("Legacy database was successfully migrated to Sqlite!");
                }
                else
                {
                    _nlogLogger.Error("Failed to migrate Binner Database!");
                    return;
                }
            }
            // handle regular EF migrations
            if (_webHostConfig.AllowDatabaseMigrations)
            {
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
            }
            else
            {
                _logger.LogInformation($"Configuration specifies to skip EF migrations!");
            }

            if (_webHostConfig.AllowConfigFileMigrations)
            {
                var factory = _webHost.Services.GetService<IServiceScopeFactory>();
                using (var scope = factory.CreateScope())
                {
                    // migrate the Binner file configuration file to the database
                    var configFileMigrator = scope.ServiceProvider.GetRequiredService<ConfigFileMigrator>();
                    var isMigrated = await configFileMigrator.MigrateConfigFileToDatabaseAsync();
                    if (isMigrated)
                    {
                        _nlogLogger.Info("Binner configuration migrated to database!");
                    }
                    else
                    {
                        _nlogLogger.Info("Binner configuration already migrated, skipping.");
                    }
                }
            }
            else
            {
                _logger.LogInformation($"Configuration specifies to skip config file migrations!");
            }

            await _webHost.RunAsync(_cancellationTokenSource.Token);

            // stop the service
            _logger.LogInformation($"WebHost stopped!");
            // because of the way storage providers are initialized using RegisterInstance(), we must dispose of it manually
            _webHost.Services.GetService<Binner.Model.Common.IStorageProvider>()
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
