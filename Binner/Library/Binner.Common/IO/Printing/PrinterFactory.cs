using Binner.Model.IO.Printing;
using Microsoft.Extensions.Logging;
using System;

namespace Binner.Common.IO.Printing
{
    /// <summary>
    /// Generates an IPrinter instance for the designated platform
    /// </summary>
    /// <exception cref="PlatformNotSupportedException"></exception>
    public class PrinterFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public PrinterFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
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
                return new WindowsPrinterEnvironment(_loggerFactory.CreateLogger<WindowsPrinterEnvironment>(), printerSettings);
            }
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsFreeBSD())
            {
                return new CupsPrinterEnvironment(_loggerFactory.CreateLogger<CupsPrinterEnvironment>(), printerSettings);
            }

            throw new PlatformNotSupportedException();
        }
    }
}
