﻿using Binner.Common;
using Binner.Common.Configuration;
using Binner.Common.Extensions;
using Binner.Common.Security;
using Binner.Common.Services;
using Binner.Common.StorageProviders;
using Binner.Data;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Web.Configuration;
using Binner.Web.WebHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Targets;
using NLog.Web;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
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
        private static readonly string _configFile = EnvironmentVarConstants.GetEnvOrDefault(EnvironmentVarConstants.Config, AppConstants.AppSettings);
        private static readonly string _logManagerConfigFile = EnvironmentVarConstants.GetEnvOrDefault(EnvironmentVarConstants.NlogConfig, AppConstants.NLogConfig);
        private static readonly string _logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _logManagerConfigFile);
        private static readonly Logger _nlogLogger = NLog.Web.NLogBuilder.ConfigureNLog(_logFile).GetCurrentClassLogger();

        private bool _isDisposed;
        private ILogger<BinnerWebHostService>? _logger;
        private WebHostServiceConfiguration? _webHostConfig;
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
            var certificate = LoadOrGenerateSelfSignedCertificate(_webHostConfig);

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
            _nlogLogger.Info($"  Userfiles: {Path.GetFullPath(storageConfig.UserUploadedFilesPath ?? string.Empty)}");
            _nlogLogger.Info($"  Certificates: {Path.GetDirectoryName(Path.GetFullPath(_webHostConfig.SslCertificate ?? string.Empty))}");
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
            _webHost.Services.GetRequiredService<Binner.Model.Common.IStorageProvider>()
                ?.Dispose();
        }

        private static X509Certificate2? LoadOrGenerateSelfSignedCertificate(WebHostServiceConfiguration webHostConfig)
        {
            X509Certificate2? certificate = null;
            var certFilename = !string.IsNullOrEmpty(webHostConfig.SslCertificate) ? Path.GetFullPath(webHostConfig.SslCertificate) : string.Empty;
            if (webHostConfig.UseHttps)
            {
                // if the certificate file exists, try to load it
                if (File.Exists(certFilename))
                {
                    try
                    {
                        _nlogLogger.Info($"Loading Certificate from '{certFilename}'...");
                        var result = CertificateLoader.LoadCertificate(certFilename, webHostConfig.SslCertificatePassword);
                        certificate = result.Certificate;
                        if (certificate != null)
                        {
                            _nlogLogger.Info($"{result.CertType} Certificate loaded from '{certFilename}'");
                            _nlogLogger.Info($"Using SSL Certificate: '{certificate.Subject}' '{certificate.FriendlyName}'");
                        }
                    }
                    catch (Exception ex)
                    {
                        _nlogLogger.Error(ex, $"Failed to load SSL certificate at '{certFilename}'. Is the password correct?");
                        throw;
                    }
                }
                else
                {
                    // generate a new certificate
                    var result = GenerateSelfSignedCertificate(webHostConfig);
                    certificate = result.Certificate;
                }
            }
            return certificate;
        }

        [Flags]
        public enum CertificateState
        {
            None,
            Created,
            Registered,
            Error
        }

        public static (X509Certificate2? Certificate, CertificateState Status, string Error) GenerateSelfSignedCertificate(WebHostServiceConfiguration webHostConfig, bool throwExceptions = false)
        {
            X509Certificate2? certificate = null;
            var status = CertificateState.None;
            // if forceHttps is enabled, but no certificate is found, generate a certificate
            _nlogLogger.Info("ForceHttps is enabled, no certificate specified so a self-signed certificate will be generated.");
            try
            {
                var selfSignedCertificate = CertificateGenerator.GenerateSelfSignedCertificate("localhost", webHostConfig.SslCertificatePassword);
                certificate = selfSignedCertificate.PfxCertificate;
                var certificateFilename = !string.IsNullOrEmpty(webHostConfig.SslCertificate) ? webHostConfig.SslCertificate : "./Certificates/localhost.pfx";
                var crtFilename = certificateFilename.Replace(".pfx", ".crt", StringComparison.InvariantCultureIgnoreCase);

                if (File.Exists(crtFilename))
                {
                    var message = $"Failed to generate certificate '{crtFilename}'. File already exists, refusing to overwrite.";
                    _nlogLogger.Error(message);
                    if (throwExceptions) throw new Exception(message);
                    return (null, CertificateState.Error, message);
                }

                if (File.Exists(certificateFilename))
                {
                    var message = $"Failed to generate certificate '{certificateFilename}'. File already exists, refusing to overwrite.";
                    _nlogLogger.Error(message);
                    if (throwExceptions) throw new Exception(message);
                    return (null, CertificateState.Error, message);
                }

                // save the crt
                File.AppendAllBytes(crtFilename, selfSignedCertificate.CrtByteArray);
                _nlogLogger.Info($"New self-signed certificate saved to '{crtFilename}'");
                // save the pfx
                File.AppendAllBytes(certificateFilename, selfSignedCertificate.PfxByteArray);
                _nlogLogger.Info($"New self-signed certificate saved to '{certificateFilename}'");

                status |= CertificateState.Created;

                // update the config with the new certificate path if it's not set
                if (string.IsNullOrEmpty(webHostConfig.SslCertificate))
                {
                    webHostConfig.SslCertificate = certificateFilename;
                    var settingsService = new SettingsService();
                    settingsService.SaveSettingsAs(webHostConfig, nameof(WebHostServiceConfiguration), _configFile, true);
                }

                // attempt to register the certificate in the store for the given platform
                var storeResult = CertificateGenerator.AddCertificateToStore(_nlogLogger, certificateFilename, webHostConfig.SslCertificatePassword);
                if (storeResult.Success)
                {
                    status |= CertificateState.Registered;
                    _nlogLogger.Info($"Registered certificate '{certificateFilename}' successfully!");
                }
                else
                {
                    var message = $"Failed to registered certificate '{certificateFilename}'. You will need to register the certificate manually on your platform. Error: {storeResult.Error}";
                    _nlogLogger.Warn(message);
                }
            }
            catch (Exception ex)
            {
                status |= CertificateState.Error;
                var message = "Failed to generate a self-signed certificate.";
                _nlogLogger.Error(ex, message);
                if (throwExceptions) throw new Exception(message, ex);
            }
            return (certificate, status, string.Empty);
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
