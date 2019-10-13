using LightInject;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.IO;

namespace Binner.Web.Configuration
{
    public partial class StartupConfiguration
    {
        public static void ConfigureLogging(IServiceContainer container, IServiceCollection services)
        {
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddSingleton(typeof(ILogger), typeof(Logger<object>));
            services.AddLogging((builder) => builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace));
            container.Register<ILoggerFactory, LoggerFactory>(new PerContainerLifetime());
        }

        public static void LoadNLogConfiguration(IServiceContainer container, IServiceProvider provider, string logConfigurationFile)
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            // post-configure NLog Instance from the service provider
            var options = new NLogProviderOptions
            {
                CaptureMessageTemplates = true,
                CaptureMessageProperties = true,
            };
            loggerFactory.AddNLog(options);
            if (File.Exists(logConfigurationFile))
                NLog.LogManager.LoadConfiguration(logConfigurationFile);

            container.RegisterInstance<ILoggerFactory>(loggerFactory);
            container.Register(typeof(ILogger<>), typeof(Logger<>), new PerContainerLifetime());
            container.RegisterConstructorDependency((factory, info) => loggerFactory.CreateLogger(info.Member.DeclaringType));
        }
    }
}
