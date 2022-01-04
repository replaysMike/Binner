using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.Versioning;

namespace Binner.Common.IO.Printing
{
    /// <summary>
    /// Windows environment printer via System.Drawing.Printing abstraction
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal class WindowsPrinterEnvironment : IPrinterEnvironment
    {
        private readonly IPrinterSettings _printerSettings;
        private LabelProperties _labelProperties;
        private Image<Rgba32> _printImage;

        public WindowsPrinterEnvironment(IPrinterSettings printerSettings)
        {
            _printerSettings = printerSettings;
        }

        public PrinterResult PrintLabel(PrinterOptions options, LabelProperties labelProperties, Image<Rgba32> labelImage)
        {
            // _printImage is required at the class level because of System.Drawing.Printing event that sets the contents of the document
            _printImage = labelImage;
            _labelProperties = labelProperties;
            var doc = CreatePrinterDocument(options);
            doc.Print();

            return new PrinterResult
            {
                IsSuccess = true
            };
        }

        private PrintDocument CreatePrinterDocument(PrinterOptions options)
        {
            var doc = new PrintDocument();
            SetPrinterByName(doc, _printerSettings.PrinterName);
            doc.PrintPage += T_PrintPage;
            doc.DefaultPageSettings.Landscape = true;
            doc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            foreach (PaperSource paperSource in doc.PrinterSettings.PaperSources)
            {
                if (paperSource.SourceName.Equals(GetSourceName(options.LabelSource ?? _printerSettings.LabelSource), StringComparison.CurrentCultureIgnoreCase))
                {
                    doc.DefaultPageSettings.PaperSource = paperSource;
                    break;
                }
            }
            doc.DefaultPageSettings.PaperSize = new PaperSize(_labelProperties.LabelName, _labelProperties.Dimensions.Width, _labelProperties.Dimensions.Height * _labelProperties.LabelCount);
            return doc;
        }

        private static void SetPrinterByName(PrintDocument t, string printerName)
        {
            if (!string.IsNullOrEmpty(printerName))
            {
                var installedPrinters = System.Drawing.Printing.PrinterSettings.InstalledPrinters.Cast<string>().ToList();
                var foundPrinter = installedPrinters.FirstOrDefault(x => x.Equals(printerName, StringComparison.CurrentCultureIgnoreCase));
                if (!string.IsNullOrEmpty(foundPrinter))
                    t.PrinterSettings.PrinterName = foundPrinter;
                else
                    throw new Exception($"Could not locate printer named: '{printerName}' - the following printers are available: {string.Join(",", installedPrinters)}");
            }
        }

        private string GetSourceName(LabelSource labelSource)
        {
            return labelSource switch
            {
                LabelSource.Left => "Left Roll",
                LabelSource.Right => "Right Roll",
                _ => "Automatically Select",
            };
            throw new InvalidOperationException($"Unknown label source: {labelSource}");
        }

        private void T_PrintPage(object sender, PrintPageEventArgs e)
        {
            // Printing requires a System.Drawing.Bitmap so it must be converted
            var bitmap = _printImage.ToBitmap();
            e.Graphics.DrawImage(bitmap, new System.Drawing.Point(0, -12));
            e.HasMorePages = false;
        }
    }
}