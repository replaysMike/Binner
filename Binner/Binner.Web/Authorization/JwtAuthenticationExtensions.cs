using Binner.Common.Services;
using Binner.Common.Services.Authentication;
using Binner.Model.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Binner.Web.Authorization
{
    public static class JwtAuthenticationExtensions
    {
        /// <summary>
        /// Adds Jwt Bearer token Authentication
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtService = Configure();

            // configure authentication for Jwt bearer tokens
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                // determine how the token validation should be performed
                options.TokenValidationParameters = jwtService.GetTokenValidationParameters();
            });

            JwtService Configure() {
                var authConfig = new WebHostServiceConfiguration();
                configuration.Bind("WebHostServiceConfiguration", authConfig);
                services.AddSingleton(authConfig);
                return new JwtService(authConfig, new SettingsService());
            }
        }
    }
}
