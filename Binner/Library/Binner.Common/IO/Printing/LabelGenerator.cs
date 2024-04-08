using AnyBarcode;
using Barcoder.Renderer.Image;
using Binner.Common.Extensions;
using Binner.Common.Services;
using Binner.Model;
using Binner.Model.IO.Printing;
using Binner.Model.Requests;
using Binner.StorageProvider.EntityFrameworkCore;
using Newtonsoft.Json;
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
using System.Text.RegularExpressions;
using System.Web;
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
        private readonly IPartTypeService _partTypeService;

        /// <summary>
        /// Printer settings
        /// </summary>
        public IPrinterSettings PrinterSettings { get; set; }

        static LabelGenerator()
        {
            LoadFonts();
        }

        public LabelGenerator(IPrinterSettings printerSettings, IBarcodeGenerator barcodeGenerator, IPartTypeService partTypeService)
        {
            PrinterSettings = printerSettings ?? throw new ArgumentNullException(nameof(printerSettings));
            _barcodeGenerator = barcodeGenerator;
            _partTypeService = partTypeService;
            _printer = new PrinterFactory().CreatePrinter(printerSettings);
        }

        public Image<Rgba32> CreateLabelImage(Label label, Part part)
        {
            var customLabelDefinition = JsonConvert.DeserializeObject<CustomLabelDefinition>(label.Template);
            return CreateLabelImage(customLabelDefinition, part);
        }

        public Image<Rgba32> CreateLabelImage(CustomLabelDefinition labelDef, Part part)
        {
            // generate the print image and send to printer hardware
            var image = CreateImage(labelDef);
            if (image == null)
                return new Image<Rgba32>(1, 1);
            // convert the screen dpi to print
            var ratio = labelDef.Label.Dpi / 96f;
            var margins = new int [4] { 0, 0, 0, 0 };
            var marginParts = labelDef.Label.Margin?.Split(' ', StringSplitOptions.RemoveEmptyEntries)
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
            if (labelDef.Label.ShowBoundaries)
            {
                image.Mutate<Rgba32>(c => c.Draw(Pens.DashDotDot(Color.Red, 1),
                    new RectangleF(margins[3], margins[0], (image.Width - margins[1] - margins[3] - 1), (image.Height - margins[0] - margins[2] - 1))));
                // draw label border
                image.Mutate<Rgba32>(c => c.Draw(Pens.DashDotDot(Color.Blue, 1), new RectangleF(0, 0, image.Width - 1, image.Height - 1)));
            }

            foreach (var box in labelDef.Boxes)
            {
                var x = box.Left * ratio;
                var y = box.Top * ratio;
                var width = box.Width * ratio;
                var height = box.Height * ratio;

                var text = GetTextValue(box, part);

                var dataToEncode = text;
                switch (box.Name)
                {
                    case "qrCode":
                        if (string.IsNullOrEmpty(dataToEncode))
                            dataToEncode = Encode2dBarcodePartData(part);
                        DrawQrCode(labelDef, image, box, dataToEncode, x, y, width, height);
                        break;
                    case "dataMatrix2d":
                        if (string.IsNullOrEmpty(dataToEncode))
                            dataToEncode = Encode2dBarcodePartData(part);
                        DrawDataMatrix(labelDef, image, box, dataToEncode, x, y, width, height);
                        break;
                    case "pdf417":
                        if (string.IsNullOrEmpty(dataToEncode))
                            dataToEncode = Encode2dBarcodePartData(part);
                        DrawPdf417(labelDef, image, box, dataToEncode, x, y, width, height);
                        break;
                    case "aztecCode":
                        if (string.IsNullOrEmpty(dataToEncode))
                            dataToEncode = Encode2dBarcodePartData(part);
                        DrawAztecCode(labelDef, image, box, dataToEncode, x, y, width, height);
                        break;
                    case "code128":
                        if (string.IsNullOrEmpty(dataToEncode))
                            dataToEncode = Encode1dBarcodePartData(part);
                        DrawCode128(labelDef, image, box, dataToEncode, x, y, width, height);
                        break;
                    default:
                        DrawText(labelDef, image, box, text, x, y, width, height);
                        break;
                }
            }

            return image;
        }

        private string GetTextValue(LabelBox box, Part part)
        {
            var text = string.Empty;
            var propertyName = box.Name.UcFirst();
            if (part.ContainsProperty(propertyName))
            {
                switch (propertyName)
                {
                    case "Keywords":
                        if (part.Keywords?.Any() == true)
                            text = string.Join(",", part.Keywords);
                        break;
                    case "Quantity":
                        text = part.GetPropertyValue<long>(propertyName).ToString();
                        break;
                    case "Cost":
                        text = part.GetPropertyValue<double>(propertyName).ToString();
                        break;
                    case "PartId":
                        text = part.GetPropertyValue<long>(propertyName).ToString();
                        break;
                    case "PartTypeId":
                        text = part.GetPropertyValue<long>(propertyName).ToString();
                        break;
                    case "ProjectId":
                        text = part.GetPropertyValue<long?>(propertyName).ToString();
                        break;
                    default:
                        text = part.GetPropertyValue<string>(propertyName);
                        break;
                }
            }

            if (!string.IsNullOrEmpty(box.Properties.Value))
                text = box.Properties.Value;

            switch (propertyName)
            {
                case "PartType":
                    // get the part type name
                    var partType = _partTypeService.GetPartTypeAsync(part.PartTypeId).GetAwaiter().GetResult();
                    text = partType?.Name ?? string.Empty;
                    if (text.Contains("{") && text.Contains("}"))
                        text = text.Replace("{partType}", partType?.Name);
                    break;
                case "MountingType":
                    var mountingType = ((MountingType)part.MountingTypeId).ToString();
                    text = mountingType;
                    if (text.Contains("{") && text.Contains("}"))
                        text = text.Replace("{mountingType}", mountingType);
                    break;
            }

            if (!string.IsNullOrEmpty(text) && text.Contains("{") && text.Contains("}"))
            {
                var urlEncode = false;
                if (text.StartsWith("http"))
                    urlEncode = true;

                text = text.Replace("{partNumber}", Encode(part.PartNumber, urlEncode));
                text = text.Replace("{partId}", Encode(part.PartId.ToString(), urlEncode));
                text = text.Replace("{partTypeId}", Encode(part.PartTypeId.ToString(), urlEncode));
                text = text.Replace("{mfrPartNumber}", Encode(part.ManufacturerPartNumber, urlEncode));
                text = text.Replace("{description}", Encode(part.Description, urlEncode));
                text = text.Replace("{manufacturer}", Encode(part.Manufacturer, urlEncode));
                text = text.Replace("{packageType}", Encode(part.PackageType, urlEncode));
                text = text.Replace("{cost}", Encode(part.Cost.ToString(), urlEncode));
                if (part.Keywords?.Any() == true)
                    text = text.Replace("{keywords}", Encode(string.Join(",", part.Keywords), urlEncode));
                text = text.Replace("{quantity}", Encode(part.Quantity.ToString(), urlEncode));
                text = text.Replace("{digiKeyPartNumber}", Encode(part.DigiKeyPartNumber, urlEncode));
                text = text.Replace("{mouserPartNumber}", Encode(part.MouserPartNumber, urlEncode));
                text = text.Replace("{arrowNumber}", Encode(part.ArrowPartNumber, urlEncode));
                text = text.Replace("{tmeNumber}", Encode(part.TmePartNumber, urlEncode));
                text = text.Replace("{location}", Encode(part.Location, urlEncode));
                text = text.Replace("{binNumber}", Encode(part.BinNumber, urlEncode));
                text = text.Replace("{binNumber2}", Encode(part.BinNumber2, urlEncode));
                text = text.Replace("{extensionValue1}", Encode(part.ExtensionValue1, urlEncode));
                text = text.Replace("{extensionValue2}", Encode(part.ExtensionValue2, urlEncode));
                text = text.Replace("{footprintName}", Encode(part.FootprintName, urlEncode));
                text = text.Replace("{symbolName}", Encode(part.SymbolName, urlEncode));
                text = text.Replace("{projectId}", Encode(part.ProjectId.ToString(), urlEncode));
            }

            if (!string.IsNullOrEmpty(text))
            {
                text = text.Replace("\r", " ").Replace("\n", " ");
                var options = RegexOptions.None;
                var regex = new Regex("[ ]{2,}", options);     
                text = regex.Replace(text, " ");
            }

            return text;
        }

        private string? Encode(string? value, bool urlEncode)
        {
            if (urlEncode && !string.IsNullOrEmpty(value))
                return HttpUtility.UrlEncode(value);
            return value;
        }

        private void DrawQrCode(CustomLabelDefinition labelDef, Image<Rgba32> image, LabelBox box, string text, float x, float y, float width, float height)
        {
            if (width == 0 || height == 0)
                return;
            using var memoryStream = new MemoryStream();
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrCodeData);
            using var barcodeImage = qrCode.GetGraphic(20);
            var marginPerc = 0.075f;
            var marginSize = (int)(barcodeImage.Size.Width * marginPerc);

            // crop and remove the white margin around the code
            barcodeImage.Mutate(c => c.Crop(new Rectangle(marginSize, marginSize, barcodeImage.Width - marginSize - marginSize, barcodeImage.Height - marginSize - marginSize)));
            barcodeImage.Mutate(c => c.Resize((int)width, (int)height));
            image.Mutate(c => c.DrawImage(barcodeImage, new Point((int)x + 1, (int)y + 1), new GraphicsOptions()));
            //image.Mutate<Rgba32>(c => c.Draw(Pens.Solid(Color.Red, 1), new RectangleF(x, y, width - 1, height - 1)));
        }

        private void DrawDataMatrix(CustomLabelDefinition labelDef, Image<Rgba32> image, LabelBox box, string text, float x, float y, float width, float height)
        {
            if (width == 0 || height == 0)
                return;
            using var memoryStream = new MemoryStream();
            var barcode = Barcoder.DataMatrix.DataMatrixEncoder.Encode(text);
            var renderer = new ImageRenderer();
            using var barcodeImage = renderer.Render(barcode);
            var marginPerc = ((barcode.Margin - 2) / (float)barcode.Bounds.X);
            var marginSize = (int)(barcodeImage.Size.Width * marginPerc);
            // crop and remove the white margin around the code
            barcodeImage.Mutate(c => c.Crop(new Rectangle(marginSize, marginSize, barcodeImage.Width - marginSize - marginSize, barcodeImage.Height - marginSize - marginSize)));
            barcodeImage.Mutate(c => c.Resize((int)width, (int)height));
            image.Mutate(c => c.DrawImage(barcodeImage, new Point((int)x + 1, (int)y + 1), new GraphicsOptions()));
            //image.Mutate<Rgba32>(c => c.Draw(Pens.Solid(Color.Red, 1), new RectangleF(x, y, width - 1, height - 1)));
        }

        private void DrawPdf417(CustomLabelDefinition labelDef, Image<Rgba32> image, LabelBox box, string text, float x, float y, float width, float height)
        {
            if (width == 0 || height == 0)
                return;
            using var memoryStream = new MemoryStream();
            var barcode = Barcoder.Pdf417.Pdf417Encoder.Encode(text, 2);
            var renderer = new ImageRenderer();
            using var barcodeImage = renderer.Render(barcode);
            barcodeImage.Mutate(c => c.Resize((int)width, (int)height));
            image.Mutate(c => c.DrawImage(barcodeImage, new Point((int)x, (int)y), new GraphicsOptions()));
            //image.Mutate<Rgba32>(c => c.Draw(Pens.Solid(Color.Red, 1), new RectangleF(x, y, width - 1, height - 1)));
        }

        private void DrawAztecCode(CustomLabelDefinition labelDef, Image<Rgba32> image, LabelBox box, string text, float x, float y, float width, float height)
        {
            if (width == 0 || height == 0)
                return;
            using var memoryStream = new MemoryStream();
            var barcode = Barcoder.Aztec.AztecEncoder.Encode(text);
            var renderer = new ImageRenderer();
            using var barcodeImage = renderer.Render(barcode);
            var marginPerc = ((barcode.Margin - 2) / (float)barcode.Bounds.X);
            var marginSize = (int)(barcodeImage.Size.Width * marginPerc);
            // crop and remove the white margin around the code
            barcodeImage.Mutate(c => c.Crop(new Rectangle(marginSize, marginSize, barcodeImage.Width - marginSize - marginSize, barcodeImage.Height - marginSize - marginSize)));
            barcodeImage.Mutate(c => c.Resize((int)width, (int)height));
            image.Mutate(c => c.DrawImage(barcodeImage, new Point((int)x + 1, (int)y + 1), new GraphicsOptions()));
            //image.Mutate<Rgba32>(c => c.Draw(Pens.Solid(Color.Red, 1), new RectangleF(x, y, width - 1, height - 1)));
        }

        private void DrawCode128(CustomLabelDefinition labelDef, Image<Rgba32> image, LabelBox box, string text, float x, float y, float width, float height)
        {
            if (width == 0 || height == 0)
                return;
            using var memoryStream = new MemoryStream();
            if (text.Length > 80)
                throw new InvalidOperationException("Max data encoding length is 80 characters.");
            var barcode = Barcoder.Code128.Code128Encoder.Encode(text);
            var renderer = new ImageRenderer();
            using var barcodeImage = renderer.Render(barcode);
            barcodeImage.Mutate(c => c.Resize((int)width, (int)height));
            image.Mutate(c => c.DrawImage(barcodeImage, new Point((int)x, (int)y), new GraphicsOptions()));
        }

        private void DrawText(CustomLabelDefinition labelDef, Image<Rgba32> image, LabelBox box, string text, float x, float y, float width, float height)
        {
            if (width == 0 || height == 0 || string.IsNullOrEmpty(text))
                return;
            var drawingOptions = new DrawingOptions();
            var fontColor = GetColor(box.Properties.Color);
            var fontFamily = GetOrCreateFontFamily(box.Properties.Font);
            var fontSize = GetFontSize(box.Properties.FontSize);
            var font = new Font(fontFamily, fontSize, box.Properties.FontWeight != FontWeights.Normal ? FontStyle.Bold : FontStyle.Regular);
            var textOptions = new TextOptions(font)
            {
                Origin = new PointF(0, 0),
                TabWidth = 4,
                Dpi = labelDef.Label.Dpi,
                WordBreaking = WordBreaking.Normal,
                KerningMode = KerningMode.None,
                WrappingLength = width,
            };

            var measure = TextMeasurer.Measure(text, textOptions);
            while (measure.Height - (fontSize * 1.5f) > height)
            {
                if (fontSize > 5.0f)
                {
                    // text is too big for the bounding box
                    fontSize -= 0.5f;
                }
                else
                {
                    // font scale can't be reduced, trim the text until it fits
                    text = text.Substring(0, text.Length - 1);
                    // do a sanity check and break from the loop otherwise
                    if (text.Length <= 3)
                        break;
                }

                font = new Font(fontFamily, fontSize, box.Properties.FontWeight != FontWeights.Normal ? FontStyle.Bold : FontStyle.Regular);
                textOptions = new TextOptions(font)
                {
                    Origin = new PointF(0, 0),
                    TabWidth = 4,
                    Dpi = labelDef.Label.Dpi,
                    WordBreaking = WordBreaking.Normal,
                    KerningMode = KerningMode.None,
                    WrappingLength = width,
                };
                measure = TextMeasurer.Measure(text, textOptions);
            }

            var textX = x;
            var textY = y - (fontSize * 1.5f);
            textOptions.Origin = new PointF(textX, textY);

            /*
             // text alignment doesn't seem to work: https://github.com/SixLabors/ImageSharp.Drawing/issues/275
            switch (box.Properties.Align)
            {
                case LabelAlign.Left:
                    textOptions.TextAlignment = TextAlignment.Start;
                    break;
                case LabelAlign.Center:
                    textOptions.TextAlignment = TextAlignment.Center;
                    break;
                case LabelAlign.Right:
                    textOptions.TextAlignment = TextAlignment.End;
                    break;
            }*/

            // manually align text since above doesn't work
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
            if (labelDef.Label.ShowBoundaries)
                image.Mutate<Rgba32>(c => c.Draw(Pens.DashDotDot(Color.Red, 1), new RectangleF(x, y, width - 1, height - 1)));
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
                    return 7f;
                default:
                case FontSizes.Normal:
                    return 8f;
                case FontSizes.Medium:
                    return 10f;
                case FontSizes.Large:
                    return 12f;
                case FontSizes.ExtraLarge:
                    return 14f;
                case FontSizes.VeryLarge:
                    return 28f;
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

        private Image<Rgba32>? CreateImage(CustomLabelDefinition labelDef)
        {
            if (labelDef.Label.Width == 0)
                labelDef.Label.Width = 1;
            if (labelDef.Label.Height == 0)
                labelDef.Label.Height = 1;
            var widthInPixels = InchesToPixels(labelDef.Label.Width, labelDef.Label.Dpi);
            var heightInPixels = InchesToPixels(labelDef.Label.Height, labelDef.Label.Dpi);
            // generate the label as an image
            _paperRect = new Rectangle(0, 0, widthInPixels, heightInPixels);
            if (_paperRect.Width > 0 && _paperRect.Height > 0)
            {
                var printerImage = new Image<Rgba32>(_paperRect.Width, _paperRect.Height);
                printerImage.Metadata.VerticalResolution = labelDef.Label.Dpi;
                printerImage.Metadata.HorizontalResolution = labelDef.Label.Dpi;

                return printerImage;
            }

            return null;
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
