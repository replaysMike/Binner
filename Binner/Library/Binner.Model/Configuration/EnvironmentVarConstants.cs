namespace Binner.Model.Configuration
{
    public static class EnvironmentVarConstants
    {
        public static string Environment = "BINNER_ENVIRONMENT";
        public static string Ip = "BINNER_IP";
        public static string PublicUrl = "BINNER_PUBLICURL";
        public static string Port = "BINNER_PORT";
        public static string SslCertificate = "BINNER_SSLCERTIFICATE";
        public static string SslCertificatePassword = "BINNER_SSLCERTIFICATEPASSWORD";
        public static string ResourceSource = "BINNER_RESOURCESOURCE";
        public static string Language = "BINNER_LANGUAGE";
        public static string Currency = "BINNER_CURRENCY";
        public static string LicenseKey = "BINNER_LICENSEKEY";

        public static string StorageProviderName = "BINNER_DB_PROVIDER";
        public static string StorageProviderFilename = "BINNER_DB_FILENAME";
        public static string StorageProviderHost = "BINNER_DB_HOST";
        public static string StorageProviderPort = "BINNER_DB_PORT";
        public static string StorageProviderUsername = "BINNER_DB_USERNAME";
        public static string StorageProviderPassword = "BINNER_DB_PASSWORD";
        public static string StorageProviderSslMode = "BINNER_DB_SSLMODE";
        public static string StorageProviderAdditionalParameters = "BINNER_DB_ADDITIONALPARAMETERS";
        public static string StorageProviderConnectionString = "BINNER_DB_CONNECTIONSTRING";

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
