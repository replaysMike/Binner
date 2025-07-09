using Binner.Common.Extensions;
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
        private LabelDefinition _labelProperties = new();
        private Image<Rgba32>? _printImage;

        public WindowsPrinterEnvironment(ILogger<WindowsPrinterEnvironment> logger, IPrinterSettings printerSettings)
        {
            _logger = logger;
            _printerSettings = printerSettings;
        }

        public PrinterResult PrintLabel(PrinterOptions options, LabelDefinition labelProperties, Image<Rgba32> labelImage)
        {
            // _printImage is required at the class level because of System.Drawing.Printing event that sets the contents of the document
            _printImage = labelImage;
            _labelProperties = labelProperties;
            var doc = CreatePrinterDocument(options, labelProperties);
            doc.Print();

            return new PrinterResult
            {
                IsSuccess = true
            };
        }

        /*
        private void PrintObject(object obj)
        {
            var fields = obj.GetFields(FieldOptions.All);
            foreach (var field in fields)
            {
                var val = obj.GetFieldValue(field.Name);
                System.Diagnostics.Debug.WriteLine($"{field.Name}:{val}");
            }
        }*/

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
            foreach (PaperSize paperSize in doc.PrinterSettings.PaperSizes)
            {
                if (paperSize.PaperName.StartsWith(_labelProperties.MediaSize.ModelName))
                {
                    doc.DefaultPageSettings.PaperSize = paperSize;
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
            if (bitmap != null)
                e.Graphics?.DrawImage(bitmap, new System.Drawing.Point(0, 0));
            e.HasMorePages = false;
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