using Binner.Global.Common.Services;
using Binner.Model.IO.Printing;
using Microsoft.Extensions.Logging;

namespace Binner.Services.IO.Printing
{
    /// <summary>
    /// Generates an IPrinter instance for the designated platform
    /// </summary>
    /// <exception cref="PlatformNotSupportedException"></exception>
    public class PrinterEnvironmentFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ISystemHubProxy _systemHubProxy;

        public PrinterEnvironmentFactory(ILoggerFactory loggerFactory, ISystemHubProxy systemHubProxy)
        {
            _loggerFactory = loggerFactory;
            _systemHubProxy = systemHubProxy;
        }

        /// <summary>
        /// Create a printer instance
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public IPrinterEnvironment CreatePrinter(IPrinterSettings printerSettings)
        {
            if (OperatingSystem.IsWindows())
            {
                return new WindowsPrinterEnvironment(_loggerFactory.CreateLogger<WindowsPrinterEnvironment>(), printerSettings, _systemHubProxy);
            }
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsFreeBSD())
            {
                return new CupsPrinterEnvironment(_loggerFactory.CreateLogger<CupsPrinterEnvironment>(), printerSettings, _systemHubProxy);
            }

            throw new PlatformNotSupportedException();
        }
    }
}
