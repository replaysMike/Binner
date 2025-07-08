using Binner.Common.Extensions;
using Binner.Model.Configuration;
using NLog;

namespace Binner.Common.Configuration
{
    /// <summary>
    /// Validates configurations
    /// </summary>
    public class ConfigurationValidator
    {
        private readonly ILogger _logger;

        public ConfigurationValidator(ILogger logger)
        {
            _logger = logger;
        }

        public void ValidateConfiguration(WebHostServiceConfiguration config)
        {
            if (config.Port == 0)
            {
                config.Port = 8090;
                ConfigAssert("Port", config.Port, "Defaulting to 8090.");
            }
            if (config.PublicUrl.SurroundedBy("@"))
                ConfigAssert("PublicUrl", config.PublicUrl);
            if (config.Licensing.LicenseKey.SurroundedBy("@"))
                ConfigAssert("Licensing.LicenseKey", config.Licensing.LicenseKey);
        }

        public void ValidateConfiguration(IntegrationConfiguration config)
        {
            if (config.Swarm.Enabled)
            {
                if (config.Swarm.ApiKey.SurroundedBy("@"))
                    ConfigAssert("Integrations.Swarm.ApiKey", config.Swarm.ApiKey);
            }
            if (config.Digikey.Enabled)
            {
                if (string.IsNullOrEmpty(config.Digikey.ApiKey) || config.Digikey.ApiKey.SurroundedBy("@"))
                    ConfigAssert("Integrations.Digikey.ClientId", config.Digikey.ApiKey);
                if (string.IsNullOrEmpty(config.Digikey.ClientSecret) || config.Digikey.ClientSecret.SurroundedBy("@"))
                    ConfigAssert("Integrations.Digikey.ClientSecret", config.Digikey.ClientSecret);
                if (string.IsNullOrEmpty(config.Digikey.oAuthPostbackUrl) || config.Digikey.oAuthPostbackUrl.Contains("@PUBLIC_URL@"))
                    ConfigAssert("Integrations.Digikey.oAuthPostbackUrl", config.Digikey.oAuthPostbackUrl);
            }
            if (config.Mouser.Enabled)
            {
                if (!string.IsNullOrEmpty(config.Mouser.ApiKeys.SearchApiKey) && config.Mouser.ApiKeys.SearchApiKey.SurroundedBy("@"))
                    ConfigAssert("Integrations.Mouser.ApiKeys.SearchApiKey", config.Mouser.ApiKeys.SearchApiKey);
                if (!string.IsNullOrEmpty(config.Mouser.ApiKeys.OrderApiKey) && config.Mouser.ApiKeys.OrderApiKey.SurroundedBy("@"))
                    ConfigAssert("Integrations.Mouser.ApiKeys.OrderApiKey", config.Mouser.ApiKeys.OrderApiKey);
                if (!string.IsNullOrEmpty(config.Mouser.ApiKeys.CartApiKey) && config.Mouser.ApiKeys.CartApiKey.SurroundedBy("@"))
                    ConfigAssert("Integrations.Mouser.ApiKeys.CartApiKey", config.Mouser.ApiKeys.CartApiKey);

                if (string.IsNullOrEmpty(config.Mouser.ApiKeys.SearchApiKey)
                    && string.IsNullOrEmpty(config.Mouser.ApiKeys.OrderApiKey)
                    && string.IsNullOrEmpty(config.Mouser.ApiKeys.CartApiKey))
                {
                    ConfigAssert("Integrations.Mouser.ApiKeys.SearchApiKey", config.Mouser.ApiKeys.SearchApiKey, "At least one api key must be provided.");
                }
            }
            if (config.Arrow.Enabled)
            {
                if (string.IsNullOrEmpty(config.Arrow.ApiKey) || config.Arrow.ApiKey.SurroundedBy("@"))
                    ConfigAssert("Integrations.Arrow.ApiKey", config.Arrow.ApiKey);
                if (string.IsNullOrEmpty(config.Arrow.Username) || config.Arrow.Username.SurroundedBy("@"))
                    ConfigAssert("Integrations.Arrow.Username", config.Arrow.Username);
            }
        }

        public void ValidateConfiguration(StorageProviderConfiguration config)
        {
            if (string.IsNullOrEmpty(config.Provider) || config.Provider.SurroundedBy("@"))
                ConfigAssert("StorageProviderConfiguration.Provider", config.Provider);
            if (config.ProviderConfiguration.ContainsKey("Filename") && config.ProviderConfiguration["Filename"].SurroundedBy("@"))
                ConfigAssert("StorageProviderConfiguration.ProviderConfiguration.Filename", config.ProviderConfiguration["Filename"]);
            if (config.ProviderConfiguration.ContainsKey("Host") && config.ProviderConfiguration["Host"].SurroundedBy("@"))
                ConfigAssert("StorageProviderConfiguration.ProviderConfiguration.Host", config.ProviderConfiguration["Host"]);
            if (config.ProviderConfiguration.ContainsKey("Port") && config.ProviderConfiguration["Port"].SurroundedBy("@"))
                ConfigAssert("StorageProviderConfiguration.ProviderConfiguration.Port", config.ProviderConfiguration["Port"]);
            if (config.ProviderConfiguration.ContainsKey("Database") && config.ProviderConfiguration["Database"].SurroundedBy("@"))
                ConfigAssert("StorageProviderConfiguration.ProviderConfiguration.Database", config.ProviderConfiguration["Database"]);
            if (config.ProviderConfiguration.ContainsKey("Username") && config.ProviderConfiguration["Username"].SurroundedBy("@"))
                ConfigAssert("StorageProviderConfiguration.ProviderConfiguration.Username", config.ProviderConfiguration["Username"]);
            if (config.ProviderConfiguration.ContainsKey("Password") && config.ProviderConfiguration["Password"].SurroundedBy("@"))
                ConfigAssert("StorageProviderConfiguration.ProviderConfiguration.Password", config.ProviderConfiguration["Password"]);
            if (config.ProviderConfiguration.ContainsKey("AdditionalParameters") && config.ProviderConfiguration["AdditionalParameters"].SurroundedBy("@"))
                ConfigAssert("StorageProviderConfiguration.ProviderConfiguration.AdditionalParameters", config.ProviderConfiguration["AdditionalParameters"]);
            if (config.ProviderConfiguration.ContainsKey("ConnectionString") && config.ProviderConfiguration["ConnectionString"].SurroundedBy("@"))
                ConfigAssert("StorageProviderConfiguration.ProviderConfiguration.ConnectionString", config.ProviderConfiguration["ConnectionString"]);
            if (config.UserUploadedFilesPath.SurroundedBy("@"))
                ConfigAssert("StorageProviderConfiguration.UserUploadedFilesPath", config.UserUploadedFilesPath);
        }

        private void ConfigAssert(string name, object? value, string? message = "")
        {
            var msg = $"Configuration value {nameof(WebHostServiceConfiguration)}.{name}={value} is invalid. {message}";
            _logger.Error(msg);
            throw new BinnerConfigurationException(msg);
        }
    }
}
