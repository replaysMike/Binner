namespace Binner.Model.Configuration
{
    public class OrganizationConfiguration
    {
        /// <summary>
        /// Use the Binner module to configure the UI
        /// </summary>
        public BinnerModules UseModule { get; set; }

        /// <summary>
        /// If you have a paid Binner Cloud subscription, provide your license key to activate pro features
        /// </summary>
        public string? LicenseKey { get; set; }

        /// <summary>
        /// Maximum number of items that can be cached
        /// </summary>
        public int MaxCacheItems { get; set; } = 1024;

        /// <summary>
        /// Sliding cache expiration in minutes
        /// </summary>
        public int CacheSlidingExpirationMinutes { get; set; } = 30;

        /// <summary>
        /// Absolute cache expiration in minutes (0 = never)
        /// </summary>
        public int CacheAbsoluteExpirationMinutes { get; set; } = 0;
    }
}
