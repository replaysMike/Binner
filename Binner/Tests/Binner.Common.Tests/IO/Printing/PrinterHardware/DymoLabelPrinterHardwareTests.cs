using Binner.Common.IO.Printing;
using Binner.Model.IO.Printing;
using Binner.Testing;
using NLog;
using Moq;
using NUnit.Framework;
using SixLabors.ImageSharp;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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

        private const string ImageValidationFolder = @".\IO\_Data\Barcodes";
        private const string ImageOutputFolder = @".\TestOutput";

        [OneTimeSetUp]
        public void Setup()
        {
            Directory.CreateDirectory(ImageOutputFolder);
        }

        [Test]
        public async Task ShouldGenerateBarcodeAsync()
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
                    Content = "HELLO WORLD",
                    Barcode = true,
                    Margin = new Margin(90, 0, 0, 0),
                    FontSize = 16,
                }
            };
            var options = GetPrinterOptions(imageOnly: true);

            var image = hardware.PrintLabel(lines, options);

            if (WriteImages)
                await image.SaveAsPngAsync(@$"{ImageOutputFolder}\{nameof(ShouldGenerateBarcodeAsync)}.png");

            Assert.That(image.Bounds.Width, Is.EqualTo(1043));
            Assert.That(image.Bounds.Height, Is.EqualTo(150));

            if (ValidateImages)
                ImageValidator.AssertValidateImageEquality($@"{ImageValidationFolder}\{nameof(ShouldGenerateBarcodeAsync)}.png", image);
        }

        private PrinterOptions GetPrinterOptions(bool imageOnly = true, bool isDebug = true) => new PrinterOptions()
        {
            LabelSource = LabelSource.Right,
            LabelName = "30346",
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
                        TopMargin = -20,
                        LeftMargin = 0,
                        HorizontalDpi = 600,
                        Dpi = 300
                    },
                    new LabelDefinition()
                    {
                        MediaSize = new MediaSize(36, 136, "1/2 in x 1-7/8 in", "w36h136", "30346", ""),
                        LabelCount = 1,
                        TotalLines = 2,
                        TopMargin = -20,
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
