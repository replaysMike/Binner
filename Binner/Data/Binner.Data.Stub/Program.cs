// See https://aka.ms/new-console-template for more information

using Binner.Data;
using Binner.Data.Generators;
using Binner.Model;
using Binner.Model.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Console.WriteLine("Binner Data Stub!");
/**
 * This application is required for creating EF Migrations to properly resolve the correct provider
 */

var host = Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        var provider = string.Empty;
        var providerIndex = Array.FindIndex(args, x => x.Equals("--provider"));
        if (providerIndex != -1 && providerIndex + 1 < args.Length)
        {
            provider = args[providerIndex + 1];
        }

        webBuilder.ConfigureServices(services =>
        {
            services.AddLogging(config =>
            {
                config.ClearProviders();
            });
            var configFile = EnvironmentVarConstants.GetEnvOrDefault(EnvironmentVarConstants.Config, AppConstants.AppSettings);
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(configFile)
                .Build();
            var storageProviderConfiguration = configuration.GetSection(nameof(StorageProviderConfiguration))
                .Get<StorageProviderConfiguration>() ?? throw new Exception($"Error: Could not load {nameof(StorageProviderConfiguration)} in file '{configFile}'!");
            // inject configuration from environment variables (if set)
            EnvironmentVarConstants.SetConfigurationFromEnvironment(storageProviderConfiguration);

            services.AddDbContext<BinnerContext>(optionsBuilder =>
            {
                StorageProviders switchOnProviderName;
                if (!string.IsNullOrEmpty(provider))
                {
                    switchOnProviderName = Enum.Parse<StorageProviders>(provider, true);
                    //Console.WriteLine($"Using provider passed from arguments: {provider}");
                }
                else
                {
                    switchOnProviderName = storageProviderConfiguration.StorageProvider;
                    //Console.WriteLine($"Using provider from configuration: {storageProviderConfiguration.Provider}");
                }

                switch (switchOnProviderName)
                {
                    case StorageProviders.Binner:
                        {
                            // treated as Sqlite, but inserting the filename into the connection string
                            var filename = storageProviderConfiguration.ProviderConfiguration
                                .Where(x => x.Key == "Filename").Select(x => x.Value).FirstOrDefault();
                            var connectionString = storageProviderConfiguration.ProviderConfiguration
                                .Where(x => x.Key == "SqliteConnectionString").Select(x => x.Value).FirstOrDefault();
                            //Console.WriteLine($"Using connectionString: {connectionString}");
                            optionsBuilder.UseSqlite(EnsureSqliteConnectionString(connectionString, filename), x => x.MigrationsAssembly("Binner.Data.Migrations.Sqlite"));
                            optionsBuilder.ReplaceService<IMigrationsSqlGenerator, SqliteCustomMigrationsSqlGenerator>();
                        }
                        break;
                    case StorageProviders.Sqlite:
                        {
                            var filename = storageProviderConfiguration.ProviderConfiguration
                                .Where(x => x.Key == "Filename").Select(x => x.Value).FirstOrDefault();
                            var connectionString = storageProviderConfiguration.ProviderConfiguration
                                .Where(x => x.Key == "SqliteConnectionString").Select(x => x.Value).FirstOrDefault();
                            //Console.WriteLine($"Using connectionString: {connectionString}");
                            optionsBuilder.UseSqlite(EnsureSqliteConnectionString(connectionString, filename), x => x.MigrationsAssembly("Binner.Data.Migrations.Sqlite"));
                            optionsBuilder.ReplaceService<IMigrationsSqlGenerator, SqliteCustomMigrationsSqlGenerator>();
                        }
                        break;
                    case StorageProviders.SqlServer:
                        {
                            var connectionString = storageProviderConfiguration.ProviderConfiguration
                                .Where(x => x.Key == "SqlServerConnectionString").Select(x => x.Value).FirstOrDefault();
                            //Console.WriteLine($"Using connectionString: {connectionString}");
                            optionsBuilder.UseSqlServer(connectionString, x => x.MigrationsAssembly("Binner.Data.Migrations.SqlServer"));
                        }
                        break;
                    case StorageProviders.Postgresql:
                        {
                            var connectionString = storageProviderConfiguration.ProviderConfiguration
                                .Where(x => x.Key == "PostgresqlConnectionString").Select(x => x.Value).FirstOrDefault();
                            //Console.WriteLine($"Using connectionString: {connectionString}");

                            optionsBuilder.UseNpgsql(connectionString, x =>
                            {
                                x.MigrationsAssembly("Binner.Data.Migrations.Postgresql");
                                x.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
                            });
                            optionsBuilder.ReplaceService<IMigrationsSqlGenerator, PostgresqlCustomMigrationsSqlGenerator>();
                        }
                        break;
                    case StorageProviders.MySql:
                        {
                            var connectionString = storageProviderConfiguration.ProviderConfiguration
                                .Where(x => x.Key == "MySqlConnectionString").Select(x => x.Value).FirstOrDefault();
                            //Console.WriteLine($"Using connectionString: {connectionString}");

                            var serverVersion = ServerVersion.AutoDetect(connectionString);
                            optionsBuilder.UseMySql(connectionString, serverVersion, x =>
                            {
                                x.MigrationsAssembly("Binner.Data.Migrations.MySql");
                                x.SchemaBehavior(Pomelo.EntityFrameworkCore.MySql.Infrastructure.MySqlSchemaBehavior.Ignore);
                            });
                            optionsBuilder.ReplaceService<IMigrationsSqlGenerator, MySqlCustomMigrationsSqlGenerator>();
                        }
                        break;
                    default:
                        throw new NotSupportedException($"Unsupported database provider '{storageProviderConfiguration.Provider}'");
                }

                string EnsureSqliteConnectionString(string? connectionString, string? filename)
                {
                    if (!string.IsNullOrEmpty(connectionString))
                        return connectionString;
                    return $"Data Source={filename}";
                }
            });
        });
    });

await host.RunConsoleAsync();
