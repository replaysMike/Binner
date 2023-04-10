namespace Binner.Model.Configuration
{
    /// <summary>
    /// Service Configuration
    /// </summary>
    public class WebHostServiceConfiguration
    {
        /// <summary>
        /// The application environment (Development, Production)
        /// </summary>
        public Environments Environment { get; set; }

        /// <summary>
        /// The server name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The server ip to bind to
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// The public facing Url accessible from the internet
        /// This is required if Digikey API features are used.
        /// </summary>
        public string PublicUrl { get; set; }

        /// <summary>
        /// The port number to host
        /// </summary>
        public int Port { get; set; } = 80;

        /// <summary>
        /// The port number to host ssl on
        /// </summary>
        public int SslPort { get; set; } = 443;

        /// <summary>
        /// True to enable integrations test mode, which will return fake responses rather than querying actual external APIs.
        /// </summary>
        public bool IntegrationsTestMode { get; set; } = false;

        /// <summary>
        /// Maximum number of items to cache
        /// </summary>
        public int MaxCacheItems { get; set; } = 1024;

        /// <summary>
        /// The number of minutes to set the sliding expiration cache to (default: 30)
        /// </summary>
        public int CacheSlidingExpirationMinutes { get; set; } = 30;

        /// <summary>
        /// The number of minutes to set the absolute expiration to (default: 0)
        /// </summary>
        public int CacheAbsoluteExpirationMinutes { get; set; } = 0;

        /// <summary>
        /// The origin to allow for Cors
        /// </summary>
        public string CorsAllowOrigin { get; set; }

        /// <summary>
        /// Authentication configuration
        /// </summary>
        public AuthenticationConfiguration Authentication { get; set; } = new AuthenticationConfiguration();

        /// <summary>
        /// Digikey configuration
        /// </summary>
        public IntegrationConfiguration Integrations { get; set; } = new IntegrationConfiguration();

        /// <summary>
        /// Printer configuration
        /// </summary>
        public PrinterConfiguration PrinterConfiguration { get; set; } = new PrinterConfiguration();

        /// <summary>
        /// Configuration for CertBot SSL Certificate renewals
        /// </summary>
        public CertBotConfiguration CertBotConfiguration { get; set; } = new CertBotConfiguration();
    }

    public enum Environments
    {
        Development,
        Production
    }
}
