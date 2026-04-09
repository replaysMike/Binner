using Binner.Model.Configuration;
using Binner.Model.IO.Printing;

namespace Binner.Model.Responses
{
    public class PrinterConfigurationResponse
    {
        public IPrinterSettings PrinterConfiguration { get; set; } = new PrinterSettings();
        public int Crc { get; set; }
    }
}
