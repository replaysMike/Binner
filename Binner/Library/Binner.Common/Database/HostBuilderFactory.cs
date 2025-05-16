using Binner.Data;
using Binner.Data.Generators;
using Binner.Model.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Binner.Common.Database
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
            var connectionString = ConnectionStringConstructor.ConstructConnectionString(configuration);

            services.AddDbContext<BinnerContext>(options => _ = configuration.StorageProvider switch
            {
                // binner and sqlite are now the same
                Model.StorageProviders.Binner => options.UseSqlite(connectionString, x =>
                {
                    x.MigrationsAssembly(SqlLiteMigrationsAssemblyName);
                }).ReplaceService<IMigrationsSqlGenerator, SqliteCustomMigrationsSqlGenerator>(),

                Model.StorageProviders.Sqlite => options.UseSqlite(connectionString, x =>
                {
                    x.MigrationsAssembly(SqlLiteMigrationsAssemblyName);
                }).ReplaceService<IMigrationsSqlGenerator, SqliteCustomMigrationsSqlGenerator>(),

                // alternative storage provider registrations
                Model.StorageProviders.SqlServer => options.UseSqlServer(connectionString, x => x.MigrationsAssembly(SqlServerMigrationsAssemblyName)),

                Model.StorageProviders.Postgresql => options.UseNpgsql(connectionString, x =>
                {
                    x.MigrationsAssembly(PostgresqlMigrationsAssemblyName);
                    x.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
                }).ReplaceService<IMigrationsSqlGenerator, PostgresqlCustomMigrationsSqlGenerator>(),

                Model.StorageProviders.MySql => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), x =>
                {
                    x.MigrationsAssembly(MySqlMigrationsAssemblyName);
                    x.SchemaBehavior(Pomelo.EntityFrameworkCore.MySql.Infrastructure.MySqlSchemaBehavior.Ignore);
                }).ReplaceService<IMigrationsSqlGenerator, MySqlCustomMigrationsSqlGenerator>(),
                _ => throw new NotSupportedException($"Unsupported provider: {configuration.Provider}")
            });
            services.AddDbContextFactory<BinnerContext>(lifetime: ServiceLifetime.Scoped);
            return services;
        }
    }
}
