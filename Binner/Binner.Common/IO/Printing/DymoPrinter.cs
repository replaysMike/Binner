using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text.RegularExpressions;
using TypeSupport.Extensions;

namespace Binner.Common.IO.Printing
{
    /// <summary>
    /// Dymo Label printer
    /// </summary>
    public class DymoPrinter : ILabelPrinter
    {
        private const string DefaultFontName = "Segoe UI";
        private IBarcodeGenerator _barcodeGenerator;
        private Image _printImage;
        private List<PointF> _labelStart = new List<PointF>();
        private Rectangle _paperRect = new Rectangle();
        private LabelProperties _labelProperties { get; set; }

        /// <summary>
        /// Printer settings
        /// </summary>
        public IPrinterSettings PrinterSettings { get; set; }

        public DymoPrinter(IPrinterSettings printerSettings, IBarcodeGenerator barcodeGenerator)
        {
            PrinterSettings = printerSettings ?? throw new ArgumentNullException(nameof(printerSettings));
            _barcodeGenerator = barcodeGenerator;
        }

        public Image PrintLabel(LabelContent content)
        {
            return PrintLabel(content, PrinterOptions.None);
        }

        public Image PrintLabel(LabelContent content, PrinterOptions options)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            _labelProperties = GetLabelDimensions(PrinterSettings.LabelName);
            var doc = CreatePrinterDocument(options);
            var g = Graphics.FromImage(_printImage);
            DrawLabel(g, content, _paperRect);

            // for debugging label layout
            if (options.ShowDiagnostic) DrawDebug(g);

            if (!options.GenerateImageOnly)
                doc.Print();
            return _printImage;
        }

        public Image PrintLabel(ICollection<LineConfiguration> lines, PrinterOptions options)
        {
            if (lines == null || !lines.Any()) throw new ArgumentNullException(nameof(lines));
            _labelProperties = GetLabelDimensions(options.LabelName);
            var doc = CreatePrinterDocument(options);
            var g = Graphics.FromImage(_printImage);
            DrawLabel(g, lines, _paperRect);

            // for debugging label layout
            if(options.ShowDiagnostic) DrawDebug(g);

            if (!options.GenerateImageOnly)
                doc.Print();
            return _printImage;
        }

        private void DrawDebug(Graphics g)
        {
            g.DrawRectangle(Pens.LightGray, 0, 0, _paperRect.Width - 1, _paperRect.Height - 1);
            var drawEveryY = _paperRect.Height / _labelProperties.LabelCount;
            for (var i = 1; i < _labelProperties.LabelCount; i++)
                g.DrawLine(new Pen(Color.Black, 2), 0, drawEveryY * i, _paperRect.Width, drawEveryY * i);
        }

        private PrintDocument CreatePrinterDocument(PrinterOptions options)
        {
            var doc = new PrintDocument();
            SetPrinterFromSettings(doc);
            doc.PrintPage += T_PrintPage;
            doc.DefaultPageSettings.Landscape = true;
            doc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            foreach (PaperSource paperSource in doc.PrinterSettings.PaperSources)
            {
                if (paperSource.SourceName.Equals(GetSourceName(options.LabelSource ?? PrinterSettings.LabelSource), StringComparison.CurrentCultureIgnoreCase))
                {
                    doc.DefaultPageSettings.PaperSource = paperSource;
                    break;
                }
            }
            doc.DefaultPageSettings.PaperSize = new PaperSize(_labelProperties.LabelName, _labelProperties.Dimensions.Width, _labelProperties.Dimensions.Height * _labelProperties.LabelCount);
            // generate the label as an image
            _paperRect = new Rectangle(0, 0, doc.DefaultPageSettings.PaperSize.Width, doc.DefaultPageSettings.PaperSize.Height);
            for(var i = 1; i <= _labelProperties.LabelCount; i++)
                _labelStart.Add(new PointF(0, _labelProperties.TopMargin + _paperRect.Height - (_paperRect.Height / i)));
            var printImage = new Bitmap(_paperRect.Width, _paperRect.Height);
            printImage.SetResolution(doc.DefaultPageSettings.PrinterResolution.X, doc.DefaultPageSettings.PrinterResolution.Y);
            _printImage = printImage;
            return doc;
        }

        private void SetPrinterFromSettings(PrintDocument t)
        {
            if (!string.IsNullOrEmpty(PrinterSettings.PrinterName))
            {
                var installedPrinters = System.Drawing.Printing.PrinterSettings.InstalledPrinters.Cast<string>().ToList();
                var foundPrinter = installedPrinters.FirstOrDefault(x => x.Equals(PrinterSettings.PrinterName, StringComparison.CurrentCultureIgnoreCase));
                if (!string.IsNullOrEmpty(foundPrinter))
                    t.PrinterSettings.PrinterName = foundPrinter;
                else
                    throw new Exception($"Could not locate printer named: '{PrinterSettings.PrinterName}' - the following printers are available: {string.Join(",", installedPrinters)}");
            }
        }

