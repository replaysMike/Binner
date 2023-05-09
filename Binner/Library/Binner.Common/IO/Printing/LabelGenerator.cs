using Barcoder.Renderer.Image;
using Binner.Common.Extensions;
using Binner.Model;
using Binner.Model.IO.Printing;
using Binner.Model.Requests;
using QRCoder;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TypeSupport.Extensions;

namespace Binner.Common.IO.Printing
{
    public class LabelGenerator : ILabelGenerator
    {
        private const string DefaultFontName = "Segoe UI";
        private const string FontsPath = "./Fonts";
        private static readonly Color DefaultTextColor = Color.Black;
        private static readonly Lazy<FontCollection> FontCollection = new(() => new FontCollection());
        private static FontFamily _fontFamily;
        private readonly IBarcodeGenerator _barcodeGenerator;
        private readonly List<PointF> _labelStart = new();
        private readonly IPrinterEnvironment _printer;
        private Rectangle _paperRect;

        /// <summary>
        /// Printer settings
        /// </summary>
        public IPrinterSettings PrinterSettings { get; set; }

        static LabelGenerator()
        {
            LoadFonts();
        }

        public LabelGenerator(IPrinterSettings printerSettings, IBarcodeGenerator barcodeGenerator)
        {
            PrinterSettings = printerSettings ?? throw new ArgumentNullException(nameof(printerSettings));
            _barcodeGenerator = barcodeGenerator;
            _printer = new PrinterFactory().CreatePrinter(printerSettings);
        }

        public Image<Rgba32> CreateLabelImage(CustomLabelRequest request, Part part)
        {
            // generate the print image and send to printer hardware
            var widthInPixels = InchesToPixels(request.Label.Width, 96);
            var heightInPixels = InchesToPixels(request.Label.Height, 96);
            var image = CreateLabelImage(request.Label);
            // convert the screen dpi to print
            var ratio = request.Label.Dpi / 96f;
            var margins = new int [4] { 0, 0, 0, 0 };
            var marginParts = request.Label.Margin?.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(x =>
                {
                    int.TryParse(x, out var result);
                    return result;
                }).ToArray() ?? new [] { 0 };
            for (var i = 0; i < marginParts.Length; i++)
            {
                if (i < margins.Length)
                    margins[i] = marginParts[i];
            }

            if (marginParts.Length == 1)
            {
                margins[1] = margins[2] = margins[3] = margins[0];
            } else if (marginParts.Length == 2)
            {
                margins[2] = margins[0];
                margins[3] = margins[1];
            }

            // draw margin
            if (request.Label.ShowBoundaries)
            {
                image.Mutate<Rgba32>(c => c.Draw(Pens.DashDotDot(Color.Red, 1),
                    new RectangleF(margins[3], margins[0], (image.Width - margins[1] - margins[3] - 1), (image.Height - margins[0] - margins[2] - 1))));
                // draw label border
                image.Mutate<Rgba32>(c => c.Draw(Pens.DashDotDot(Color.Blue, 1), new RectangleF(0, 0, image.Width - 1, image.Height - 1)));
            }

            foreach (var box in request.Boxes)
            {
                var x = box.Left * ratio;
                var y = box.Top * ratio;
                var width = box.Width * ratio;
                var height = box.Height * ratio;

                var field = box.Name;
                var text = string.Empty;
                var propertyName = field.UcFirst();
                if(part.ContainsProperty(propertyName))
                    text = part.GetPropertyValue<string>(propertyName);

                if (!string.IsNullOrEmpty(box.Properties.Value))
                    text = box.Properties.Value;

                var dataToEncode = text;
                switch (field)
                {
                    case "qrCode":
                        if (string.IsNullOrEmpty(dataToEncode))
                            dataToEncode = Encode2dBarcodePartData(part);
                        DrawQrCode(request, image, box, dataToEncode, x, y, width, height);
                        break;
                    case "dataMatrix2d":
                        if (string.IsNullOrEmpty(dataToEncode))
                            dataToEncode = Encode2dBarcodePartData(part);
                        DrawDataMatrix(request, image, box, dataToEncode, x, y, width, height);
                        break;
                    case "pdf417":
                        if (string.IsNullOrEmpty(dataToEncode))
                            dataToEncode = Encode2dBarcodePartData(part);
                        DrawPdf417(request, image, box, dataToEncode, x, y, width, height);
                        break;
                    case "aztecCode":
                        if (string.IsNullOrEmpty(dataToEncode))
                            dataToEncode = Encode2dBarcodePartData(part);
                        DrawAztecCode(request, image, box, dataToEncode, x, y, width, height);
                        break;
                    case "code128":
                        if (string.IsNullOrEmpty(dataToEncode))
                            dataToEncode = Encode1dBarcodePartData(part);
                        DrawCode128(request, image, box, dataToEncode, x, y, width, height);
                        break;
                    default:
                        DrawText(request, image, box, text, x, y, width, height);
                        break;
                }
            }

            return image;
        }

