namespace Binner.Model.Configuration
{
    public static class EnvironmentVarConstants
    {
        public const string Config = "BINNER_CONFIG";                                  // name of application config file, typically appsettings.json
        public const string NlogConfig = "BINNER_NLOGCONFIG";                          // name of NLog config file, typically nlog.config
        public const string Environment = "BINNER_ENVIRONMENT";                        // Environment variable for the application environment (Development, Production)
        public const string Ip = "BINNER_IP";                                          // IP address to bind to, default "*"
        public const string PublicUrl = "BINNER_PUBLICURL";                            // Public URL for the server, used for Digikey API features and serving content
        public const string Port = "BINNER_PORT";                                      // Port to bind to, default 8090
        public const string UseHttps = "BINNER_USEHTTPS";                              // Use HTTPS for the server, default true
        public const string SslCertificate = "BINNER_SSLCERTIFICATE";                  //  Path to the SSL certificate file
        public const string SslCertificatePassword = "BINNER_SSLCERTIFICATEPASSWORD";  // Password for the SSL certificate file
        public const string ResourceSource = "BINNER_RESOURCESOURCE";                  // Hostname of the CDN used to serve static resources (datasheets, images)
        public const string Language = "BINNER_LANGUAGE";                              // Language for the application, default "en-US"
        public const string Currency = "BINNER_CURRENCY";                              // Currency for the application, default "USD"
        public const string LicenseKey = "BINNER_LICENSEKEY";                          // License key for the application, used for licensing features

        public const string StorageProviderName = "BINNER_DB_PROVIDER";                // Name of the storage provider
        public const string StorageProviderFilename = "BINNER_DB_FILENAME";            // Filename for the Sqlite storage provider
        public const string StorageProviderHost = "BINNER_DB_HOST";                    // Hostname for the storage provider
        public const string StorageProviderPort = "BINNER_DB_PORT";                    // Port for the storage provider
        public const string StorageProviderUsername = "BINNER_DB_USERNAME";            // Username for the storage provider
        public const string StorageProviderPassword = "BINNER_DB_PASSWORD";            // Password for the storage provider
        public const string StorageProviderSslMode = "BINNER_DB_SSLMODE";              // SSL mode for the Postgresql storage provider
        public const string StorageProviderAdditionalParameters = "BINNER_DB_ADDITIONALPARAMETERS"; // Additional parameters for the Postgresql storage provider
        public const string StorageProviderConnectionString = "BINNER_DB_CONNECTIONSTRING";         // Connection string for the storage provider

        /// <summary>
        /// Get an environment variable. If it doesn't exist the specified default value will be returned
        /// </summary>
        /// <param name="envName">Name of environment variable</param>
        /// <param name="defaultValue">Default value to return if environment variable does not exist</param>
        /// <returns></returns>
        public static string GetEnvOrDefault(string envName, string defaultValue)
        {
            if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(envName)))
                return System.Environment.GetEnvironmentVariable(envName) ?? defaultValue;
            return defaultValue;
        }

        /// <summary>
        /// Inject configuration from environment variables, if exists
        /// </summary>
        /// <param name="storageProviderConfig"></param>
        public static void SetConfigurationFromEnvironment(StorageProviderConfiguration storageProviderConfig)
        {
            var dbFilename = System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.StorageProviderFilename);
            if (!string.IsNullOrEmpty(dbFilename))
                storageProviderConfig.ProviderConfiguration[EnvironmentVarConstants.StorageProviderFilename] = dbFilename;

            var dbHost = System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.StorageProviderHost);
            if (!string.IsNullOrEmpty(dbHost))
                storageProviderConfig.ProviderConfiguration[EnvironmentVarConstants.StorageProviderHost] = dbHost;
            var dbPort = System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.StorageProviderPort);
            if (!string.IsNullOrEmpty(dbPort))
                storageProviderConfig.ProviderConfiguration[EnvironmentVarConstants.StorageProviderPort] = dbPort;
            var dbUsername = System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.StorageProviderUsername);
            if (!string.IsNullOrEmpty(dbUsername))
                storageProviderConfig.ProviderConfiguration[EnvironmentVarConstants.StorageProviderUsername] = dbUsername;
            var dbPassword = System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.StorageProviderPassword);
            if (!string.IsNullOrEmpty(dbPassword))
                storageProviderConfig.ProviderConfiguration[EnvironmentVarConstants.StorageProviderPassword] = dbPassword;
            var dbSslMode = System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.StorageProviderSslMode);
            if (!string.IsNullOrEmpty(dbSslMode))
                storageProviderConfig.ProviderConfiguration[EnvironmentVarConstants.StorageProviderSslMode] = dbSslMode;
            var dbAdditionalParameters = System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.StorageProviderAdditionalParameters);
            if (!string.IsNullOrEmpty(dbAdditionalParameters))
                storageProviderConfig.ProviderConfiguration[EnvironmentVarConstants.StorageProviderAdditionalParameters] = dbAdditionalParameters;
            var dbConnectionString = System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.StorageProviderConnectionString);
            if (!string.IsNullOrEmpty(dbConnectionString))
                storageProviderConfig.ProviderConfiguration[EnvironmentVarConstants.StorageProviderConnectionString] = dbConnectionString;
        }
    }
}
