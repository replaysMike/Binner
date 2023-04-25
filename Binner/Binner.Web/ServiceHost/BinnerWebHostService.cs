using Binner.Common;
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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
using System.Linq;
using System.Linq.Expressions;
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
    public class BinnerWebHostService : IHostService, IDisposable
    {
        const string ConfigFile = "appsettings.json";
        private const string CertificatePassword = "password";
        const string LogManagerConfigFile = "nlog.config"; // TODO: Inject from appsettings
        private static readonly string _logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogManagerConfigFile);
        private static readonly Logger _nlogLogger = NLog.Web.NLogBuilder.ConfigureNLog(_logFile).GetCurrentClassLogger();

        private bool _isDisposed;
        private ILogger<BinnerWebHostService>? _logger;
        private WebHostServiceConfiguration? _config;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private IWebHost? _webHost;

        public bool Start(HostControl hostControl)
        {
            Task.Run(() => InitializeWebHostAsync()).ContinueWith(t =>
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
            _config = configuration.GetSection(nameof(WebHostServiceConfiguration)).Get<WebHostServiceConfiguration>() ?? throw new BinnerConfigurationException($"Configuration section '{nameof(WebHostServiceConfiguration)}' does not exist!");

            // parse the requested IP from the config
            var ipAddress = IPAddress.Any;
            var ipString = _config.IP;
            if (!string.IsNullOrEmpty(ipString) && ipString != "*")
                if (!IPAddress.TryParse(_config.IP, out ipAddress)) throw new BinnerConfigurationException($"Failed to parse IpAddress '{ipString}'");

            // use embedded certificate
            var certificateBytes = ResourceLoader.LoadResourceBytes(Assembly.GetExecutingAssembly(), @"Certificates.Binner.pfx");
            var certificate = new X509Certificate2(certificateBytes, CertificatePassword);

            var storageConfig = configuration.GetSection(nameof(StorageProviderConfiguration)).Get<StorageProviderConfiguration>() ?? throw new BinnerConfigurationException($"Configuration section '{nameof(StorageProviderConfiguration)}' does not exist!");
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

            _nlogLogger.Info($"Building the WebHost on {ipAddress}:{_config.Port} ...");
            var host = Microsoft.AspNetCore.WebHost
            .CreateDefaultBuilder()
            .ConfigureKestrel(options =>
            {
                options.ConfigureHttpsDefaults(opt =>
                {
                    opt.ServerCertificate = certificate;
                    opt.ClientCertificateMode = ClientCertificateMode.NoCertificate;
                    opt.CheckCertificateRevocation = false;
                    opt.AllowAnyClientCertificate();
                });
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
            ApplicationLogging.LoggerFactory = _webHost.Services.GetRequiredService<ILoggerFactory>();
            _logger = _webHost.Services.GetRequiredService<ILogger<BinnerWebHostService>>();
            _logger.LogInformation($"Using SSL Certificate: '{certificate.Subject}' '{certificate.FriendlyName}'");

            using (var context = migrationHost.Services.GetRequiredService<BinnerContext>())
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

        private async Task ApplyOrganizationIdPatchAsync(BinnerContext context)
        {
            await PatchTableAsync(context, x => x.Users);
            await PatchTableAsync(context, x => x.OAuthCredentials);
            await PatchTableAsync(context, x => x.OAuthRequests);
            await PatchTableAsync(context, x => x.Parts);
            await PatchTableAsync(context, x => x.PartSuppliers);
            await PatchTableAsync(context, x => x.PartTypes);
            await PatchTableAsync(context, x => x.Pcbs);
            await PatchTableAsync(context, x => x.PcbStoredFileAssignments);
            await PatchTableAsync(context, x => x.ProjectPartAssignments);
            await PatchTableAsync(context, x => x.ProjectPcbAssignments);
            await PatchTableAsync(context, x => x.Projects);
            await PatchTableAsync(context, x => x.StoredFiles);
            await PatchTableAsync(context, x => x.UserIntegrationConfigurations);
            await PatchTableAsync(context, x => x.UserLoginHistory);
            await PatchTableAsync(context, x => x.UserPrinterConfigurations);
            await PatchTableAsync(context, x => x.UserPrinterTemplateConfigurations);
            await PatchTableAsync(context, x => x.UserTokens);
        }

        private class ReplaceExpressionVisitor
            : ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node)
            {
                if (node == _oldValue)
                    return _newValue;
                return base.Visit(node);
            }
        }

        private async Task PatchTableAsync<T>(BinnerContext context, Func<BinnerContext, DbSet<T>> expression)
        where T : class
        {
            var propertiesToPatch = new[] { "OrganizationId", "UserId" };
            try
            {
                foreach (var propertyName in propertiesToPatch)
                {
                    var parameter = Expression.Parameter(typeof(T), "e");
                    var propExpression = Expression.Property(parameter, propertyName); // OrganizationId, UserId
                    var value = 0;
                    Expression<Func<T, bool>> filterLambda;
                    if (propExpression.Type == typeof(Int32))
                    {
                        var equalCondition = Expression.Equal(propExpression, Expression.Constant(value));
                        filterLambda = Expression.Lambda<Func<T, bool>>(equalCondition, parameter);
                    }
                    else if (Nullable.GetUnderlyingType(propExpression.Type) == typeof(Int32))
                    {
                        // create an expression that does an EqualTo value OR EqualTo null
                        var areEqual = Expression.Equal(propExpression, Expression.Convert(Expression.Constant(value), propExpression.Type));
                        var isNull = Expression.Equal(propExpression, Expression.Convert(Expression.Constant(null), propExpression.Type));

                        var expr1 = Expression.Lambda<Func<T, bool>>(areEqual, parameter);
                        var expr2 = Expression.Lambda<Func<T, bool>>(isNull, parameter);
                        var body = Expression.Or(expr1.Body, expr2.Body);
                        filterLambda = Expression.Lambda<Func<T, bool>>(body, expr1.Parameters[0]);
                    }
                    else
                    {
                        throw new Exception($"Unexpected type: {propExpression.Type}");
                    }

                    var records = await expression.Invoke(context).Where(filterLambda).ToListAsync();
                    foreach (var record in records)
                    {
                        var type = record.GetType();
                        var pi = type.GetProperty("OrganizationId");
                        pi.SetValue(record, 1);
                    }

                    var updatedCount = await context.SaveChangesAsync();
                    if (updatedCount > 0) _logger!.LogWarning($"Patched {updatedCount} {typeof(T).Name}s missing {propertyName}!");
                }
            }
            catch (Exception ex)
            {
                // log the error
                _logger!.LogError(ex, "PatchTableAsync encountered an unexpected error!");
            }
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
