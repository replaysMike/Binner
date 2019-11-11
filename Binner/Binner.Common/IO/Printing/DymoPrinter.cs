using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;

namespace Binner.Common.IO.Printing
{
    /// <summary>
    /// Dymo Label printer
    /// </summary>
    public class DymoPrinter : ILabelPrinter
    {
        /// <summary>
        /// For debugging, true to draw the label bounds
        /// </summary>
        private const bool DrawBounds = false;
        private const string DefaultFont = "Segoe UI";
        private ICollection<string> _lines;
        private IBarcodeGenerator _barcodeGenerator;
        private Image _printImage;
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

        public Image PrintLabel(ICollection<string> lines, LabelSource? labelSource = null)
        {
            if (lines?.Any() == false) throw new ArgumentNullException(nameof(lines));
            _lines = lines;
            _labelProperties = GetLabelDimensions();
            var t = new PrintDocument();
            SetPrinterFromSettings(t);
            t.PrintPage += T_PrintPage;
            t.DefaultPageSettings.Landscape = true;
            t.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            foreach (PaperSource paperSource in t.PrinterSettings.PaperSources)
            {
                if (paperSource.SourceName.Equals(GetSourceName(labelSource ?? PrinterSettings.LabelSource), StringComparison.CurrentCultureIgnoreCase))
                {
                    t.DefaultPageSettings.PaperSource = paperSource;
                    break;
                }
            }
            t.DefaultPageSettings.PaperSize = new PaperSize(_labelProperties.LabelName, _labelProperties.Dimensions.Width, _labelProperties.Dimensions.Height * _labelProperties.LabelCount);
            // generate the label as an image
            var paperRect = new Rectangle(0, 0, t.DefaultPageSettings.PaperSize.Width, t.DefaultPageSettings.PaperSize.Height);
            var printImage = new Bitmap(paperRect.Width, paperRect.Height);
            printImage.SetResolution(t.DefaultPageSettings.PrinterResolution.X, t.DefaultPageSettings.PrinterResolution.Y);
            _printImage = printImage;
            GenerateLabel(Graphics.FromImage(_printImage), paperRect);

            t.Print();
            return _printImage;
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

        private void GenerateLabel(Graphics g, Rectangle paperRect)
        {
            var printBrush = Brushes.Black;
            var line1 = _lines.First().ToUpper();
            var line2 = string.Empty;
            var line3 = string.Empty;
            var binNumberWidth = 0;
            // allow vertical binNumber to be written, if provided
            if (_lines.Count > 2)
                binNumberWidth = 25;
            // autodecrease the font on line1, if it exceeds the bounds
            var line1Font = AutosizeFont(g, PrinterSettings.Font ?? DefaultFont, _labelProperties.LineFontSizes.First(), line1, paperRect.Width);
            // autowrap text on lines 2/3
            var line2Font = new Font(PrinterSettings.Font ?? DefaultFont, _labelProperties.LineFontSizes.Skip(1).First(), GraphicsUnit.Point);
            if (_lines.Count > 1)
            {
                SizeF len;
                var description = _lines.Skip(1).First()?.Trim() ?? "";
                line2 = description.ToString();
                line3 = "";
                // autowrap line 2
                do
                {
                    len = g.MeasureString(line2, line2Font);
                    if (len.Width > paperRect.Width- binNumberWidth)
                        line2 = line2.Substring(0, line2.Length - 1);
                } while (len.Width > paperRect.Width- binNumberWidth);
                if (line2.Length < description.Length)
                {
                    // autowrap line 3
                    line3 = description.Substring(line2.Length, description.Length - line2.Length).Trim();
                    do
                    {
                        len = g.MeasureString(line3, line2Font);
                        if (len.Width > paperRect.Width- binNumberWidth)
                            line3 = line3.Substring(0, line3.Length - 1);
                    } while (len.Width > paperRect.Width- binNumberWidth);
                }
            }
            var line1Bounds = g.MeasureString(line1, line1Font);
            var line2Bounds = g.MeasureString(line2, line2Font);
            var line3Bounds = g.MeasureString(line3, line2Font);
            var line1x = (paperRect.Width- binNumberWidth) / 2 - line1Bounds.Width / 2 + _labelProperties.LeftMargin;
            var line2x = (paperRect.Width- binNumberWidth) / 2 - line2Bounds.Width / 2 + _labelProperties.LeftMargin;
            var startY = _labelProperties.TopMargin + paperRect.Height - (paperRect.Height / _labelProperties.LabelCount);
            var line2startY = startY + (int)(line1Bounds.Height * 0.65);
            var line3startY = line2startY + (int)(line2Bounds.Height * 0.6);
            g.DrawString(line1, line1Font, printBrush, new PointF(line1x, startY));
            g.DrawString(line2, line2Font, printBrush, new PointF(line2x, line2startY));
            g.DrawString(line3, line2Font, printBrush, new PointF(line2x, line3startY));
            var lastEndLine = line2startY;
            if (!string.IsNullOrEmpty(line2))
                lastEndLine = line2startY + (int)(line2Bounds.Height * 0.9);
            if (!string.IsNullOrEmpty(line3))
                lastEndLine = line3startY + (int)(line3Bounds.Height * 0.9);
            var barcodeY = lastEndLine;
            DrawBarcode128(g, new Rectangle(0, barcodeY, paperRect.Width, paperRect.Height / _labelProperties.LabelCount));

            // draw vertical right bin number if specified
            if(binNumberWidth > 0)
            {
                var binNumber = _lines.Last();
                var binNumberBounds = g.MeasureString(binNumber, line2Font);
                var state = g.Save();
                g.ResetTransform();
                g.RotateTransform(90);
                g.TranslateTransform(paperRect.Width, startY + 14, System.Drawing.Drawing2D.MatrixOrder.Append);
                g.DrawString(binNumber, line2Font, printBrush, 0, 0);
                g.Restore(state);
            }

            if (DrawBounds)
            {
                g.DrawRectangle(Pens.Red, 0, 0, paperRect.Width - 1, paperRect.Height - 1);
                var drawEveryY = paperRect.Height / _labelProperties.LabelCount;
                for (var i = 1; i < _labelProperties.LabelCount; i++)
                    g.DrawLine(Pens.Blue, 0, drawEveryY * i, paperRect.Width, drawEveryY * i);
            }
        }

        private void T_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(_printImage, new Point(0, -12));
            e.HasMorePages = false;
        }

        private void DrawBarcode128(Graphics g, Rectangle rect)
        {
            var image = _barcodeGenerator.GenerateBarcode(_lines.First(), rect.Width, 25);
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

        private LabelProperties GetLabelDimensions()
        {
            switch (PrinterSettings.LabelName)
            {
                case "30277": // 9/16" x 3 7/16"
                    return new LabelProperties(labelName: PrinterSettings.LabelName, topMargin: -15, leftMargin: -20, labelCount: 0, totalLines: 2, dimensions: new Size(350, 120), lineFontSizes: new List<float> { 16, 10 });
                case "30346": // 1/2" x 1 7/8"
                default:
                    return new LabelProperties(labelName: PrinterSettings.LabelName, topMargin: 0, leftMargin: 0, labelCount: 2, totalLines: 3, dimensions: new Size(475, 175), lineFontSizes: new List<float> { 16, 8 });
            }
        }
    }
}
