using Binner.Common.Extensions;
using LightInject;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Topshelf;
using Topshelf.Runtime;

namespace Binner.Web.ServiceHost
{
    /// <summary>
    /// Provides instance of a TopShelf Host
    /// </summary>
    public class ServiceHostProvider
    {
        private const int TimeoutSeconds = 15;

        /// <summary>
        /// Configure a TopShelf Host
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static Host ConfigureService<T>(IServiceContainer container, IServiceProvider provider)
            where T : class, ServiceControl
        {
            var logger = provider.GetService<ILogger>();
            var type = typeof(T);
            var displayName = type.GetDisplayName();
            var serviceName = displayName.Replace(" ", "");
            var serviceDescription = typeof(T).GetDescription();
            var timeout = TimeSpan.FromSeconds(TimeoutSeconds);
            return HostFactory.New(x => {
                x.Service<T>((sc) => {
                    container.RegisterConstructorDependency<HostSettings>((factory, info) => sc);
                    return provider.GetRequiredService<T>();
                });

                //x.RunAsLocalSystem();
                x.SetServiceName(serviceName);
                x.SetDescription(serviceDescription);
                x.SetDisplayName(displayName);

                // x.SetStartTimeout(timeout);
                // x.SetStopTimeout(timeout);
                // x.EnableShutdown();
                //x.StartAutomatically();
                x.BeforeInstall(() => {
                    logger.LogInformation($"Installing service {serviceName}...");
                });
                x.BeforeUninstall(() => {
                    logger.LogInformation($"Uninstalling service {serviceName}...");
                });
                x.AfterInstall(() => {
                    logger.LogInformation($"{serviceName} service installed.");
                });
                x.AfterUninstall(() => {
                    logger.LogInformation($"{serviceName} service uninstalled.");
                });
                x.OnException((ex) => {
                    logger.LogError(ex, $"{serviceName} exception thrown: {ex.Message}");
                });
                /* x.EnableServiceRecovery(rc => {
                    rc.OnCrashOnly();
                    rc.RestartService(TimeSpan.FromSeconds(30));
                    // rc.RestartComputer(TimeSpan.FromMinutes(5), "Restarting system due to service failure");
                    rc.SetResetPeriod(1);
                }); */
                x.UnhandledExceptionPolicy = Topshelf.Runtime.UnhandledExceptionPolicyCode.LogErrorAndStopService;
            });
        }
    }
}
