using Binner.Common.IO.Printing;
using Binner.Model.IO.Printing;
using Binner.Testing;
using NUnit.Framework;
using SixLabors.ImageSharp;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Binner.Common.Tests.IO.Printing.PrinterHardware
{
    [TestFixture]
    public class DymoLabelPrinterHardwareTests
    {
        /// <summary>
        /// Enable this to write a test image during the test run.
        /// Once an image is generated in the test build output, copy it to the TestImages project folder and enable "Copy if newer"
        /// </summary>
        private const bool WriteImages = false;
        /// <summary>
        /// Enable this to validate the image for equality
        /// </summary>
        private const bool ValidateImages = true;
        /// <summary>
        /// False will print to printer, True will generate an image only
        /// </summary>
        private const bool GenerateImageOnly = true;
        /// <summary>
        /// True will draw debug rects around each line in the label
        /// </summary>
        private const bool DrawDebug = false;

        private const string ImageValidationFolder = @".\IO\_Data\Barcodes";
        private const string ImageOutputFolder = @".\TestOutput";

        [OneTimeSetUp]
        public void Setup()
        {
            Directory.CreateDirectory(ImageOutputFolder);
        }

        [Test]
        public async Task ShouldGenerate30346BarcodeAsync()
        {
            var testContext = new TestContext();
            var printerSettings = GetPrinterSettings();
            var barcodeGenerator = new BarcodeGenerator();
            var hardware = new DymoLabelPrinterHardware(testContext.LoggerFactory.Object, printerSettings, barcodeGenerator);

            var lines = new List<LineConfiguration>()
            {
                new LineConfiguration()
                {
                    Label = 2,
                    Content = "LM358", // struggles beyond 16 chars
                    Barcode = true,
                    Margin = new Margin(0, 0, 0, 0),
                    FontSize = 16,
                }
            };
            var options = GetPrinterOptions(GenerateImageOnly, DrawDebug);

            var image = hardware.PrintLabel(lines, options);

            if (WriteImages)
                await image.SaveAsPngAsync(@$"{ImageOutputFolder}\{nameof(ShouldGenerate30346BarcodeAsync)}.png");

            Assert.That(image.Bounds.Width, Is.EqualTo(1043));
            Assert.That(image.Bounds.Height, Is.EqualTo(150));

            if (ValidateImages)
                ImageValidator.AssertValidateImageEquality($@"{ImageValidationFolder}\{nameof(ShouldGenerate30346BarcodeAsync)}.png", image);
        }

        [Test]
        public async Task ShouldGenerate30346LongBarcodeAsync()
        {
            var testContext = new TestContext();
            var printerSettings = GetPrinterSettings();
            var barcodeGenerator = new BarcodeGenerator();
            var hardware = new DymoLabelPrinterHardware(testContext.LoggerFactory.Object, printerSettings, barcodeGenerator);

            var lines = new List<LineConfiguration>()
            {
                new LineConfiguration()
                {
                    Label = 2,
                    Content = "NC11-LM358QFN-ND", // struggles beyond 16 chars
                    Barcode = true,
                    Margin = new Margin(0, 0, 0, 0),
                    FontSize = 16,
                }
            };
            var options = GetPrinterOptions(GenerateImageOnly, DrawDebug);

            var image = hardware.PrintLabel(lines, options);

            if (WriteImages)
                await image.SaveAsPngAsync(@$"{ImageOutputFolder}\{nameof(ShouldGenerate30346LongBarcodeAsync)}.png");

            Assert.That(image.Bounds.Width, Is.EqualTo(1043));
            Assert.That(image.Bounds.Height, Is.EqualTo(150));

            if (ValidateImages)
                ImageValidator.AssertValidateImageEquality($@"{ImageValidationFolder}\{nameof(ShouldGenerate30346LongBarcodeAsync)}.png", image);
        }

        [Test]
        public async Task ShouldGenerate30346PartLabelAsync()
        {
            var testContext = new TestContext();
            var printerSettings = GetPrinterSettings();
            var barcodeGenerator = new BarcodeGenerator();
            var hardware = new DymoLabelPrinterHardware(testContext.LoggerFactory.Object, printerSettings, barcodeGenerator);

            var lines = new List<LineConfiguration>()
            {
                new LineConfiguration()
                {
                    Label = 2,
                    Content = "LM358",
                    Barcode = false,
                    Margin = new Margin(0, 0, -15, 0),
                    FontSize = 10,
                },
                new LineConfiguration()
                {
                    Label = 2,
                    Content = "General purpose op amp",
                    Barcode = false,
                    Margin = new Margin(0, 0, 5, 0),
                    FontSize = 6,
                },
                new LineConfiguration()
                {
                    Label = 2,
                    Content = "LM358", // struggles beyond 16 chars
                    Barcode = true,
                    Margin = new Margin(0, 0, -10, 0),
                    FontSize = 8,
                    
                },
            };
            var options = GetPrinterOptions(GenerateImageOnly, DrawDebug);

            var image = hardware.PrintLabel(lines, options);

            if (WriteImages)
                await image.SaveAsPngAsync(@$"{ImageOutputFolder}\{nameof(ShouldGenerate30346PartLabelAsync)}.png");

            Assert.That(image.Bounds.Width, Is.EqualTo(1043));
            Assert.That(image.Bounds.Height, Is.EqualTo(150));

            if (ValidateImages)
                ImageValidator.AssertValidateImageEquality($@"{ImageValidationFolder}\{nameof(ShouldGenerate30346PartLabelAsync)}.png", image);
        }

        [Test]
        public async Task ShouldGenerate30346LongPartLabelAsync()
        {
            var testContext = new TestContext();
            var printerSettings = GetPrinterSettings();
            var barcodeGenerator = new BarcodeGenerator();
            var hardware = new DymoLabelPrinterHardware(testContext.LoggerFactory.Object, printerSettings, barcodeGenerator);

            var lines = new List<LineConfiguration>()
            {
                new LineConfiguration()
                {
                    Label = 2,
                    Content = "NC11-LM358QFN-ND",
                    Barcode = false,
                    Margin = new Margin(0, 0, -15, 0),
                    FontSize = 10,
                },
                new LineConfiguration()
                {
                    Label = 2,
                    Content = "General purpose op amp for voltage reference",
                    Barcode = false,
                    Margin = new Margin(0, 0, 5, 0),
                    FontSize = 6,
                },
                new LineConfiguration()
                {
                    Label = 2,
                    Content = "NC11-LM358QFN-ND", // struggles beyond 16 chars
                    Barcode = true,
                    Margin = new Margin(0, 0, -10, 0),
                    FontSize = 8,

                },
            };
            var options = GetPrinterOptions(GenerateImageOnly, DrawDebug);

            var image = hardware.PrintLabel(lines, options);

            if (WriteImages)
                await image.SaveAsPngAsync(@$"{ImageOutputFolder}\{nameof(ShouldGenerate30346LongPartLabelAsync)}.png");

            Assert.That(image.Bounds.Width, Is.EqualTo(1043));
            Assert.That(image.Bounds.Height, Is.EqualTo(150));

            if (ValidateImages)
                ImageValidator.AssertValidateImageEquality($@"{ImageValidationFolder}\{nameof(ShouldGenerate30346LongPartLabelAsync)}.png", image);
        }

        [Test]
        public async Task ShouldGenerate30277PartLabelAsync()
        {
            var testContext = new TestContext();
            var printerSettings = GetPrinterSettings();
            var barcodeGenerator = new BarcodeGenerator();
            var hardware = new DymoLabelPrinterHardware(testContext.LoggerFactory.Object, printerSettings, barcodeGenerator);

            var lines = new List<LineConfiguration>()
            {
                new LineConfiguration()
                {
                    Label = 1,
                    Content = "NC11-LM358-ZFQFNMFR105-ND",
                    Barcode = false,
                    Margin = new Margin(0, 0, -15, 0),
                    FontSize = 10,
                },
                new LineConfiguration()
                {
                    Label = 1,
                    Content = "NC11-LM358-ZFQFNMFR105-ND", // struggles beyond 25 chars
                    Barcode = true,
                    Margin = new Margin(0, 0, -10, 0),
                    FontSize = 8,

                },
            };
            var options = GetPrinterOptions(GenerateImageOnly, DrawDebug, "30277", LabelSource.Left);

            var image = hardware.PrintLabel(lines, options);

            if (WriteImages)
                await image.SaveAsPngAsync(@$"{ImageOutputFolder}\{nameof(ShouldGenerate30277PartLabelAsync)}.png");

            Assert.That(image.Bounds.Width, Is.EqualTo(1976));
            Assert.That(image.Bounds.Height, Is.EqualTo(341));

            if (ValidateImages)
                ImageValidator.AssertValidateImageEquality($@"{ImageValidationFolder}\{nameof(ShouldGenerate30277PartLabelAsync)}.png", image);
        }

        private PrinterOptions GetPrinterOptions(bool imageOnly = true, bool isDebug = true, string labelName = "30346", LabelSource labelSource = LabelSource.Right) => new PrinterOptions()
        {
            LabelSource = labelSource,
            LabelName = labelName,
            GenerateImageOnly = imageOnly,
            ShowDiagnostic = isDebug
        };

        private IPrinterSettings GetPrinterSettings()
        {
            var printerSettings = new PrinterSettings()
            {
                LabelDefinitions = new List<LabelDefinition>()
                {
                    new LabelDefinition()
                    {
                        MediaSize = new MediaSize(82, 248, "File Folder (2 up)", "w82h248", "30277", ""),
                        LabelCount = 2,
                        TotalLines = 2,
                        TopMargin = 0,
                        LeftMargin = 0,
                        HorizontalDpi = 600,
                        Dpi = 300
                    },
                    new LabelDefinition()
                    {
                        MediaSize = new MediaSize(36, 136, "1/2 in x 1-7/8 in", "w36h136", "30346", ""),
                        LabelCount = 1,
                        TotalLines = 2,
                        TopMargin = 0,
                        LeftMargin = 0,
                        HorizontalDpi = 600,
                        Dpi = 300
                    }
                },
                PartLabelName = "30346",
                PartLabelSource = LabelSource.Right,
                PrinterName = "DYMO LabelWriter 450 Twin Turbo",
                PrintMode = Model.Configuration.PrintModes.Direct
            };
            return printerSettings;
        }
    }
}
