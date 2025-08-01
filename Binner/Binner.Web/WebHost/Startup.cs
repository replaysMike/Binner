﻿using Binner.Data;
using Binner.Global.Common;
using Binner.Model.Authentication;
using Binner.Model.Configuration;
using Binner.Web.Authorization;
using Binner.Web.Configuration;
using Binner.Web.Middleware;
using Binner.Web.ServiceHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Binner.Web.WebHost
{
    public class Startup
    {
        private readonly string _uiFolder = Path.Combine("ClientApp", "build");

        /// <summary>
        /// Web configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            // ensure UI build folder exists
            Directory.CreateDirectory(_uiFolder);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            var configuration = StartupConfiguration.Configure(services);

            services.AddControllersWithViews(config =>
            {
                // add support for registering api controllers with generic type arguments
                //config.Conventions.Add(new GenericControllerNameConvention());
            });
            /*.ConfigureApplicationPartManager(apm =>
            {
                apm.FeatureProviders.Add(new GenericRestControllerFeatureProvider());
            });*/

            services.AddOutputCache(options =>
            {
                options.AddBasePolicy(builder => builder.Cache());
            });

            services.Configure<FormOptions>(options =>
            {
                // limit files to 64MB
                options.MultipartBodyLengthLimit = 64 * 1024 * 1024;
            });

            // configure the location of SPA static files
            services.AddSpaStaticFiles(options =>
            {
                options.RootPath = _uiFolder;
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

            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConfiguration(configuration);
                logging.AddEventSourceLogger();
                //if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                    //logging.AddConsole();
                logging.AddNLogWeb();
            });

            StartupConfiguration.ConfigureIoC(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var config = app.ApplicationServices.GetRequiredService<WebHostServiceConfiguration>();
            if (config == null) throw new InvalidOperationException("Could not retrieve WebHostServiceConfiguration, configuration file may be invalid!");

            app.UseForwardedHeaders();
            // add build version to the response headers
            app.UseVersionHeader();

            if (env.IsDevelopment())
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
            app.UseOutputCache();

            app.UseExceptionHandler(appError =>
            {
                appError.Run(context =>
                {
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        var logger = app.ApplicationServices.GetRequiredService<ILogger<Startup>>();
                        if (contextFeature.Error is UserContextUnauthorizedException)
                        {
                            // user is not logged in, or access denied
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            logger.LogWarning(contextFeature.Error, $"User not authorized!\nEndpoint:'{contextFeature.Endpoint}'");
                            return Task.CompletedTask;
                        }
                        logger.LogError(contextFeature.Error, $"Unhandled Application Exception!\nEndpoint: '{contextFeature.Endpoint}'");
                        Console.WriteLine($"Application Error: '{contextFeature.Endpoint}'");
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

            // required to serve SPA static files
            app.UseSpaStaticFiles();
            app.UseRouting();

            // global error handler
            app.UseMiddleware<GlobalErrorMiddleware>();

            // required for running in production mode
            app.UseSpa((spa) =>
            {
                spa.Options.SourcePath = _uiFolder;
                if (env.IsProduction())
                {
                    spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), _uiFolder))
                    };
                }
            });

            // enable authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            using (var scope = app.ApplicationServices.CreateScope())
            {
                // initialize the database context
                var context = scope.ServiceProvider.GetRequiredService<BinnerContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<BinnerWebHostService>>();
                BinnerContextInitializer.Initialize(logger, context,
                    (password) => PasswordHasher
                        .GeneratePasswordHash(password)
                        .GetBase64EncodedPasswordHash());
            }
        }
    }
}
