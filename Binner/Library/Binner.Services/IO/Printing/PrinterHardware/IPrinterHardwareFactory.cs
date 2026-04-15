using Binner.Model.Configuration;
using Binner.Model.IO.Printing.PrinterHardware;

namespace Binner.Services.IO.Printing.PrinterHardware
{
    public interface IPrinterHardwareFactory
    {
        ILabelPrinterHardware Create(PrintHardwares printHardware);
    }
}