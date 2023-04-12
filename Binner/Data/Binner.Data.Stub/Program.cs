// See https://aka.ms/new-console-template for more information

using Binner.Data;
using Binner.Data.Generators;
using Binner.Model.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            var storageProviderConfiguration = configuration.GetSection(nameof(StorageProviderConfiguration))
                .Get<StorageProviderConfiguration>() ?? throw new Exception($"Error: Could not load {nameof(StorageProviderConfiguration)}!");

            services.AddDbContext<BinnerContext>(optionsBuilder =>
            {
                string switchOnProviderName;
                if (!string.IsNullOrEmpty(provider))
                {
                    switchOnProviderName = provider.ToLower();
                    Console.WriteLine($"Using provider passed from arguments: {provider}");
                }
                else
                {
                    switchOnProviderName = storageProviderConfiguration.Provider.ToLower();
                    Console.WriteLine($"Using provider from configuration: {storageProviderConfiguration.Provider}");
                }

                switch (switchOnProviderName)
                {
                    case "binner":
                        {
                            // treated as Sqlite, but inserting the filename into the connection string
                            var filename = storageProviderConfiguration.ProviderConfiguration
                                .Where(x => x.Key == "Filename").Select(x => x.Value).FirstOrDefault();
                            var connectionString = $"Data Source={filename}";
                            Console.WriteLine($"Using connectionString: {connectionString}");
                            optionsBuilder.UseSqlite(connectionString, x => x.MigrationsAssembly("Binner.Data.Migrations.Sqlite"));
                            optionsBuilder.ReplaceService<IMigrationsSqlGenerator, SqliteCustomMigrationsSqlGenerator>();
                        }
                        break;
                    case "sqlite":
                        {
                            var connectionString = storageProviderConfiguration.ProviderConfiguration
                                .Where(x => x.Key == "SqliteConnectionString").Select(x => x.Value).FirstOrDefault();
                            Console.WriteLine($"Using connectionString: {connectionString}");
                            optionsBuilder.UseSqlite(connectionString, x => x.MigrationsAssembly("Binner.Data.Migrations.Sqlite"));
                            optionsBuilder.ReplaceService<IMigrationsSqlGenerator, SqliteCustomMigrationsSqlGenerator>();
                        }
                        break;
                    case "sqlserver":
                        {
                            var connectionString = storageProviderConfiguration.ProviderConfiguration
                                .Where(x => x.Key == "SqlServerConnectionString").Select(x => x.Value).FirstOrDefault();
                            Console.WriteLine($"Using connectionString: {connectionString}");
                            optionsBuilder.UseSqlServer(connectionString, x => x.MigrationsAssembly("Binner.Data.Migrations.SqlServer"));
                        }
                        break;
                    case "postgresql":
                        {
                            var connectionString = storageProviderConfiguration.ProviderConfiguration
                                .Where(x => x.Key == "PostgresqlConnectionString").Select(x => x.Value).FirstOrDefault();
                            Console.WriteLine($"Using connectionString: {connectionString}");

                            optionsBuilder.UseNpgsql(connectionString, x =>
                            {
                                x.MigrationsAssembly("Binner.Data.Migrations.Postgresql");
                                x.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
                            });
                            optionsBuilder.ReplaceService<IMigrationsSqlGenerator, PostgresqlCustomMigrationsSqlGenerator>();
                        }
                        break;
                    case "mysql":
                        {
                            var connectionString = storageProviderConfiguration.ProviderConfiguration
                                .Where(x => x.Key == "MySqlConnectionString").Select(x => x.Value).FirstOrDefault();
                            Console.WriteLine($"Using connectionString: {connectionString}");

                            var serverVersion = ServerVersion.AutoDetect(connectionString);
                            optionsBuilder.UseMySql(connectionString, serverVersion, x => x.MigrationsAssembly("Binner.Data.Migrations.MySql"));
                            optionsBuilder.ReplaceService<IMigrationsSqlGenerator, MySqlCustomMigrationsSqlGenerator>();
                        }
                        break;
                    default:
                        throw new NotSupportedException($"Unsupported database provider '{storageProviderConfiguration.Provider}'");
                }
            });
        });
    });

await host.RunConsoleAsync();
