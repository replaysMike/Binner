using Binner.Common.Services;
using Binner.Common.StorageProviders;
using Binner.Web.ServiceHost;
using Binner.Web.WebHost;
using LightInject;
using Microsoft.Extensions.DependencyInjection;

namespace Binner.Web.Configuration
{
    public partial class StartupConfiguration
    {
        public static void ConfigureIoC(IServiceContainer container, IServiceCollection services)
        {
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1#service-lifetimes
            // Transient = created each time requested, Scoped = once per http/scope request, Singleton = first time requested only
            // allow the container to be injected
            services.AddSingleton<IServiceContainer, ServiceContainer>();
            services.AddSingleton(container);

            services.AddTransient<IWebHostFactory, WebHostFactory>();
            container.RegisterInstance(container);

            // register services
            container.Register<IPartService, PartService>(new PerContainerLifetime());
            container.RegisterSingleton<IStorageProvider>((factory) =>
            {
                return factory.Create<BinnerFileStorageProvider>();
            });

            // the main server app
            container.Register<BinnerWebHostService>(new PerContainerLifetime());

            // register the CertificateProvider for providing access to the server certificate
            var config = container.GetInstance<WebHostServiceConfiguration>();
        }
    }
}
