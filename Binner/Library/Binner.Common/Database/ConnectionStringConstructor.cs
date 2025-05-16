using Binner.Model.Configuration;
using Microsoft.Data.SqlClient;
using MySqlConnector;
using Npgsql;
using System;
using System.Linq;

namespace Binner.Common.Database
{
    public static class ConnectionStringConstructor
    {
        public static string ConstructConnectionString(StorageProviderConfiguration configuration)
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
            var sslmode = configuration.ProviderConfiguration
                .Where(x => x.Key == "SslMode")
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
                case Model.StorageProviders.Binner:
                case Model.StorageProviders.Sqlite:
                    {
                        var filename = configuration.ProviderConfiguration
                            .Where(x => x.Key == "Filename")
                            .Select(x => x.Value)
                            .FirstOrDefault();
                        return EnsureSqliteConnectionString(connectionString, filename);
                    }
                case Model.StorageProviders.SqlServer:
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
                case Model.StorageProviders.Postgresql:
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
                        var sslMode = SslMode.Allow;
                        builder.SslMode = sslMode;
                        if (!string.IsNullOrEmpty(sslmode))
                        {
                            if (Enum.TryParse(sslmode, out sslMode))
                                builder.SslMode = sslMode;
                        }
                        if (!string.IsNullOrEmpty(additionalParameters))
                            return $"{builder};{additionalParameters}";
                        return $"{builder}";
                    }
                case Model.StorageProviders.MySql:
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

        private static string EnsureSqliteConnectionString(string? connectionString, string? filename)
        {
            if (!string.IsNullOrEmpty(connectionString))
                return connectionString;
            return $"Data Source={filename}";
        }
    }
}
