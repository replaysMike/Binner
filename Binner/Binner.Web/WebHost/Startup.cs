using Binner.Web.Configuration;
using LightInject;
using LightInject.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Binner.Web.WebHost
{
    public class Startup
    {
        /// <summary>
        /// Web configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Service container for web application
        /// </summary>
        public IServiceContainer Container { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            var containerOptions = new ContainerOptions
            {
                EnablePropertyInjection = false,
                DefaultServiceSelector = s => s.Last()
            };
            Container = new ServiceContainer(containerOptions);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // needed to load IP Rate limit configuration from appsettings.json
            services.AddOptions();

            var configuration = StartupConfiguration.Configure(Container, services);

            // needed to store IP rate limit counters and ip rules, as well as in-memory API caching
            services.AddMemoryCache();

            services.AddControllers();

            StartupConfiguration.ConfigureIoC(Container, services);
            StartupConfiguration.ConfigureLogging(Container, services);
            var provider = Container.CreateServiceProvider(services);

            Container.ScopeManagerProvider = new PerLogicalCallContextScopeManagerProvider();
            Container.BeginScope();

            return provider;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var config = Configuration.GetSection(nameof(WebHostServiceConfiguration)).Get<WebHostServiceConfiguration>();
            Console.WriteLine($"ENVIRONMENT NAME: {env.EnvironmentName}");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }

        /// <summary>
        /// Convert a hex formatted string to bytes
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        private static byte[] ConvertHexToBytes(string hex)
        {
            var NumberChars = hex.Length;
            var bytes = new byte[NumberChars / 2];
            for (var i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}