        private string GetSourceName(LabelSource labelSource)
        {
            switch (labelSource)
            {
                case LabelSource.Default:
                    return "Automatically Select";
                case LabelSource.Left:
                    return "Left Roll";
                case LabelSource.Right:
                    return "Right Roll";
            }
            throw new InvalidOperationException($"Unknown label source: {labelSource.ToString()}");
        }

        private void DrawLabel(Graphics g, ICollection<LineConfiguration> lines, Rectangle paperRect)
        {
            var margins = new Margin(0, 0, 0, 0);
            var lastLinePosition = new List<PointF>();
            for (var i = 0; i < _labelProperties.LabelCount; i++)
                lastLinePosition.Add(_labelStart[i]);
            foreach (var line in lines)
            {
                lastLinePosition[line.Label - 1] = DrawLine(g, lastLinePosition[line.Label - 1], null, line.Content, line, paperRect, margins);
            }
        }

        private void DrawLabel(Graphics g, LabelContent content, Rectangle paperRect)
        {
            var rightMargin = 0;
            var leftMargin = 0;
            // allow vertical binNumber to be written, if provided
            if (!string.IsNullOrEmpty(PrinterSettings.PartLabelTemplate.Identifier.Content) && PrinterSettings.PartLabelTemplate.Identifier.Position == LabelPosition.Right)
                rightMargin = 25;
            if (!string.IsNullOrEmpty(PrinterSettings.PartLabelTemplate.Identifier.Content) && PrinterSettings.PartLabelTemplate.Identifier.Position == LabelPosition.Left)
                leftMargin = 25;
            var margins = new Margin(leftMargin, rightMargin, 0, 0);

            // process template values
            content.Line1 = content.Line1 ?? ReplaceTemplate(content.Part, PrinterSettings.PartLabelTemplate.Line1);
            content.Line2 = content.Line2 ?? ReplaceTemplate(content.Part, PrinterSettings.PartLabelTemplate.Line2);
            content.Line3 = content.Line3 ?? ReplaceTemplate(content.Part, PrinterSettings.PartLabelTemplate.Line3);
            content.Line4 = content.Line4 ?? ReplaceTemplate(content.Part, PrinterSettings.PartLabelTemplate.Line4);
            content.Identifier = content.Identifier ?? ReplaceTemplate(content.Part, PrinterSettings.PartLabelTemplate.Identifier);

            // merge any adjascent template lines together
            MergeLines(g, PrinterSettings.PartLabelTemplate, content, paperRect, margins);
            var line1Position = DrawLine(g, new PointF(_labelStart[PrinterSettings.PartLabelTemplate.Line1.Label - 1].X, _labelStart[PrinterSettings.PartLabelTemplate.Line1.Label - 1].Y), content.Part, content.Line1, PrinterSettings.PartLabelTemplate.Line1, paperRect, margins);
            var line2Position = DrawLine(g, line1Position, content.Part, content.Line2, PrinterSettings.PartLabelTemplate.Line2, paperRect, margins);
            var line3Position = DrawLine(g, line2Position, content.Part, content.Line3, PrinterSettings.PartLabelTemplate.Line3, paperRect, margins);
            var line4Position = DrawLine(g, line3Position, content.Part, content.Line4, PrinterSettings.PartLabelTemplate.Line4, paperRect, margins);
            var identifierPosition = DrawLine(g, line4Position, content.Part, content.Identifier, PrinterSettings.PartLabelTemplate.Identifier, paperRect, margins);
        }

