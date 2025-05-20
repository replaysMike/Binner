using Binner.Model.IO.Printing;
using Binner.Model.IO.Printing.PrinterHardware;
using Microsoft.Extensions.Logging;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TypeSupport.Extensions;

namespace Binner.Common.IO.Printing
{
    /// <summary>
    /// Dymo Label printer, manages generation of the print image
    /// </summary>
    public class DymoLabelPrinterHardware : ILabelPrinterHardware
    {
        public static string FontsPath = "./Fonts";
        private const string DefaultFontName = "Segoe UI";
        private const float VerticalDpi = 300;
        private const float HorizontalDpi = 600;
        private static readonly Color DefaultTextColor = Color.Black;
        private static readonly Lazy<FontCollection> FontCollection = new(() => new FontCollection());
        private ILoggerFactory _loggerFactory;
        private static FontFamily _fontFamily;
        private readonly IBarcodeGenerator _barcodeGenerator;
        private readonly List<PointF> _labelStart = new();
        private readonly IPrinterEnvironment _printer;
        private Rectangle _paperRect;
        private const float _scaleFactor = 0.75f;

        /// <summary>
        /// Printer settings
        /// </summary>
        public IPrinterSettings PrinterSettings { get; set; }

        static DymoLabelPrinterHardware()
        {
            LoadFonts();
        }

