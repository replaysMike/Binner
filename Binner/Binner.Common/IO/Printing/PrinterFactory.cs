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
        internal IPrinter CreatePrinter(IPrinterSettings printerSettings)
        {
            if (OperatingSystem.IsWindows())
            {
                return new WindowsPrinter(printerSettings);
            }
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsFreeBSD())
            {
                return new CupsPrinter(printerSettings);
            }

            throw new PlatformNotSupportedException();
        }
    }
}
