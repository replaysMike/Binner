using Binner.Data;
using Binner.Model.Authentication;
using Binner.Model.Configuration;
using Binner.Web.Authorization;
using Binner.Web.Configuration;
using Binner.Web.Middleware;
using LightInject;
using LightInject.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Binner.Global.Common;
using Microsoft.Extensions.FileProviders;
using System.IO;

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
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            var configuration = StartupConfiguration.Configure(Container, services);

            services.AddControllersWithViews();

            services.Configure<FormOptions>(options =>
            {
                // limit files to 64MB
                options.MultipartBodyLengthLimit = 64 * 1024 * 1024;
            });

            // specify the path to our spa production build
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            // add custom Jwt authentication support
            services.AddJwtAuthentication(configuration);

            services.AddAuthorization(options =>
            {
                // add a default policy, the user must have the CanLogin policy to access authenticated pages
                options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireClaim(JwtClaimTypes.CanLogin, true.ToString())
                    .Build();

                // add admin policy for admin users only access
                options.AddPolicy(AuthorizationPolicies.Admin, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(JwtClaimTypes.Admin, true.ToString());
                });
                // user must be able to login to access resource (confirmed email, not locked out)
                options.AddPolicy(AuthorizationPolicies.CanLogin, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(JwtClaimTypes.CanLogin, true.ToString());
                });
            });

            StartupConfiguration.ConfigureIoC(Container, services);
            var provider = Container.CreateServiceProvider(services);

            Container.ScopeManagerProvider = new PerLogicalCallContextScopeManagerProvider();
            Container.BeginScope();
            return provider;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var config = app.ApplicationServices.GetRequiredService<WebHostServiceConfiguration>();
            if (config == null) throw new InvalidOperationException("Could not retrieve WebHostServiceConfiguration, configuration file may be invalid!");
            Console.WriteLine($"ENVIRONMENT NAME: {config.Environment}");

            app.UseForwardedHeaders();

            // add build version to the response headers
            app.UseVersionHeader();

            if (config.Environment == Environments.Development)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            if (config.UseHttps)
            {
                app.UseHttpsRedirection();
            }

            app.UseExceptionHandler(appError =>
            {
                appError.Run(context =>
                {
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null) { 
                        Console.WriteLine($"Application Error: {contextFeature.Endpoint}");
                        Console.WriteLine($"  Exception: {contextFeature.Error.GetType().Name} {contextFeature.Error.Message}");
                        if (contextFeature.Error.InnerException != null)
                            Console.WriteLine($"  Inner Exception: {contextFeature.Error.InnerException.GetType().Name} {contextFeature.Error.InnerException.Message}");
                        if (contextFeature.Error.InnerException?.InnerException != null)
                            Console.WriteLine($"  Base Exception: {contextFeature.Error.GetBaseException().GetType().Name} {contextFeature.Error.GetBaseException().Message}");
                        if (contextFeature.Error.StackTrace != null)
                            Console.WriteLine($"  Stack Trace: {contextFeature.Error.StackTrace}");
                    }
                    return Task.CompletedTask;
                });
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "ClientApp")),
                RequestPath = ""
            });
            app.UseSpaStaticFiles();
            app.UseRouting();

            // enable authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSpa((config) =>
            {
                config.Options.SourcePath = "ClientApp";
            });

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            using (var scope = app.ApplicationServices.CreateScope())
            {
                // initialize the database context
                var context = scope.ServiceProvider.GetRequiredService<BinnerContext>();
                BinnerContextInitializer.Initialize(context,
                    (password) => PasswordHasher
                        .GeneratePasswordHash(password)
                        .GetBase64EncodedPasswordHash());
            }
        }
    }
}
