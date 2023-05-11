using Binner.Data;
using Binner.Data.Generators;
using Binner.Model.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using Npgsql;
using System;
using System.Linq;
using Binner.Model;

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

        private static string ConstructConnectionString(StorageProviderConfiguration configuration)
        {
            var connectionString = configuration.ProviderConfiguration
                .Where(x => x.Key == "ConnectionString")
                .Select(x => x.Value)
                .FirstOrDefault();
            var host = configuration.ProviderConfiguration
                .Where(x => x.Key == "Host")
                .Select(x => x.Value)
                .FirstOrDefault();
            var port = configuration.ProviderConfiguration
                .Where(x => x.Key == "Port")
                .Select(x => x.Value)
                .FirstOrDefault();
            var database = configuration.ProviderConfiguration
                .Where(x => x.Key == "Database")
                .Select(x => x.Value)
                .FirstOrDefault();
            var username = configuration.ProviderConfiguration
                .Where(x => x.Key == "Username")
                .Select(x => x.Value)
                .FirstOrDefault();
            var password = configuration.ProviderConfiguration
                .Where(x => x.Key == "Password")
                .Select(x => x.Value)
                .FirstOrDefault();
            var additionalParameters = configuration.ProviderConfiguration
                .Where(x => x.Key == "AdditionalParameters")
                .Select(x => x.Value)
                .FirstOrDefault();

            // set a default if not specified
            if (string.IsNullOrEmpty(connectionString) && string.IsNullOrEmpty(database))
                database = "Binner";

            switch (configuration.StorageProvider)
            {
                case StorageProviders.Binner:
                case StorageProviders.Sqlite:
                {
                    var filename = configuration.ProviderConfiguration
                        .Where(x => x.Key == "Filename")
                        .Select(x => x.Value)
                        .FirstOrDefault();
                    return EnsureSqliteConnectionString(connectionString, filename);
                }
                case StorageProviders.SqlServer:
                {
                    var builder = new SqlConnectionStringBuilder(connectionString);
                    if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(port))
                        builder.DataSource = $"{host},{port}";
                    else if (!string.IsNullOrEmpty(host))
                        builder.DataSource = $"{host}";
                    if (!string.IsNullOrEmpty(database))
                        builder.InitialCatalog = database;
                    if (!string.IsNullOrEmpty(username))
                        builder.UserID = username;
                    else
                        builder.IntegratedSecurity = true;

                    if (!string.IsNullOrEmpty(password))
                        builder.Password = password;

                    builder.TrustServerCertificate = true;

                    if (!string.IsNullOrEmpty(additionalParameters))
                        return $"{builder};{additionalParameters}";
                    return $"{builder}";
                }
                case StorageProviders.Postgresql:
                {
                    var builder = new NpgsqlConnectionStringBuilder(connectionString);
                    if (!string.IsNullOrEmpty(host))
                        builder.Host = host;
                    if (!string.IsNullOrEmpty(port))
                    {
                        if (int.TryParse(port, out var portNumber))
                            builder.Port = portNumber;
                    }
                    if (!string.IsNullOrEmpty(database))
                        builder.Database = database;
                    if (!string.IsNullOrEmpty(username))
                        builder.Username = username;
                    if (!string.IsNullOrEmpty(password))
                        builder.Password = password;
                    builder.PersistSecurityInfo = true;
                    builder.SslMode = SslMode.Disable;
                    if (!string.IsNullOrEmpty(additionalParameters))
                        return $"{builder};{additionalParameters}";
                    return $"{builder}";
                }
                case StorageProviders.MySql:
                {
                    var builder = new MySqlConnectionStringBuilder(connectionString ?? string.Empty);
                    if (!string.IsNullOrEmpty(host))
                        builder.Server = host;
                    if (!string.IsNullOrEmpty(port))
                    {
                        if (uint.TryParse(port, out var portNumber))
                            builder.Port = portNumber;
                    }
                    if (!string.IsNullOrEmpty(database))
                        builder.Database = database;
                    if (!string.IsNullOrEmpty(username))
                        builder.UserID = username;
                    if (!string.IsNullOrEmpty(password))
                        builder.Password = password;
                    if (!string.IsNullOrEmpty(additionalParameters))
                        return $"{builder};{additionalParameters}";
                    return $"{builder}";
                }
                default:
                    throw new NotSupportedException($"Unsupported provider: {configuration.Provider}");
            }
        }

        public static IServiceCollection RegisterDbContext(IServiceCollection services, StorageProviderConfiguration configuration)
        {
            var connectionString = ConstructConnectionString(configuration);

            services.AddDbContext<BinnerContext>(options => _ = configuration.StorageProvider switch
            {
                // binner and sqlite are now the same
                StorageProviders.Binner => options.UseSqlite(connectionString, x =>
                {
                    x.MigrationsAssembly(SqlLiteMigrationsAssemblyName);
                }).ReplaceService<IMigrationsSqlGenerator, SqliteCustomMigrationsSqlGenerator>(),

                StorageProviders.Sqlite => options.UseSqlite(connectionString, x =>
                {
                    x.MigrationsAssembly(SqlLiteMigrationsAssemblyName);
                }).ReplaceService<IMigrationsSqlGenerator, SqliteCustomMigrationsSqlGenerator>(),

                // alternative storage provider registrations
                StorageProviders.SqlServer => options.UseSqlServer(connectionString, x => x.MigrationsAssembly(SqlServerMigrationsAssemblyName)),

                StorageProviders.Postgresql => options.UseNpgsql(connectionString, x =>
                {
                    x.MigrationsAssembly(PostgresqlMigrationsAssemblyName);
                    x.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
                }).ReplaceService<IMigrationsSqlGenerator, PostgresqlCustomMigrationsSqlGenerator>(),

                StorageProviders.MySql => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), x =>
                {
                    x.MigrationsAssembly(MySqlMigrationsAssemblyName);
                    x.SchemaBehavior(Pomelo.EntityFrameworkCore.MySql.Infrastructure.MySqlSchemaBehavior.Ignore);
                }).ReplaceService<IMigrationsSqlGenerator, MySqlCustomMigrationsSqlGenerator>(),
                _ => throw new NotSupportedException($"Unsupported provider: {configuration.Provider}")
            });
            services.AddDbContextFactory<BinnerContext>(lifetime: ServiceLifetime.Scoped);
            return services;
        }

        private static string EnsureSqliteConnectionString(string? connectionString, string? filename)
        {
            if (!string.IsNullOrEmpty(connectionString))
                return connectionString;
            return $"Data Source={filename}";
        }
    }
}
