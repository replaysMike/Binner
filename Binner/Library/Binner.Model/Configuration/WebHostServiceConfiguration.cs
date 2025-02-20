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
        public string? Name { get; set; }

        /// <summary>
        /// The server ip to bind to
        /// </summary>
        public string? IP { get; set; }

        /// <summary>
        /// The public facing Url accessible from the internet
        /// This is required if Digikey API features are used.
        /// </summary>
        public string? PublicUrl { get; set; }

        /// <summary>
        /// The port number to host
        /// </summary>
        public int Port { get; set; } = 7000;

        /// <summary>
        /// Path to SSL certificate
        /// </summary>
        public string SslCertificate { get; set; } = "./certificates/localhost-windows.pfx";

        /// <summary>
        /// Optional password for certificate
        /// </summary>
        public string? SslCertificatePassword { get; set; } = "password";

        /// <summary>
        /// Public resource web address (without https://) for serving public resources
        /// </summary>
        public string ResourceSource { get; set; } = "d6ng6g5o3ih7k.cloudfront.net";

        /// <summary>
        /// Default language
        /// </summary>
        public string Language { get; set; } = "en";

        /// <summary>
        /// Default currency
        /// </summary>
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// License key
        /// </summary>
        public string LicenseKey { get; set; } = string.Empty;

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
        public string? CorsAllowOrigin { get; set; }

        /// <summary>
        /// The lifetime of the part types cache
        /// </summary>
        public TimeSpan MaxPartTypesCacheLifetime { get; set; } = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Locale configuration
        /// </summary>
        public LocaleConfiguration Locale { get; set; } = new();

        /// <summary>
        /// Authentication configuration
        /// </summary>
        public AuthenticationConfiguration Authentication { get; set; } = new AuthenticationConfiguration();

        /// <summary>
        /// Digikey configuration
        /// </summary>
        public IntegrationConfiguration Integrations { get; set; } = new();

        /// <summary>
        /// Printer configuration
        /// </summary>
        public PrinterConfiguration PrinterConfiguration { get; set; } = new();

        /// <summary>
        /// Registered license configuration for activating paid features
        /// </summary>
        public LicenseConfiguration Licensing { get; set; } = new();

        /// <summary>
        /// Barcode configuration
        /// </summary>
        public BarcodeConfiguration Barcode { get; set; } = new();
    }
}
