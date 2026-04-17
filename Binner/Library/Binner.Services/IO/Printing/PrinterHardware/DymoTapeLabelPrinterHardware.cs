using Binner.Global.Common;
using Binner.Model.IO.Printing;
using Binner.Model.IO.Printing.PrinterHardware;
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

        public DymoTapeLabelPrinterHardware(ILoggerFactory loggerFactory, IBarcodeGenerator barcodeGenerator, IPrinterSettings printerSettings)
        {
            _loggerFactory = loggerFactory;
            _barcodeGenerator = barcodeGenerator;
            _printerSettings = printerSettings;
            _printer = new PrinterFactory(loggerFactory).CreatePrinter(_printerSettings);
        }

        public void PrintLabelImage(Image<Rgba32> image, PrinterOptions options)
        {
            var labelProperties = GetLabelDimensions(options.TapeSizeMm);
            _printer.PrintLabel(options, labelProperties, image);
        }

        public Image<Rgba32> PrintLabel(LabelContent content, PrinterOptions options)
        {
            throw new NotImplementedException();
        }

        public Image<Rgba32> PrintLabel(ICollection<LineConfiguration> lines, PrinterOptions options)
        {
            throw new NotImplementedException();
        }

        private LabelDefinition GetLabelDimensions(string tapeSizeMm)
        {
            var labelDefinition = _printerSettings.LabelDefinitions
                .FirstOrDefault(x => x.MediaSize.ModelName.Equals(tapeSizeMm));
            if (labelDefinition != null)
            {
                // call set dimensions to ensure we calculate all the properties correctly
                labelDefinition.UpdateDimensions();
                return labelDefinition;
            }

            throw new BinnerConfigurationException($"Unsupported label: '{tapeSizeMm}'");
        }
    }
}
