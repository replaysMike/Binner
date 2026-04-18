using Binner.Global.Common.Services;
using Binner.Model.Configuration;
using Binner.Model.IO.Printing;
using Binner.Model.IO.Printing.PrinterHardware;
using Microsoft.Extensions.Logging;

namespace Binner.Services.IO.Printing.PrinterHardware
{
    public class PrinterHardwareFactory : IPrinterHardwareFactory
    {
        private ILoggerFactory _loggerFactory;
        private readonly IBarcodeGenerator _barcodeGenerator;
        private readonly IPrinterSettings _printerSettings;
        private readonly ISystemHubProxy _systemHubProxy;

        public PrinterHardwareFactory(ILoggerFactory loggerFactory, IBarcodeGenerator barcodeGenerator, IPrinterSettings printerSettings, ISystemHubProxy systemHubProxy)
        {
            _loggerFactory = loggerFactory;
            _barcodeGenerator = barcodeGenerator;
            _printerSettings = printerSettings;
            _systemHubProxy = systemHubProxy;
        }

        public ILabelPrinterHardware Create(PrintHardwares printHardware)
        {
            switch (printHardware)
            {
                default:
                case PrintHardwares.DymoLabelWriter:
                    return new DymoLabelPrinterHardware(_loggerFactory, _barcodeGenerator, _printerSettings, _systemHubProxy);
                case PrintHardwares.DymoTape:
                    return new DymoTapeLabelPrinterHardware(_loggerFactory, _barcodeGenerator, _printerSettings, _systemHubProxy);
                case PrintHardwares.BrotherPTouch:
                    return new BrotherLabelPrinterHardware(_loggerFactory, _barcodeGenerator, _printerSettings, _systemHubProxy);
            }
            throw new NotSupportedException();
        }
    }
}