        public DymoLabelPrinterHardware(ILoggerFactory loggerFactory, IPrinterSettings printerSettings, IBarcodeGenerator barcodeGenerator)
        {
            _loggerFactory = loggerFactory;
            PrinterSettings = printerSettings ?? throw new ArgumentNullException(nameof(printerSettings));
            _barcodeGenerator = barcodeGenerator;
            _printer = new PrinterFactory(loggerFactory).CreatePrinter(printerSettings);
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

        /// <summary>
        /// Print a label image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="options"></param>
        public void PrintLabelImage(Image<Rgba32> image, PrinterOptions options)
        {
            var labelProperties = GetLabelDimensions(options.LabelName);
            _printer.PrintLabel(options, labelProperties, image);
        }

        public Image<Rgba32> PrintLabel(LabelContent content, PrinterOptions options)
        {
            // generate the print image and send to printer hardware
            if (content is null) throw new ArgumentNullException(nameof(content));
            if (options is null) throw new ArgumentNullException(nameof(options));
            var (image, labelProperties) = CreatePrinterImage(options);
            DrawLabelFromContent(image, labelProperties, content, _paperRect);

            return Print(image, labelProperties, options);
        }

        public Image<Rgba32> PrintLabel(ICollection<LineConfiguration> lines, PrinterOptions options)
        {
            // generate the print image and send to printer hardware
            if (lines is null || !lines.Any()) throw new ArgumentNullException(nameof(lines));
            if (options is null) throw new ArgumentNullException(nameof(options));
            var (image, labelProperties) = CreatePrinterImage(options);
            DrawLabelFromLines(image, labelProperties, lines, _paperRect);

            return Print(image, labelProperties, options);
        }

        private Image<Rgba32> Print(Image<Rgba32> image, LabelDefinition labelProperties, PrinterOptions options)
        {
            // for debugging label layout
            if (options.ShowDiagnostic) DrawDebug(image, labelProperties);

            if (!options.GenerateImageOnly)
                _printer.PrintLabel(options, labelProperties, image);
            return image;
        }

        private void DrawDebug(Image<Rgba32> image, LabelDefinition labelProperties)
        {
            // draw rectangle
            image.Mutate(c => c.Draw(Pens.Solid(Color.LightGray, 1), new RectangleF(0, 0, _paperRect.Width - 1, _paperRect.Height - 1)));
            var drawEveryY = _paperRect.Height / labelProperties.LabelCount;
            for (var i = 1; i < labelProperties.LabelCount; i++)
            {
                image.Mutate(c => c.DrawLine(Pens.Solid(Color.Black, 2), new PointF(0, drawEveryY * i), new PointF(_paperRect.Width, drawEveryY * i)));
            }
        }

        private (Image<Rgba32>, LabelDefinition labelProperties) CreatePrinterImage(PrinterOptions options)
        {
            var labelName = options.LabelName;
            if (string.IsNullOrWhiteSpace(labelName))
                labelName = PrinterSettings.PartLabelName;
            var labelProperties = GetLabelDimensions(labelName);

            // generate the label as an image
            _paperRect = new Rectangle(0, 0, labelProperties.ImageDimensions.Width - 90, labelProperties.ImageDimensions.Height);
            // for each physical label, calculate where it's Y start position will be in the image
            for (var i = 1; i <= labelProperties.LabelCount; i++)
                _labelStart.Add(new PointF(0, labelProperties.TopMargin + _paperRect.Height - (_paperRect.Height / i)));

            var printerImage = new Image<Rgba32>(_paperRect.Width, _paperRect.Height);
            printerImage.Metadata.VerticalResolution = labelProperties.Dpi;
            printerImage.Metadata.HorizontalResolution = labelProperties.HorizontalDpi;

            return (printerImage, labelProperties);
        }

        private void DrawLabelFromLines(Image<Rgba32> image, LabelDefinition labelProperties, ICollection<LineConfiguration> lines, Rectangle paperRect)
        {
            var margins = new Margin(0, 0, 0, 0);
            var lastLinePosition = new List<PointF>();
            var prevLine = _labelStart.Last();
            foreach (var line in lines)
            {
                prevLine = DrawLine(image, labelProperties, prevLine, null, line.Content, line, paperRect, margins);
            }
        }

        private void DrawLabelFromContent(Image<Rgba32> image, LabelDefinition labelProperties, LabelContent content, Rectangle paperRect)
        {
            var rightMargin = 0;
            var leftMargin = 0;
            // allow vertical binNumber to be written, if provided
            if (!string.IsNullOrEmpty(PrinterSettings.PartLabelTemplate.Identifier?.Content) && PrinterSettings.PartLabelTemplate.Identifier?.Position == LabelPosition.Right)
                rightMargin = 5;
            if (!string.IsNullOrEmpty(PrinterSettings.PartLabelTemplate.Identifier?.Content) && PrinterSettings.PartLabelTemplate.Identifier?.Position == LabelPosition.Left)
                leftMargin = 5;
            var margins = new Margin(leftMargin, rightMargin, 0, 0);

            // process template values
            content.Line1 = content.Line1 ?? ReplaceTemplate(content.Part, PrinterSettings.PartLabelTemplate.Line1);
            content.Line2 = content.Line2 ?? ReplaceTemplate(content.Part, PrinterSettings.PartLabelTemplate.Line2);
            content.Line3 = content.Line3 ?? ReplaceTemplate(content.Part, PrinterSettings.PartLabelTemplate.Line3);
            content.Line4 = content.Line4 ?? ReplaceTemplate(content.Part, PrinterSettings.PartLabelTemplate.Line4);
            content.Identifier = content.Identifier ?? ReplaceTemplate(content.Part, PrinterSettings.PartLabelTemplate.Identifier);
            content.Identifier2 = content.Identifier2 ?? ReplaceTemplate(content.Part, PrinterSettings.PartLabelTemplate.Identifier2);

            // merge any adjacent template lines together
            MergeLines(PrinterSettings.PartLabelTemplate, content, paperRect, margins);
            var index = PrinterSettings.PartLabelTemplate.Line1?.Label ?? 1;
            var line0Position = _labelStart[index - 1];
            var line1Position = DrawLine(image, labelProperties, line0Position, content.Part, content.Line1, PrinterSettings.PartLabelTemplate.Line1, paperRect, margins);
            var line2Position = DrawLine(image, labelProperties, line1Position, content.Part, content.Line2, PrinterSettings.PartLabelTemplate?.Line2, paperRect, margins);
            var line3Position = DrawLine(image, labelProperties, line2Position, content.Part, content.Line3, PrinterSettings.PartLabelTemplate?.Line3, paperRect, margins);
            var line4Position = DrawLine(image, labelProperties, line3Position, content.Part, content.Line4, PrinterSettings.PartLabelTemplate?.Line4, paperRect, margins);

            var identifierPosition = DrawLine(image, labelProperties, line4Position, content.Part, content.Identifier, PrinterSettings.PartLabelTemplate?.Identifier, paperRect, margins);
            var identifierPosition2 = DrawLine(image, labelProperties, identifierPosition, content.Part, content.Identifier2, PrinterSettings.PartLabelTemplate?.Identifier2, paperRect, margins);
        }

        /// <summary>
        /// Merge all adjacent template lines
        /// </summary>
        /// <param name="template"></param>
        /// <param name="content"></param>
        /// <param name="paperRect"></param>
        /// <param name="margins"></param>
        private void MergeLines(PartLabelTemplate template, LabelContent content, Rectangle paperRect, Margin margins)
        {
            if (template.Line1?.Content == template.Line2?.Content)
            {
                MergeLines(content.Line1, content.Line2, paperRect, margins, out var newLine, out var newLine2);
                content.Line1 = newLine;
                content.Line2 = newLine2;
            }
            if (template.Line2?.Content == template.Line3?.Content)
            {
                content.Line2 = content.Line2.Replace("\n", "").Replace("\r", "");
                MergeLines(content.Line2, content.Line3, paperRect, margins, out var newLine, out var newLine2);
                content.Line2 = newLine;
                content.Line3 = newLine2;
            }
            if (template.Line3?.Content == template.Line4?.Content)
            {
                MergeLines(content.Line3, content.Line4, paperRect, margins, out var newLine, out var newLine2);
                content.Line3 = newLine;
                content.Line4 = newLine2;
            }
        }

        /// <summary>
        /// Merge two adjascent content lines
        /// </summary>
        /// <param name="firstLine"></param>
        /// <param name="secondLine"></param>
        /// <param name="paperRect"></param>
        /// <param name="margins"></param>
        /// <param name="newFirstLine"></param>
        /// <param name="newSecondLine"></param>
        private void MergeLines(string? firstLine, string? secondLine, Rectangle paperRect, Margin margins, out string newFirstLine, out string newSecondLine)
        {
            var fontFirstLine = CreateFont(PrinterSettings.PartLabelTemplate.Line2, firstLine, paperRect);
            var fontSecondLine = CreateFont(PrinterSettings.PartLabelTemplate.Line3, secondLine, paperRect);
            var line1 = firstLine?.ToString() ?? string.Empty;
            var line2 = string.Empty;
            // merge lines and use the second line to wrap
            FontRectangle len;
            var description = line1?.Trim() ?? string.Empty;
            line1 = description.ToString();
            // autowrap line 2
            do
            {
                len = TextMeasurer.MeasureSize(line1, new TextOptions(fontFirstLine) { Dpi = HorizontalDpi } );
                if (len.Width > paperRect.Width - margins.Right - margins.Left)
                    line1 = line1.Substring(0, line1.Length - 1);
            } while (len.Width > paperRect.Width - margins.Right - margins.Left);
            if (line1.Length < description.Length)
            {
                // autowrap line 3
                line2 = description.Substring(line1.Length, description.Length - line1.Length).Trim();
                do
                {
                    len = TextMeasurer.MeasureSize(line2, new TextOptions(fontSecondLine) { Dpi = HorizontalDpi });
                    if (len.Width > paperRect.Width - margins.Right - margins.Left)
                        line2 = line2.Substring(0, line2.Length - 1);
                } while (len.Width > paperRect.Width - margins.Right - margins.Left);
            }
            // overwrite line2 and line3 with new values
            newFirstLine = line1;
            newSecondLine = line2;
        }

        private PointF DrawLine(Image<Rgba32> image, LabelDefinition labelProperties, PointF lineOffset, object? part, string? text, LineConfiguration? template, Rectangle paperRect, Margin margins)
        {
            if (string.IsNullOrWhiteSpace(text) || template == null)
                return new PointF(0, lineOffset.Y);
            var font = CreateFont(template, text, paperRect);
            
            var fontColor = string.IsNullOrEmpty(template.Color) ? DefaultTextColor : template.Color.StartsWith("#") ? Color.ParseHex(template.Color) : Color.Parse(template.Color);
            var textOptions = new RichTextOptions(font) { Dpi = HorizontalDpi, KerningMode = KerningMode.Auto };
            var textBounds = TextMeasurer.MeasureSize(text, textOptions);
            var x = 0f;
            var y = lineOffset.Y;
            x += template.Margin.Left;
            y += template.Margin.Top;
            if (template.Barcode == true)
            {
                x = template.Margin.Left;
                y += 12;
                DrawBarcode128(image, fontColor, Color.White, text, new Rectangle((int)x, (int)y, (int)(paperRect.Width - x), paperRect.Height), (int)((template.FontSize ?? 6) * 10f));
            }
            else
            {

                if (template.Rotate > 0)
                {
                    switch (template.Position)
                    {
                        case LabelPosition.Right:
                            x += (margins.Left + paperRect.Width - margins.Right) - (textBounds.Height + labelProperties.LeftMargin) - 25;
                            break;
                        case LabelPosition.Left:
                            x += margins.Left + labelProperties.LeftMargin;
                            break;
                        case LabelPosition.Center:
                            x += (margins.Left + paperRect.Width - margins.Right) / 2 - (textBounds.Height / 2 + labelProperties.LeftMargin);
                            break;
                    }
                    // rotated labels will start at the top of the label
                    y = _labelStart[template.Label - 1].Y + template.Margin.Top;

                    // https://github.com/SixLabors/ImageSharp.Drawing/discussions/190
                    // rotating text correctly in ImageSharp turned out to be beyond trivial
                    var builder = new AffineTransformBuilder()
                        .AppendRotationDegrees(PrinterSettings.PartLabelTemplate?.Identifier?.Rotate ?? 0)
                        .AppendTranslation(new PointF(x, y));
                    var drawingOptions = new DrawingOptions
                    {
                        Transform = builder.BuildMatrix(Rectangle.Round(new RectangleF(textBounds.X, textBounds.Y, textBounds.Width, textBounds.Height)))
                    };
                    var textOptions2 = new RichTextOptions(font)
                    {
                        Origin = new PointF(0, 0),
                        Dpi = HorizontalDpi,
                        KerningMode = KerningMode.Standard,
                    };
                    image.Mutate(c => c.DrawText(drawingOptions, textOptions2, text, Brushes.Solid(fontColor), null));
                }
                else
                {
                    switch (template.Position)
                    {
                        case LabelPosition.Right:
                            x += (margins.Left + paperRect.Width - margins.Right) - (textBounds.Width + labelProperties.LeftMargin);
                            break;
                        case LabelPosition.Left:
                            x += margins.Left + labelProperties.LeftMargin;
                            break;
                        case LabelPosition.Center:
                            x += (margins.Left + paperRect.Width - margins.Right) / 2 - (textBounds.Width / 2 + labelProperties.LeftMargin);
                            break;
                    }
                    var drawingOptions = new DrawingOptions();
                    var textOptions2 = new RichTextOptions(font)
                    {
                        Origin = new PointF(x, y),
                        Dpi = HorizontalDpi,
                        KerningMode = KerningMode.Standard
                    };
                    image.Mutate(c => c.DrawText(drawingOptions, textOptions2, text, Brushes.Solid(fontColor), null));
                }
            }
            // return the new drawing cursor position
            return new PointF(0, y + textBounds.Height);
        }

        private Font CreateFont(LineConfiguration? template, string? lineValue, Rectangle paperRect)
        {
            Font font;
            var fontFamily = GetOrCreateFontFamily(template?.FontName ?? DefaultFontName);
            if (template?.AutoSize == true)
                font = AutosizeFont(fontFamily, (template?.FontSize ?? 8 - 1) * _scaleFactor, lineValue, paperRect.Width);
            else
            {
                font = new Font(fontFamily, (template?.FontSize ?? 8 - 1) * _scaleFactor);
            }
            return font;
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

        private static string ReplaceTemplate(object? data, LineConfiguration? config)
        {
            var template = config?.Content ?? string.Empty;
            var value = template;
            if (template.Contains("{") && template.Contains("}"))
            {
                var propertyName = string.Empty;
                var matches = Regex.Match(template, @"{([^}]+)}");
                if (matches.Groups.Count > 1)
                    propertyName = matches.Groups[1].Value;
                propertyName = propertyName[0].ToString().ToUpper() + propertyName.Substring(1);
                var propertyValue = data.GetPropertyValue(propertyName);
                if (propertyValue != null)
                    value = value.Replace(template, propertyValue.ToString());
                else
                    value = value.Replace(template, string.Empty);
            }
            if (config?.UpperCase == true)
                value = value.ToUpper();
            else if (config?.LowerCase == true)
                value = value.ToLower();

            return value;
        }

        private void DrawBarcode128(Image<Rgba32> image, Color foregroundColor, Color backgroundColor, string encodeValue, Rectangle rect, int barcodeHeight)
        {
            var graphicsOptions = new GraphicsOptions
            {
                Antialias = false,
            };
            var generatedBarcodeImage = _barcodeGenerator.GenerateBarcode(encodeValue, foregroundColor, backgroundColor, rect.Width - 20, Math.Max(barcodeHeight, rect.Height));
            try
            {
                image.Mutate(c => c.DrawImage(generatedBarcodeImage, new Point(0, rect.Y), graphicsOptions));
                image.Metadata.HorizontalResolution = HorizontalDpi;
                image.Metadata.VerticalResolution = VerticalDpi;
            }
            catch (Exception)
            {
                // swallow exceptions if we draw outside the bounds
            }
        }

        private Font AutosizeFont(FontFamily fontFamily, float fontSize, string? text, int maxWidth)
        {
            FontRectangle len;
            var newFontSize = fontSize;
            if (!string.IsNullOrEmpty(text))
            {
                do
                {
                    var testFont = new Font(fontFamily, DrawingUtilities.PointToPixel(newFontSize * _scaleFactor));
                    var textOptions = new TextOptions(testFont)
                    {
                        Dpi = HorizontalDpi
                    };
                    len = TextMeasurer.MeasureSize(text, textOptions);
                    if (len.Width > maxWidth)
                        newFontSize -= 0.5f;
                } while (len.Width > maxWidth);
            }

            return new Font(fontFamily, DrawingUtilities.PointToPixel(newFontSize * _scaleFactor));
        }

        private LabelDefinition GetLabelDimensions(string labelName)
        {
            var labelDefinition = PrinterSettings.LabelDefinitions
                .FirstOrDefault(x => x.MediaSize.ModelName.Equals(labelName));
            if (labelDefinition != null)
            {
                // call set dimensions to ensure we calculate all the properties correctly
                labelDefinition.UpdateDimensions();
                return labelDefinition;
            }

            throw new BinnerConfigurationException($"Unsupported label: '{labelName}'");
        }
    }
}
