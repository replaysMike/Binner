using Microsoft.Extensions.Configuration;
using System.IO;

namespace Binner.Common
{
    public class Config
    {
        public static IConfigurationRoot GetConfiguration(string appSettingsJson, string? path = null)
        {
            if (string.IsNullOrEmpty(path))
                path = Directory.GetCurrentDirectory();
            var filePath = Path.Combine(path, appSettingsJson);
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"The configuration file named '{filePath}' was not found.");
            var builder = new ConfigurationBuilder()
                .SetBasePath(path)
                .AddJsonFile(appSettingsJson, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            return builder.Build();
        }
    }
}