        private void DrawQrCode(CustomLabelRequest request, Image<Rgba32> image, LabelBox box, string text, float x, float y, float width, float height)
        {
            using var memoryStream = new MemoryStream();
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrCodeData);
            using var barcodeImage = qrCode.GetGraphic(20);
            barcodeImage.Mutate(c => c.Resize((int)width, (int)height));
            image.Mutate(c => c.DrawImage(barcodeImage, new Point((int)x, (int)y), new GraphicsOptions()));
        }

        private void DrawDataMatrix(CustomLabelRequest request, Image<Rgba32> image, LabelBox box, string text, float x, float y, float width, float height)
        {
            using var memoryStream = new MemoryStream();
            var barcode = Barcoder.DataMatrix.DataMatrixEncoder.Encode(text);
            var renderer = new ImageRenderer();
            using var barcodeImage = renderer.Render(barcode);
            barcodeImage.Mutate(c => c.Resize((int)width, (int)height));
            image.Mutate(c => c.DrawImage(barcodeImage, new Point((int)x, (int)y), new GraphicsOptions()));
        }

        private void DrawPdf417(CustomLabelRequest request, Image<Rgba32> image, LabelBox box, string text, float x, float y, float width, float height)
        {
            using var memoryStream = new MemoryStream();
            var barcode = Barcoder.Pdf417.Pdf417Encoder.Encode(text, 2);
            var renderer = new ImageRenderer();
            using var barcodeImage = renderer.Render(barcode);
            barcodeImage.Mutate(c => c.Resize((int)width, (int)height));
            image.Mutate(c => c.DrawImage(barcodeImage, new Point((int)x, (int)y), new GraphicsOptions()));
        }

        private void DrawAztecCode(CustomLabelRequest request, Image<Rgba32> image, LabelBox box, string text, float x, float y, float width, float height)
        {
            using var memoryStream = new MemoryStream();
            var barcode = Barcoder.Aztec.AztecEncoder.Encode(text);
            var renderer = new ImageRenderer();
            using var barcodeImage = renderer.Render(barcode);
            barcodeImage.Mutate(c => c.Resize((int)width, (int)height));
            image.Mutate(c => c.DrawImage(barcodeImage, new Point((int)x, (int)y), new GraphicsOptions()));
        }

        private void DrawCode128(CustomLabelRequest request, Image<Rgba32> image, LabelBox box, string text, float x, float y, float width, float height)
        {
            using var memoryStream = new MemoryStream();
            if (text.Length > 80)
                text = text.Substring(0, 80);
            var barcode = Barcoder.Code128.Code128Encoder.Encode(text);
            var renderer = new ImageRenderer();
            using var barcodeImage = renderer.Render(barcode);
            barcodeImage.Mutate(c => c.Resize((int)width, (int)height));
            image.Mutate(c => c.DrawImage(barcodeImage, new Point((int)x, (int)y), new GraphicsOptions()));
        }

