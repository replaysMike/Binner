using Binner.Model.IO.Printing;
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
        private LabelDefinition _labelProperties = new ();
        private Image<Rgba32>? _printImage;

        public WindowsPrinterEnvironment(IPrinterSettings printerSettings)
        {
            _printerSettings = printerSettings;
        }

        public PrinterResult PrintLabel(PrinterOptions options, LabelDefinition labelProperties, Image<Rgba32> labelImage)
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
            // select the paper source
            foreach (PaperSource paperSource in doc.PrinterSettings.PaperSources)
            {
                if (paperSource.SourceName.Equals(GetSourceName(options.LabelSource ?? _printerSettings.PartLabelSource), StringComparison.CurrentCultureIgnoreCase))
                {
                    doc.DefaultPageSettings.PaperSource = paperSource;
                    break;
                }
            }
            // select the paper size
            foreach(PaperSize paperSize in doc.PrinterSettings.PaperSizes)
            {
                if (paperSize.PaperName.StartsWith(_labelProperties.MediaSize.ModelName))
                {
                    doc.DefaultPageSettings.PaperSize = paperSize;
                }
            }
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
        }

        private void T_PrintPage(object sender, PrintPageEventArgs e)
        {
            // Printing requires a System.Drawing.Bitmap so it must be converted
            var bitmap = _printImage?.ToBitmap();
            if (bitmap != null)
                e.Graphics?.DrawImage(bitmap, new System.Drawing.Point(0, 0));
            e.HasMorePages = false;
        }
    }
}