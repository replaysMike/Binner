using System;

namespace Binner.Common.IO.Printing
{
    /// <summary>
    /// Generates an IPrinter instance for the designated platform
    /// </summary>
    /// <exception cref="PlatformNotSupportedException"></exception>
    internal class PrinterFactory
    {
        /// <summary>
        /// Create a printer instance
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        internal IPrinterEnvironment CreatePrinter(IPrinterSettings printerSettings)
        {
            if (OperatingSystem.IsWindows())
            {
                return new CupsPrinterEnvironment(printerSettings);
                //return new WindowsPrinter(printerSettings);
            }
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsFreeBSD())
            {
                return new CupsPrinterEnvironment(printerSettings);
            }

            throw new PlatformNotSupportedException();
        }
    }
}