        private void DrawText(CustomLabelRequest request, Image<Rgba32> image, LabelBox box, string text, float x, float y, float width, float height)
        {
            var drawingOptions = new DrawingOptions();
            var fontColor = GetColor(box.Properties.Color);
            var fontFamily = GetOrCreateFontFamily(box.Properties.Font);
            var fontSize = GetFontSize(box.Properties.FontSize);
            var font = new Font(fontFamily, fontSize, box.Properties.FontWeight != FontWeights.Normal ? FontStyle.Bold : FontStyle.Regular);
            var textOptions = new TextOptions(font)
            {
                Origin = new PointF(0, 0),
                TabWidth = 4,
                WordBreaking = WordBreaking.BreakAll,
                Dpi = request.Label.Dpi,
                KerningMode = KerningMode.None,
                LayoutMode = LayoutMode.HorizontalTopBottom,
                TextDirection = TextDirection.LeftToRight,
                //HorizontalAlignment = GetTextAlignment(box.Properties.Align),
                HorizontalAlignment = HorizontalAlignment.Left,
                WrappingLength = width
            };
            var measure = TextMeasurer.Measure(text, textOptions);
            var textX = x;
            var textY = y - (fontSize * 1.5f);
            textOptions.Origin = new PointF(textX, textY);
            if (box.Properties.Align == LabelAlign.Center)
                textOptions.Origin = new PointF(textX + (width / 2f) - (measure.Width / 2f), textY);
            else if(box.Properties.Align == LabelAlign.Right)
                textOptions.Origin = new PointF(textX + width - measure.Width, textY);

            if (box.Properties.Rotate != Rotations.Zero)
            {
                var builder = new AffineTransformBuilder()
                    .AppendRotationDegrees(GetRotation(box.Properties.Rotate))
                    .AppendTranslation(new PointF(x, y));
                // todo: this isn't correct
                switch (box.Properties.Align)
                {
                    case LabelAlign.Right:
                        textOptions.Origin =  new PointF(textX, textY);
                        break;
                    case LabelAlign.Left:
                        textOptions.Origin =  new PointF(textX, textY - ((measure.Height / 2f) + (height / 2f)));
                        break;
                    case LabelAlign.Center:
                        textOptions.Origin =  new PointF(textX, textY - ((measure.Height / 2f) + (height / 2f)));
                        break;
                }
                drawingOptions.Transform = builder.BuildMatrix(Rectangle.Round(new RectangleF(x, y, width - 1, Math.Max(height, measure.Height - (fontSize * 1.5f)) - 1)));
            }

            // draw text
            image.Mutate<Rgba32>(c => c.DrawText(drawingOptions, textOptions, text, Brushes.Solid(fontColor), Pens.Solid(fontColor, 1)));
            
            // draw text box bounds border
            if (request.Label.ShowBoundaries)
                image.Mutate<Rgba32>(c => c.Draw(Pens.DashDotDot(Color.Red, 1), new RectangleF(x, y, width - 1, Math.Max(height, measure.Height - (fontSize * 1.5f)) - 1)));
        }

        private string Encode2dBarcodePartData(Part part)
        {
            var builder = new StringBuilder();
            var rs = "\u001e"; // \u001e, \u005e, \u241e (␞)
            var gs = "\u001d"; // \u001d, \u005d, \u241d (␝)
            var eot = "\u0004"; // \u0004, ^\u0044, \u2404 (␄)
            builder.Append($"[)>{rs}06{gs}");
            builder.Append($"{part.PartNumber}");
            builder.Append($"{gs}1P{part.ManufacturerPartNumber}");
            builder.Append($"{gs}K");
            builder.Append($"{gs}{part.Description}");
            builder.Append($"{gs}{part.PartTypeId}");
            builder.Append($"{gs}{part.MountingTypeId}");
            builder.Append($"{gs}{part.PackageType}");
            builder.Append($"{gs}Q{part.Quantity}");
            builder.Append($"{gs}{part.Location}");
            builder.Append($"{gs}{part.BinNumber}");
            builder.Append($"{gs}{part.BinNumber2}");
            builder.Append($"{gs}\r");
            builder.Append($"{eot}\r");
            return builder.ToString();
        }

