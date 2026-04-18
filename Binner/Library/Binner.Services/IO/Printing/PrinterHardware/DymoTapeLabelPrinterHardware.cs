using Binner.Global.Common.Services;
using Binner.Model.IO.Printing;
using Binner.Model.IO.Printing.PrinterHardware;
using Binner.Services.IO.Printing.PrinterHardware;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Binner.Services.IO.Printing
{
    /// <summary>
    /// Dymo tape style printer
    /// </summary>
    public class DymoTapeLabelPrinterHardware : ILabelPrinterHardware
    {
        private ILoggerFactory _loggerFactory;
        private readonly IBarcodeGenerator _barcodeGenerator;
        private readonly IPrinterEnvironment _printer;
        private readonly IPrinterSettings _printerSettings;
        private readonly ISystemHubProxy _systemHubProxy;

        public DymoTapeLabelPrinterHardware(ILoggerFactory loggerFactory, IBarcodeGenerator barcodeGenerator, IPrinterSettings printerSettings, ISystemHubProxy systemHubProxy)
        {
            _loggerFactory = loggerFactory;
            _barcodeGenerator = barcodeGenerator;
            _printerSettings = printerSettings;
            _systemHubProxy = systemHubProxy;
            _printer = new PrinterEnvironmentFactory(loggerFactory, _systemHubProxy).CreatePrinter(_printerSettings);
        }

        public void PrintLabelImage(Image<Rgba32> image, PrinterOptions options, IPrintContext printContext)
        {
            var labelProperties = GetLabelDimensions(options.TapeWidthMm, options.TapeLengthMm);
            _printer.PrintLabel(options, labelProperties, image, printContext);
        }

        public Image<Rgba32> PrintLabel(LabelContent content, PrinterOptions options, IPrintContext printContext)
        {
            // legacy
            throw new NotImplementedException();
        }

        public Image<Rgba32> PrintLabel(ICollection<LineConfiguration> lines, PrinterOptions options, IPrintContext printContext)
        {
            // legacy
            throw new NotImplementedException();
        }

        private LabelDefinition GetLabelDimensions(float tapeWidthMm, float tapeLengthMm)
        {
            // media modelName is specified in inches
            var tapeWidth = DymoTapeWidths.Values
                .Where(x => x.TapeWidthMm == tapeWidthMm)
                .FirstOrDefault() ?? DymoTapeWidths.Values[2];
            return new LabelDefinition(new MediaSize(tapeWidth.ModelName), tapeWidth.TapeWidthMm, tapeLengthMm, tapeWidth.LeftMarginMm, tapeWidth.TopMarginMm, tapeWidth.BottomMarginMm);
        }
    }
}
