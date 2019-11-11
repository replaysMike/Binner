using Binner.Web.Configuration;
using Binner.Web.ServiceHost;
using LightInject;
using LightInject.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;

namespace Binner.Web
{
    public class Program
    {
        private const string LogManagerConfigFile = "nlog.config"; // TODO: Inject from appsettings
        private static string _logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogManagerConfigFile);
        private static NLog.Logger _logger;

        static void Main(string[] args)
        {
            if (File.Exists(_logFile))
                _logger = NLog.Web.NLogBuilder.ConfigureNLog(_logFile).GetCurrentClassLogger();
            try
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                _logger.Info($"Binner Version {version.ToString()}");
                using (var container = new ServiceContainer(new ContainerOptions { EnablePropertyInjection = false }))
                {
                    var provider = ConfigureServices(container);
                    // run the service
                    ServiceHostProvider
                        .ConfigureService<BinnerWebHostService>(container, provider)
                        .Run();
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Main exception");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        /// <summary>
        /// Creating a service provider is required purely for use by the KMSHostService.
        /// Any web services it launches will be responsible for initializing their own provider
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        static IServiceProvider ConfigureServices(IServiceContainer container)
        {
            var services = new ServiceCollection();
            StartupConfiguration.Configure(container, services);
            StartupConfiguration.ConfigureIoC(container, services);
            StartupConfiguration.ConfigureLogging(container, services);
            var provider = container.CreateServiceProvider(services);
            container.RegisterInstance(provider);
            StartupConfiguration.LoadNLogConfiguration(container, provider, LogManagerConfigFile);

            container.ScopeManagerProvider = new PerLogicalCallContextScopeManagerProvider();
            container.BeginScope();

            return provider;
        }
    }
}