        private string Encode1dBarcodePartData(Part part)
        {
            return part?.PartNumber ?? part?.ManufacturerPartNumber ?? string.Empty;
        }

        private HorizontalAlignment GetTextAlignment(LabelAlign align)
        {
            switch (align)
            {
                case LabelAlign.Center:
                    return HorizontalAlignment.Center;
                default:
                case LabelAlign.Left:
                    return HorizontalAlignment.Left;
                case LabelAlign.Right:
                    return HorizontalAlignment.Right;
            }
        }

        private float GetFontSize(FontSizes fontSize)
        {
            switch (fontSize)
            {
                case FontSizes.Tiny:
                    return 6f;
                case FontSizes.Small:
                    return 10f;
                default:
                case FontSizes.Normal:
                    return 12f;
                case FontSizes.Medium:
                    return 16f;
                case FontSizes.Large:
                    return 18f;
                case FontSizes.ExtraLarge:
                    return 22f;
                case FontSizes.VeryLarge:
                    return 26f;
            }
        }

        private FontFamily GetOrCreateFontFamily(string fontName)
        {
            if (FontCollection.Value.TryGet(fontName, out var fontFamily))
            {
                return fontFamily;
            }
            else
            {
                // try to load it from system fonts
                if (SystemFonts.TryGet(fontName, out fontFamily))
                {
                    return fontFamily;
                }
            }
            // return the default font
            return _fontFamily;
        }

        private Color GetColor(LabelColors color)
        {
            switch (color)
            {
                default:
                case LabelColors.Black:
                    return Color.Black;
                case LabelColors.Blue:
                    return Color.Blue;
                case LabelColors.Gray:
                    return Color.Gray;
                case LabelColors.Green:
                    return Color.Green;
                case LabelColors.Orange:
                    return Color.Orange;
                case LabelColors.Purple:
                    return Color.Purple;
                case LabelColors.Red:
                    return Color.Red;
                case LabelColors.Yellow:
                    return Color.Yellow;
            }
        }

        private int GetRotation(Rotations rotation)
        {
            switch (rotation)
            {
                default:
                case Rotations.Zero:
                    return 0;
                case Rotations.FourtyFive:
                    return 45;
                case Rotations.Ninety:
                    return 90;
                case Rotations.OneThirtyFive:
                    return 135;
                case Rotations.OneEighty:
                    return 180;
                case Rotations.TwoTwentyFive:
                    return 225;
                case Rotations.TwoSeventy:
                    return 270;
                case Rotations.ThreeFifteen:
                    return 315;
            }
        }

        private static void LoadFonts()
        {
            // Load fonts
            if (!FontCollection.IsValueCreated)
            {
                var fontFiles = new FontScanner().GetFonts(FontsPath);
                foreach (var fontFile in fontFiles)
                {
                    if (File.Exists(fontFile))
                    {
                        try
                        {
                            FontCollection.Value.Add(fontFile);
                        }
                        catch (Exception)
                        {
                            // can't use font
                        }
                    }
                }
            }
            _fontFamily = FontCollection.Value.Get(DefaultFontName);
        }

        private Image<Rgba32> CreateLabelImage(PrinterLabel label)
        {
            var widthInPixels = InchesToPixels(label.Width, label.Dpi);
            var heightInPixels = InchesToPixels(label.Height, label.Dpi);
            // generate the label as an image
            _paperRect = new Rectangle(0, 0, widthInPixels, heightInPixels);
            var printerImage = new Image<Rgba32>(_paperRect.Width, _paperRect.Height);
            printerImage.Metadata.VerticalResolution = label.Dpi;
            printerImage.Metadata.HorizontalResolution = label.Dpi;

            return printerImage;
        }

        private int InchesToPixels(double inches, int dpi)
        {
            return (int)(inches * dpi);
        }

        private double PixelsToInches(int pixels, int dpi)
        {
            return (int)(pixels / (double)dpi);
        }
    }
}
