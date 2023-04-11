using Binner.Common.Configuration;
using Binner.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace Binner.Web.Configuration
{
    public static class HostBuilderFactory
    {
        public const string SqlLiteMigrationsAssemblyName = "Binner.Data.Migrations.Sqlite";
        public const string SqlServerMigrationsAssemblyName = "Binner.Data.Migrations.SqlServer";
        public const string PostgresqlMigrationsAssemblyName = "Binner.Data.Migrations.Postgresql";
        public const string MySqlMigrationsAssemblyName = "Binner.Data.Migrations.MySql";

        public static IHostBuilder Create(StorageProviderConfiguration configuration)
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    RegisterDbContext(services, configuration);
                });
        }

        public static IServiceCollection RegisterDbContext(IServiceCollection services, StorageProviderConfiguration configuration)
        {
            var connectionString = configuration.ProviderConfiguration
                .Where(x => x.Key == "ConnectionString")
                .Select(x => x.Value)
                .FirstOrDefault();

            services.AddDbContext<BinnerContext>(options => _ = configuration.Provider.ToLower() switch
            {
                // binner and sqlite are now the same
                "binner" => options.UseSqlite(connectionString, x => x.MigrationsAssembly(SqlLiteMigrationsAssemblyName)),
                "sqlite" => options.UseSqlite(connectionString, x => x.MigrationsAssembly(SqlLiteMigrationsAssemblyName)),

                // alternative storage provider registrations
                "sqlserver" => options.UseSqlServer(connectionString, x => x.MigrationsAssembly(SqlServerMigrationsAssemblyName)),
                "postgresql" => options.UseNpgsql(connectionString, x => x.MigrationsAssembly(PostgresqlMigrationsAssemblyName)),
                "mysql" => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), x => x.MigrationsAssembly(MySqlMigrationsAssemblyName)),
                _ => throw new NotSupportedException($"Unsupported provider: {configuration.Provider}")
            });
            services.AddDbContextFactory<BinnerContext>(lifetime: ServiceLifetime.Scoped);
            return services;
        }
    }
}
