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
        private const string DefaultFontName = "Segoe UI";
        private static readonly Color DefaultTextColor = Color.Black;
        private const float Dpi = 300;
        private const string FontsPath = "./Fonts";
        private static Lazy<FontCollection> _fontCollection = new Lazy<FontCollection>(() => new FontCollection());
        private static FontFamily _fontFamily;
        private readonly IBarcodeGenerator _barcodeGenerator;
        private readonly List<PointF> _labelStart = new();
        private readonly IPrinterEnvironment _printer;
        private Rectangle _paperRect;

        /// <summary>
        /// Printer settings
        /// </summary>
        public IPrinterSettings PrinterSettings { get; set; }

        static DymoLabelPrinterHardware()
        {
            LoadFonts();
        }

        public DymoLabelPrinterHardware(IPrinterSettings printerSettings, IBarcodeGenerator barcodeGenerator)
        {
            PrinterSettings = printerSettings ?? throw new ArgumentNullException(nameof(printerSettings));
            _barcodeGenerator = barcodeGenerator;
            _printer = new PrinterFactory().CreatePrinter(printerSettings);
        }

        private static void LoadFonts()
        {
            // Load fonts
            if (!_fontCollection.IsValueCreated)
            {
                var fontFiles = new FontScanner().GetFonts(FontsPath);
                foreach (var fontFile in fontFiles)
                {
                    if (File.Exists(fontFile))
                    {
                        try
                        {
                            _fontCollection.Value.Install(fontFile);
                        }
                        catch (Exception)
                        {
                            // can't use font
                        }
                    }
                }
            }
            _fontFamily = _fontCollection.Value.Find(DefaultFontName);
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
                image.Mutate(c => c.DrawLines(Pens.Solid(Color.Black, 2), new PointF(0, drawEveryY * i), new PointF(_paperRect.Width, drawEveryY * i)));
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
            printerImage.Metadata.VerticalResolution = Dpi;
            printerImage.Metadata.HorizontalResolution = Dpi;

            return (printerImage, labelProperties);
        }

        private void DrawLabelFromLines(Image<Rgba32> image, LabelDefinition labelProperties, ICollection<LineConfiguration> lines, Rectangle paperRect)
        {
            var margins = new Margin(0, 0, 0, 0);
            var lastLinePosition = new List<PointF>();
            for (var i = 0; i < labelProperties.LabelCount; i++)
                lastLinePosition.Add(_labelStart[i]);
            foreach (var line in lines)
            {
                lastLinePosition[line.Label - 1] = DrawLine(image, labelProperties, lastLinePosition[line.Label - 1], null, line.Content, line, paperRect, margins);
            }
        }

        private void DrawLabelFromContent(Image<Rgba32> image, LabelDefinition labelProperties, LabelContent content, Rectangle paperRect)
        {
            var rightMargin = 0;
            var leftMargin = 0;
            // allow vertical binNumber to be written, if provided
            if (!string.IsNullOrEmpty(PrinterSettings.PartLabelTemplate.Identifier.Content) && PrinterSettings.PartLabelTemplate.Identifier.Position == LabelPosition.Right)
                rightMargin = 5;
            if (!string.IsNullOrEmpty(PrinterSettings.PartLabelTemplate.Identifier.Content) && PrinterSettings.PartLabelTemplate.Identifier.Position == LabelPosition.Left)
                leftMargin = 5;
            var margins = new Margin(leftMargin, rightMargin, 0, 0);

            // process template values
            content.Line1 = content.Line1 ?? ReplaceTemplate(content.Part, PrinterSettings.PartLabelTemplate.Line1);
            content.Line2 = content.Line2 ?? ReplaceTemplate(content.Part, PrinterSettings.PartLabelTemplate.Line2);
            content.Line3 = content.Line3 ?? ReplaceTemplate(content.Part, PrinterSettings.PartLabelTemplate.Line3);
            content.Line4 = content.Line4 ?? ReplaceTemplate(content.Part, PrinterSettings.PartLabelTemplate.Line4);
            content.Identifier = content.Identifier ?? ReplaceTemplate(content.Part, PrinterSettings.PartLabelTemplate.Identifier);
            content.Identifier2 = content.Identifier2 ?? ReplaceTemplate(content.Part, PrinterSettings.PartLabelTemplate.Identifier2);

            // merge any adjascent template lines together
            MergeLines(PrinterSettings.PartLabelTemplate, content, paperRect, margins);
            var line1Position = DrawLine(image, labelProperties, new PointF(_labelStart[PrinterSettings.PartLabelTemplate.Line1.Label - 1].X, _labelStart[PrinterSettings.PartLabelTemplate.Line1.Label - 1].Y), content.Part, content.Line1, PrinterSettings.PartLabelTemplate.Line1, paperRect, margins);
            var line2Position = DrawLine(image, labelProperties, line1Position, content.Part, content.Line2, PrinterSettings.PartLabelTemplate.Line2, paperRect, margins);
            var line3Position = DrawLine(image, labelProperties, line2Position, content.Part, content.Line3, PrinterSettings.PartLabelTemplate.Line3, paperRect, margins);
            var line4Position = DrawLine(image, labelProperties, line3Position, content.Part, content.Line4, PrinterSettings.PartLabelTemplate.Line4, paperRect, margins);

            var identifierPosition = DrawLine(image, labelProperties, line4Position, content.Part, content.Identifier, PrinterSettings.PartLabelTemplate.Identifier, paperRect, margins);
            var identifierPosition2 = DrawLine(image, labelProperties, identifierPosition, content.Part, content.Identifier2, PrinterSettings.PartLabelTemplate.Identifier2, paperRect, margins);
        }

        /// <summary>
        /// Merge all adjascent template lines
        /// </summary>
        /// <param name="template"></param>
        /// <param name="content"></param>
        /// <param name="paperRect"></param>
        /// <param name="margins"></param>
        private void MergeLines(PartLabelTemplate template, LabelContent content, Rectangle paperRect, Margin margins)
        {
            if (template.Line1.Content == template.Line2.Content)
            {
                MergeLines(content.Line1, content.Line2, paperRect, margins, out var newLine, out var newLine2);
                content.Line1 = newLine;
                content.Line2 = newLine2;
            }
            if (template.Line2.Content == template.Line3.Content)
            {
                MergeLines(content.Line2, content.Line3, paperRect, margins, out var newLine, out var newLine2);
                content.Line2 = newLine;
                content.Line3 = newLine2;
            }
            if (template.Line3.Content == template.Line4.Content)
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
        private void MergeLines(string firstLine, string secondLine, Rectangle paperRect, Margin margins, out string newFirstLine, out string newSecondLine)
        {
            var fontFirstLine = CreateFont(PrinterSettings.PartLabelTemplate.Line2, firstLine, paperRect);
            var fontSecondLine = CreateFont(PrinterSettings.PartLabelTemplate.Line3, secondLine, paperRect);
            var line1 = firstLine.ToString();
            string line2;
            // merge lines and use the second line to wrap
            FontRectangle len;
            var description = line1?.Trim() ?? "";
            line1 = description.ToString();
            line2 = "";
            // autowrap line 2
            do
            {
                len = TextMeasurer.Measure(line1, new RendererOptions(fontFirstLine));
                if (len.Width > paperRect.Width - margins.Right - margins.Left)
                    line1 = line1.Substring(0, line1.Length - 1);
            } while (len.Width > paperRect.Width - margins.Right - margins.Left);
            if (line1.Length < description.Length)
            {
                // autowrap line 3
                line2 = description.Substring(line1.Length, description.Length - line1.Length).Trim();
                do
                {
                    len = TextMeasurer.Measure(line2, new RendererOptions(fontSecondLine));
                    if (len.Width > paperRect.Width - margins.Right - margins.Left)
                        line2 = line2.Substring(0, line2.Length - 1);
                } while (len.Width > paperRect.Width - margins.Right - margins.Left);
            }
            // overwrite line2 and line3 with new values
            newFirstLine = line1;
            newSecondLine = line2;
        }

        private PointF DrawLine(Image<Rgba32> image, LabelDefinition labelProperties, PointF lineOffset, object part, string text, LineConfiguration template, Rectangle paperRect, Margin margins)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new PointF(0, lineOffset.Y);
            var font = CreateFont(template, text, paperRect);
            var fontColor = string.IsNullOrEmpty(template.Color) ? DefaultTextColor : template.Color.StartsWith("#") ? Color.ParseHex(template.Color) : Color.Parse(template.Color);
            var rendererOptions = new RendererOptions(font, Dpi);
            var textBounds = TextMeasurer.Measure(text, rendererOptions);
            var x = 0f;
            var y = lineOffset.Y;
            x += template.Margin.Left;
            y += template.Margin.Top;
            if (template.Barcode)
            {
                x = 0;
                y += 12;
                DrawBarcode128(image, fontColor, Color.White, text, new Rectangle((int)x, (int)y, paperRect.Width, paperRect.Height));
            }
            else
            {
                switch (template.Position)
                {
                    case LabelPosition.Right:
                        x += (margins.Left + paperRect.Width - margins.Right) - textBounds.Width + labelProperties.LeftMargin;
                        break;
                    case LabelPosition.Left:
                        x += margins.Left + labelProperties.LeftMargin;
                        break;
                    case LabelPosition.Center:
                        x += (margins.Left + paperRect.Width - margins.Right) / 2 - textBounds.Width / 2 + labelProperties.LeftMargin;
                        break;
                }
                if (template.Rotate > 0)
                {
                    // rotated labels will start at the top of the label
                    y = _labelStart[template.Label - 1].Y + template.Margin.Top;

                    // https://github.com/SixLabors/ImageSharp.Drawing/discussions/190
                    // rotating text correctly in ImageSharp turned out to be beyond trivial
                    var builder = new AffineTransformBuilder()
                        .AppendRotationDegrees(PrinterSettings.PartLabelTemplate.Identifier.Rotate)
                        .AppendTranslation(new PointF(x, y));
                    var drawingOptions = new DrawingOptions
                    {
                        TextOptions = new TextOptions
                        {
                            ApplyKerning = true,
                            DpiX = Dpi,
                            DpiY = Dpi
                        },
                        Transform = builder.BuildMatrix(Rectangle.Round(new RectangleF(textBounds.X, textBounds.Y, textBounds.Width, textBounds.Height)))
                    };
                    image.Mutate(c => c.DrawText(drawingOptions, text, font, fontColor, new PointF(0, 0)));
                }
                else
                {
                    var drawingOptions = new DrawingOptions
                    {
                        TextOptions = new TextOptions
                        {
                            ApplyKerning = true,
                            DpiX = Dpi,
                            DpiY = Dpi
                        },
                    };
                    image.Mutate(c => c.DrawText(drawingOptions, text, font, fontColor, new PointF(x, y)));
                }
            }
            // return the new drawing cursor position
            return new PointF(0, y + textBounds.Height);
        }

        private Font CreateFont(LineConfiguration template, string lineValue, Rectangle paperRect)
        {
            Font font;
            var fontFamily = GetOrCreateFontFamily(template.Font ?? DefaultFontName);
            if (template.AutoSize)
                font = AutosizeFont(fontFamily, template.FontSize, lineValue, paperRect.Width);
            else
            {
                font = new Font(fontFamily, template.FontSize);
            }
            return font;
        }

        private FontFamily GetOrCreateFontFamily(string fontName)
        {
            FontFamily fontFamily;
            if (_fontCollection.Value.TryFind(fontName, out fontFamily))
            {
                return fontFamily;
            }
            else
            {
                // try to load it from system fonts
                if (SystemFonts.TryFind(fontName, out fontFamily))
                {
                    return fontFamily;
                }
            }
            // return the default font
            return _fontFamily;
        }

        private static string ReplaceTemplate(object data, LineConfiguration config)
        {
            var template = config.Content;
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
            if (config.UpperCase)
                value = value.ToUpper();
            else if (config.LowerCase)
                value = value.ToLower();

            return value;
        }

        private void DrawBarcode128(Image<Rgba32> image, Color foregroundColor, Color backgroundColor, string encodeValue, Rectangle rect)
        {
            var graphicsOptions = new GraphicsOptions
            {
                Antialias = false,
            };
            var generatedBarcodeImage = _barcodeGenerator.GenerateBarcode(encodeValue, foregroundColor, backgroundColor, rect.Width, 25);
            try
            {
                image.Mutate(c => c.DrawImage(generatedBarcodeImage, new Point(0, rect.Y), graphicsOptions));
                image.Metadata.HorizontalResolution = Dpi;
                image.Metadata.VerticalResolution = Dpi;
            }
            catch (Exception)
            {
                // swallow exceptions if we draw outside the bounds
            }
        }

        private Font AutosizeFont(FontFamily fontFamily, float fontSize, string text, int maxWidth)
        {
            FontRectangle len;
            var newFontSize = fontSize;
            do
            {
                var testFont = new Font(fontFamily, DrawingUtilities.PointToPixel(newFontSize));
                var rendererOptions = new RendererOptions(testFont)
                {
                    DpiX = Dpi,
                    DpiY = Dpi
                };
                len = TextMeasurer.Measure(text, rendererOptions);
                if (len.Width > maxWidth)
                    newFontSize -= 0.5f;
            } while (len.Width > maxWidth);
            return new Font(fontFamily, DrawingUtilities.PointToPixel(newFontSize));
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
