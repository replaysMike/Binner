using Microsoft.Extensions.Logging;

namespace Binner.Web.Configuration
{
    public static class ApplicationLogging
    {
        internal static ILoggerFactory LoggerFactory { get; set; }
    }
}
