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
            services.AddLogging((builder) => {
                builder.ClearProviders();
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                builder.AddNLog();
            });
            container.Register<ILoggerFactory, LoggerFactory>(new PerContainerLifetime());
        }
    }
}
