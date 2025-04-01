namespace Binner.Model.Configuration
{
    /// <summary>
    /// Service Configuration
    /// </summary>
    /// <remarks>Environment variables with the same name as the property will override appsettings.json values</remarks>
    public class WebHostServiceConfiguration
    {
        private Environments _environment;
        /// <summary>
        /// The application environment (Development, Production)
        /// </summary>
        public Environments Environment
        {
            get
            {
                if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.Environment)))
                    return Enum.Parse<Environments>(System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.Environment) ?? "Production");
                return _environment;
            }
            set
            {
                _environment = value;
            }
        }

        /// <summary>
        /// The server name
        /// </summary>
        public string? Name { get; set; }

        private string? _ip = "*";
        /// <summary>
        /// The server ip to bind to
        /// </summary>
        public string? IP
        {
            get
            {
                if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.Ip)))
                    return System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.Ip);
                return _ip;
            }
            set
            {
                _ip = value;
            }
        }

        private string? _publicUrl = "https://localhost:8090";
        /// <summary>
        /// The public facing Url accessible from the internet
        /// This is required if Digikey API features are used.
        /// </summary>
        public string? PublicUrl
        {
            get
            {
                if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.PublicUrl)))
                    return System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.PublicUrl);
                return _publicUrl;
            }
            set
            {
                _publicUrl = value;
            }
        }

        private int _port = 8090;
        /// <summary>
        /// The port number to host
        /// </summary>
        public int Port
        {
            get
            {
                if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.Port)))
                    return int.Parse(System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.Port) ?? "8090");
                return _port;
            }
            set
            {
                _port = value;
            }
        }

        private string? _sslCertificate = "./Certificates/localhost-windows.pfx";
        /// <summary>
        /// Path to SSL certificate
        /// </summary>
        public string? SslCertificate
        {
            get
            {
                if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.SslCertificate)))
                    return System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.SslCertificate);
                return _sslCertificate;
            }
            set
            {
                _sslCertificate = value;
            }
        }

        private string? _sslCertificatePassword = "password";
        /// <summary>
        /// Optional password for certificate
        /// </summary>
        public string? SslCertificatePassword
        {
            get
            {
                if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.SslCertificatePassword)))
                    return System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.SslCertificatePassword);
                return _sslCertificatePassword;
            }
            set
            {
                _sslCertificatePassword = value;
            }
        }

        private string? _resourceSource = "d6ng6g5o3ih7k.cloudfront.net";
        /// <summary>
        /// Public resource web address (without https://) for serving public resources
        /// </summary>
        public string? ResourceSource
        {
            get
            {
                if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.ResourceSource)))
                    return System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.ResourceSource);
                return _resourceSource;
            }
            set
            {
                _resourceSource = value;
            }
        }

        private string? _language = "en";
        /// <summary>
        /// Default language
        /// </summary>
        public string? Language
        {
            get
            {
                if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.Language)))
                    return System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.Language);
                return _language;
            }
            set
            {
                _language = value;
            }
        }

        private string? _currency = "USD";
        /// <summary>
        /// Default currency
        /// </summary>
        public string? Currency {
            get
            {
                if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.Currency)))
                    return System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.Currency);
                return _currency;
            }
            set
            {
                _currency = value;
            }
        }

        private string? _licenseKey = string.Empty;
        /// <summary>
        /// License key
        /// </summary>
        public string? LicenseKey {
            get
            {
                if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.LicenseKey)))
                    return System.Environment.GetEnvironmentVariable(EnvironmentVarConstants.LicenseKey);
                return _licenseKey;
            }
            set
            {
                _licenseKey = value;
            }
        }

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