        /// <summary>
        /// Merge all adjascent template lines
        /// </summary>
        /// <param name="template"></param>
        /// <param name="content"></param>
        /// <param name="paperRect"></param>
        /// <param name="margins"></param>
        private void MergeLines(Graphics g, PartLabelTemplate template, LabelContent content, Rectangle paperRect, Margin margins)
        {
            if (template.Line1.Content == template.Line2.Content)
            {
                MergeLines(g, content.Line1, content.Line2, paperRect, margins, out var newLine, out var newLine2);
                content.Line1 = newLine;
                content.Line2 = newLine2;
            }
            if (template.Line2.Content == template.Line3.Content)
            {
                MergeLines(g, content.Line2, content.Line3, paperRect, margins, out var newLine, out var newLine2);
                content.Line2 = newLine;
                content.Line3 = newLine2;
            }
            if (template.Line3.Content == template.Line4.Content)
            {
                MergeLines(g, content.Line3, content.Line4, paperRect, margins, out var newLine, out var newLine2);
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
        private void MergeLines(Graphics g, string firstLine, string secondLine, Rectangle paperRect, Margin margins, out string newFirstLine, out string newSecondLine)
        {
            var fontFirstLine = CreateFont(g, PrinterSettings.PartLabelTemplate.Line2, firstLine, paperRect);
            var fontSecondLine = CreateFont(g, PrinterSettings.PartLabelTemplate.Line3, secondLine, paperRect);
            var line1 = firstLine.ToString();
            var line2 = secondLine.ToString();
            // merge lines and use the second line to wrap
            SizeF len;
            var description = line1?.Trim() ?? "";
            line1 = description.ToString();
            line2 = "";
            // autowrap line 2
            do
            {
                len = g.MeasureString(line1, fontFirstLine);
                if (len.Width > paperRect.Width - margins.Right - margins.Left)
                    line1 = line1.Substring(0, line1.Length - 1);
            } while (len.Width > paperRect.Width - margins.Right - margins.Left);
            if (line1.Length < description.Length)
            {
                // autowrap line 3
                line2 = description.Substring(line1.Length, description.Length - line1.Length).Trim();
                do
                {
                    len = g.MeasureString(line2, fontSecondLine);
                    if (len.Width > paperRect.Width - margins.Right - margins.Left)
                        line2 = line2.Substring(0, line2.Length - 1);
                } while (len.Width > paperRect.Width - margins.Right - margins.Left);
            }
            // overwrite line2 and line3 with new values
            newFirstLine = line1;
            newSecondLine = line2;
            fontFirstLine.Dispose();
            fontSecondLine.Dispose();
        }

        private PointF DrawLine(Graphics g, PointF lineOffset, object part, string lineValue, LineConfiguration template, Rectangle paperRect, Margin margins)
        {
            var font = CreateFont(g, template, lineValue, paperRect);
            var lineBounds = g.MeasureString(lineValue, font);
            if (template.Barcode)
            {
                var x = 0;
                var y = lineOffset.Y + 12;
                DrawBarcode128(lineValue, g, new Rectangle((int)x, (int)y, paperRect.Width, paperRect.Height / _labelProperties.LabelCount));
            }
            else
            {
                float x = 0;
                float y = lineOffset.Y;
                switch (template.Position)
                {
                    case LabelPosition.Right:
                        x = paperRect.Width;
                        break;
                    case LabelPosition.Left:
                        x = (int)lineBounds.Height;
                        break;
                    case LabelPosition.Center:
                        x = (margins.Left + paperRect.Width - margins.Right) / 2 - lineBounds.Width / 2 + _labelProperties.LeftMargin;
                        break;
                }
                x += template.Margin.Left;
                y += template.Margin.Top;
                if (template.Rotate > 0)
                {
                    // rotated labels will start at the top of the label
                    y = _labelStart[template.Label - 1].Y + template.Margin.Top;
                    var state = g.Save();
                    g.ResetTransform();
                    g.RotateTransform(PrinterSettings.PartLabelTemplate.Identifier.Rotate);
                    g.TranslateTransform(x, y, System.Drawing.Drawing2D.MatrixOrder.Append);
                    g.DrawString(lineValue, font, Brushes.Black, new PointF(0, 0));
                    g.Restore(state);
                }
                else
                {
                    g.DrawString(lineValue, font, Brushes.Black, new PointF(x, y));
                }
            }

            font.Dispose();
            // return the new drawing cursor position
            return new PointF(0, lineOffset.Y + lineBounds.Height * 0.65f);
        }

        private Font CreateFont(Graphics g, LineConfiguration template, string lineValue, Rectangle paperRect)
        {
            Font font;
            if (template.AutoSize)
                font = AutosizeFont(g, template.FontName ?? DefaultFontName, template.FontSize, lineValue, paperRect.Width);
            else
                font = new Font(template.FontName ?? DefaultFontName, template.FontSize, GraphicsUnit.Point);
            return font;
        }

        private string ReplaceTemplate(object data, LineConfiguration config)
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
                value = value.Replace(template, data.GetPropertyValue(propertyName).ToString());
            }
            if (config.UpperCase)
                value = value.ToUpper();
            else if (config.LowerCase)
                value = value.ToLower();

            return value;
        }

        private void T_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(_printImage, new Point(0, -12));
            e.HasMorePages = false;
        }

        private void DrawBarcode128(string encodeValue, Graphics g, Rectangle rect)
        {
            var image = _barcodeGenerator.GenerateBarcode(encodeValue, rect.Width, 25);
            image.SetResolution(g.DpiX, g.DpiY);
            g.DrawImageUnscaled(image, new Point(0, rect.Y));
        }

        private Font AutosizeFont(Graphics g, string fontName, float fontSize, string text, int maxWidth)
        {

            SizeF len;
            var newFontSize = fontSize;
            do
            {
                using (var testFont = new Font(fontName, newFontSize, GraphicsUnit.Point))
                {
                    len = g.MeasureString(text, testFont);
                    if (len.Width > maxWidth)
                        newFontSize -= 0.5f;
                }
            } while (len.Width > maxWidth);
            return new Font(fontName, newFontSize, GraphicsUnit.Point);
        }

        private LabelProperties GetLabelDimensions(string labelName)
        {
            switch (labelName)
            {
                case "30277": // 9/16" x 3 7/16"
                    return new LabelProperties(labelName: PrinterSettings.LabelName, topMargin: -15, leftMargin: -20, labelCount: 2, 
                        totalLines: 2, dimensions: new Size(900, 180));
                case "30346": // 1/2" x 1 7/8"
                default:
                    return new LabelProperties(labelName: PrinterSettings.LabelName, topMargin: 0, leftMargin: 0, labelCount: 2, 
                        totalLines: 3, dimensions: new Size(475, 175));
            }
        }
    }
}
