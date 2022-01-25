using Binner.Web.Configuration;

namespace Binner.Web.ServiceHost
{
    public class AppSettings
    {
        public WebHostServiceConfiguration WebHostServiceConfiguration { get; set; }
        public StorageProviderConfiguration StorageProviderConfiguration { get; set; }
        public LoggingConfiguration Logging { get; set; }
    }
}
