using Binner.Common;
using Binner.Common.Extensions;
using Binner.Common.IO;
using Binner.Global.Common.Services;
using Binner.Model.IO.Printing;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Binner.Services.IO.Printing
{
    /// <summary>
    /// Windows environment printer via System.Drawing.Printing abstraction
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class WindowsPrinterEnvironment : IPrinterEnvironment
    {
        private readonly ILogger<WindowsPrinterEnvironment> _logger;
        private readonly IPrinterSettings _printerSettings;
        private PrinterOptions _options = new();
        private LabelDefinition _labelProperties = new();
        private Image<Rgba32>? _printImage;
        private ISystemHubProxy _systemHubProxy;
        private IPrintContext _printContext;

        public WindowsPrinterEnvironment(ILogger<WindowsPrinterEnvironment> logger, IPrinterSettings printerSettings, ISystemHubProxy systemHubProxy)
        {
            _logger = logger;
            _printerSettings = printerSettings;
            _systemHubProxy = systemHubProxy;
        }

        public PrinterResult PrintLabel(PrinterOptions options, LabelDefinition labelProperties, Image<Rgba32> labelImage, IPrintContext printContext)
        {
            // _printImage is required at the class level because of System.Drawing.Printing event that sets the contents of the document
            _options = options;
            _printImage = labelImage;
            _labelProperties = labelProperties;
            _printContext = printContext;
            var doc = CreatePrinterDocument(options, labelProperties);
            doc.Print();

            return new PrinterResult
            {
                IsSuccess = true
            };
        }

        private PrintDocument CreatePrinterDocument(PrinterOptions options, LabelDefinition labelDefinition)
        {
            var doc = new PrintDocument();
            SetPrinterByName(doc, _printerSettings.PrinterName);

            // via: https://stackoverflow.com/questions/28007554/how-can-i-save-and-restore-printersettings
            // after examining the raw DEVMODE values, there are no other values that are available to us through the windows print system
            /*byte[] devModeData;
            var hDevMode = doc.PrinterSettings.GetHdevmode();
            IntPtr pDevMode = NativeMethods.GlobalLock(hDevMode);
            var devMode = (NativeMethods.DEVMODE)Marshal.PtrToStructure(pDevMode, typeof(NativeMethods.DEVMODE));
            PrintObject(devMode);
            var devModeSize = devMode.dmSize + devMode.dmDriverExtra;
            devModeData = new byte[devModeSize];
            Marshal.Copy(pDevMode, devModeData, 0, devModeSize);*/

            doc.PrintPage += T_PrintPage;
            doc.QueryPageSettings += Doc_QueryPageSettings;
            doc.EndPrint += Doc_EndPrint;
            doc.DocumentName = labelDefinition.LabelName;
            doc.DefaultPageSettings.Landscape = true; // required
            doc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0); // no effect
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
            var paperSizeFound = false;
            foreach (PaperSize paperSize in doc.PrinterSettings.PaperSizes)
            {
                if (paperSize.PaperName.Equals(_labelProperties.MediaSize.ModelName))
                {
                    doc.DefaultPageSettings.PaperSize = paperSize;
                    paperSizeFound = true;
                    break;
                }
            }
            if (!paperSizeFound)
            {
                foreach (PaperSize paperSize in doc.PrinterSettings.PaperSizes)
                {
                    if (paperSize.PaperName.StartsWith(_labelProperties.MediaSize.ModelName))
                    {
                        doc.DefaultPageSettings.PaperSize = paperSize;
                        paperSizeFound = true;
                        break;
                    }
                }
            }

            if (paperSizeFound)
            {
                if (options.TapeWidthMm > 0)
                {
                    // tape style print
                    var tapeLengthMm = options.TapeLengthMm;
                    if (tapeLengthMm <= 0)
                    {
                        // tape length was not specified, calculate it for best output
                        tapeLengthMm = PrintUtils.CalculateTapeLengthMm(labelDefinition.TapeWidthMm!.Value, labelDefinition.TapeTopMarginMm, labelDefinition.TapeBottomMarginMm, _printImage?.Width ?? 1, _printImage?.Height ?? 1);
                        if (_printerSettings.PrintHardware == Model.Configuration.PrintHardwares.DymoTape)
                        {
                            // the Dymo software prints an extra 7.5mm of length for some reason, uncomment if we want to match this
                            //tapeLengthMm += 7.5f;
                        }
                    }

                    // in order to customize tape length, we must create a custom paper size
                    var customLength = (tapeLengthMm / 25.4d) * 100; // convert to 1/100th of an inch (units)
                    var customPaperSize = new PaperSize($"{doc.DefaultPageSettings.PaperSize.PaperName}-{tapeLengthMm}mm", doc.DefaultPageSettings.PaperSize.Width, (int)customLength);
                    customPaperSize.RawKind = doc.DefaultPageSettings.PaperSize.RawKind;
                    doc.DefaultPageSettings.PaperSize = customPaperSize;
                }
            }

            // configure resolution
            // first, find a supported resolution
            foreach (PrinterResolution resolution in doc.PrinterSettings.PrinterResolutions)
            {
                if (resolution.Y == labelDefinition.HorizontalDpi && resolution.X == labelDefinition.Dpi)
                {
                    doc.PrinterSettings.DefaultPageSettings.PrinterResolution = resolution;
                    break;
                }
                else if (resolution.X == labelDefinition.HorizontalDpi && resolution.Y == labelDefinition.Dpi)
                {
                    doc.PrinterSettings.DefaultPageSettings.PrinterResolution = resolution;
                    break;
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


        // fired before a page is printed
        private void Doc_QueryPageSettings(object sender, QueryPageSettingsEventArgs e)
        {
            var pageSettings = e.PageSettings;
        }

        private void T_PrintPage(object sender, PrintPageEventArgs e)
        {
            _logger.LogInformation($"Printing label at dpi X:{e.PageSettings.PrinterResolution.X}, Y:{e.PageSettings.PrinterResolution.Y}");
            // Printing requires a System.Drawing.Bitmap so it must be converted
            var bitmap = _printImage?.ToBitmap();
            // set the print resolution to the printer's dpi setting so the image will be printed at the correct scale
            bitmap.SetResolution(e.Graphics.DpiX, e.Graphics.DpiY);
            if (bitmap != null)
            {
                if (_options.TapeWidthMm > 0)
                {
                    // tape type label
                    // fit the height to the max paper height, and shrink the width to fit
                    var ratio = (float)bitmap.Height / bitmap.Width;
                    // width and height are transposed on the destination rect due to paper dimensions
                    //var destHeight = e.PageSettings.PrintableArea.Width;
                    var destHeight = e.PageSettings.PaperSize.Width - (e.PageSettings.PrintableArea.X * 2); // use a recalculated version of PrintableArea based on the paper size
                    var scaledWidth = destHeight / ratio;
                    e.Graphics?.DrawImage(bitmap, new System.Drawing.RectangleF(0, 0, scaledWidth, destHeight), new System.Drawing.RectangleF(0, 0, bitmap.Width, bitmap.Height), System.Drawing.GraphicsUnit.Pixel);
                }
                else
                {
                    // die cut label
                    e.Graphics?.DrawImage(bitmap, new System.Drawing.Point(0, 0));
                }
            }
            e.HasMorePages = false;
        }

        private void Doc_EndPrint(object sender, PrintEventArgs e)
        {
            try
            {
                if (_printContext.UserId > 0 && _printContext.OrganizationId > 0)
                {
                    AsyncHelper.RunSync(async () =>
                    {
                        await _systemHubProxy.NotifyPrintCompleteAsync(_printContext.PartName, _printContext.UserId, _printContext.OrganizationId);
                    });
                }
            }
            catch (Exception) { }
        }

        static class NativeMethods
        {
            private const string Kernel32 = "kernel32.dll";

            [DllImport(Kernel32, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
            public static extern IntPtr GlobalLock(IntPtr handle);

            [DllImport(Kernel32, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
            public static extern bool GlobalUnlock(IntPtr handle);

            [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Auto)]
            public struct DEVMODE
            {
                private const int CCHDEVICENAME = 32;
                private const int CCHFORMNAME = 32;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
                public string dmDeviceName;
                public short dmSpecVersion;
                public short dmDriverVersion;
                public short dmSize;
                public short dmDriverExtra;
                public int dmFields;

                public int dmPositionX;
                public int dmPositionY;
                public int dmDisplayOrientation;
                public int dmDisplayFixedOutput;

                public short dmColor;
                public short dmDuplex;
                public short dmYResolution;
                public short dmTTOption;
                public short dmCollate;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
                public string dmFormName;
                public short dmLogPixels;
                public int dmBitsPerPel;
                public int dmPelsWidth;
                public int dmPelsHeight;
                public int dmDisplayFlags;
                public int dmDisplayFrequency;
                public int dmICMMethod;
                public int dmICMIntent;
                public int dmMediaType;
                public int dmDitherType;
                public int dmReserved1;
                public int dmReserved2;
                public int dmPanningWidth;
                public int dmPanningHeight;
            }
        }
    }
}