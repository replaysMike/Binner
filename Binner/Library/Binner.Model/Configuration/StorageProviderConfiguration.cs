namespace Binner.Model.Configuration
{
    public class StorageProviderConfiguration
    {
        private string _provider = "Binner";
        /// <summary>
        /// The storage provider to use
        /// </summary>
        public string Provider { 
            get
            {
                if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.StorageProviderName)))
                    return System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.StorageProviderName) ?? "Binner";
                return _provider;
            }
            set
            {
                _provider = value;
            }
        }

        /// <summary>
        /// The storage provider
        /// </summary>
        public StorageProviders StorageProvider => Enum.Parse<StorageProviders>(Provider, true);

        /// <summary>
        /// Configuration to pass to the provider
        /// </summary>
        public IDictionary<string, string> ProviderConfiguration { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// The path to store user uploaded files to
        /// </summary>
        public string? UserUploadedFilesPath { get; set; }
    }
}
