namespace Binner.Model.Configuration
{
    public interface IApiConfiguration
    {
        /// <summary>
        /// True if api is enabled
        /// </summary>
        public bool Enabled { get; }
        
        /// <summary>
        /// The api key
        /// </summary>
        public string? ApiKey { get; }

        /// <summary>
        /// True if api is configured
        /// </summary>
        public bool IsConfigured { get; }
    }
}
