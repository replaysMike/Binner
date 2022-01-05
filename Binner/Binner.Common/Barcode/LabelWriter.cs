using System;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Binner.Common.Barcode
{
    public class LabelWriter
    {
        /// <summary>
        /// Draws Label for ITF-14 barcodes
        /// </summary>
        /// <param name="img">Image representation of the barcode without the labels</param>
        /// <returns>Image representation of the barcode with labels applied</returns>
        public static Image<TPixel> LabelITF14<TPixel>(Barcode barcode, Image<TPixel> img) where TPixel : unmanaged, IPixel<TPixel>
        {
            try
            {
                var font = barcode.LabelFont;

                //image.Mutate(c => c.DrawLines(pen, new Point(0, 0), new Point(image.Width, 0))); // top
                //g.DrawImage(img, (float)0, (float)0);
                //g.SmoothingMode = SmoothingMode.HighQuality;
                //g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                //g.CompositingQuality = CompositingQuality.HighQuality;

                // color a white box at the bottom of the barcode to hold the string of data
                img.Mutate(c => c.Fill(Brushes.Solid(barcode.BackColor), new Rectangle(0, img.Height - (font.LineHeight - 2), img.Width, font.LineHeight)));

                // draw datastring under the barcode image centered
                var text = barcode.AlternateLabel == null ? barcode.Data : barcode.AlternateLabel;
                var options = new DrawingOptions
                {
                    TextOptions = new TextOptions
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        DpiX = barcode.HoritontalResolution,
                        DpiY = barcode.VerticalResolution
                    }
                };
                img.Mutate(c => c.DrawText(options, text, font, barcode.ForeColor, new PointF(img.Width / 2f, img.Height - font.LineHeight + 1)));

                // bottom                
                img.Mutate(c => c.DrawLines(Pens.Solid(barcode.ForeColor, img.Height / 16f), new Point(0, img.Height - font.LineHeight - 2), new Point(img.Width, img.Height - font.LineHeight - 2)));
                //pen.Alignment = PenAlignment.Inset;


                return img;
            }
            catch (Exception ex)
            {
                throw new Exception("ELABEL_ITF14-1: " + ex.Message);
            }
        }

        /// <summary>
        /// Draws Label for Generic barcodes
        /// </summary>
        /// <param name="img">Image representation of the barcode without the labels</param>
        /// <returns>Image representation of the barcode with labels applied</returns>
        public static Image<TPixel> LabelGeneric<TPixel>(Barcode barcode, Image<TPixel> img) where TPixel : unmanaged, IPixel<TPixel>
        {
            try
            {
                var font = barcode.LabelFont;

                //g.DrawImage(img, (float)0, (float)0);
                //g.SmoothingMode = SmoothingMode.HighQuality;
                //g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                //g.CompositingQuality = CompositingQuality.HighQuality;
                //g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                var options = new DrawingOptions
                {
                    TextOptions = new TextOptions
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        DpiX = barcode.HoritontalResolution,
                        DpiY = barcode.VerticalResolution
                    }
                };
                var labelX = 0;
                var labelY = 0;

                switch (barcode.LabelPosition)
                {
                    case LabelPositions.BottomCenter:
                        labelX = img.Width / 2;
                        labelY = img.Height - font.LineHeight;
                        options.TextOptions.HorizontalAlignment = HorizontalAlignment.Center;
                        options.TextOptions.VerticalAlignment = VerticalAlignment.Bottom;
                        break;
                    case LabelPositions.BottomLeft:
                        labelX = 0;
                        labelY = img.Height - font.LineHeight;
                        options.TextOptions.HorizontalAlignment = HorizontalAlignment.Left;
                        options.TextOptions.VerticalAlignment = VerticalAlignment.Bottom;
                        break;
                    case LabelPositions.BottomRight:
                        labelX = img.Width;
                        labelY = img.Height - font.LineHeight;
                        options.TextOptions.HorizontalAlignment = HorizontalAlignment.Right;
                        options.TextOptions.VerticalAlignment = VerticalAlignment.Bottom;
                        break;
                    case LabelPositions.TopCenter:
                        labelX = img.Width / 2;
                        labelY = 0;
                        options.TextOptions.HorizontalAlignment = HorizontalAlignment.Center;
                        options.TextOptions.VerticalAlignment = VerticalAlignment.Top;
                        break;
                    case LabelPositions.TopLeft:
                        labelX = img.Width;
                        labelY = 0;
                        options.TextOptions.HorizontalAlignment = HorizontalAlignment.Left;
                        options.TextOptions.VerticalAlignment = VerticalAlignment.Top;
                        break;
                    case LabelPositions.TopRight:
                        labelX = img.Width;
                        labelY = 0;
                        options.TextOptions.HorizontalAlignment = HorizontalAlignment.Right;
                        options.TextOptions.VerticalAlignment = VerticalAlignment.Top;
                        break;
                }

                // color a background color box at the bottom of the barcode to hold the string of data
                img.Mutate(c => c.Fill(Brushes.Solid(barcode.BackColor), new RectangleF(0f, labelY, img.Width, font.LineHeight)));

                // draw datastring under the barcode image
                var text = barcode.AlternateLabel == null ? barcode.Data : barcode.AlternateLabel;
                img.Mutate(c => c.DrawText(options, text, font, barcode.ForeColor, new Point(img.Width, font.LineHeight)));

                return img;
            }
            catch (Exception ex)
            {
                throw new Exception("ELABEL_GENERIC-1: " + ex.Message);
            }
        }

        /// <summary>
        /// Draws Label for EAN-13 barcodes
        /// </summary>
        /// <param name="img">Image representation of the barcode without the labels</param>
        /// <returns>Image representation of the barcode with labels applied</returns>
        public static Image<TPixel> LabelEAN13<TPixel>(Barcode barcode, Image<TPixel> img) where TPixel : unmanaged, IPixel<TPixel>
        {
            try
            {
                var fontStyle = FontStyle.Regular;
                var barWidth = barcode.Width / barcode.EncodedValue.Length;
                var text = barcode.Data;
                var fontSize = GetFontsize(barcode, barcode.Width - barcode.Width % barcode.EncodedValue.Length, img.Height, text) * Barcode.DotsPerPointAt96Dpi;
                var labelFont = new Font(SystemFonts.Find("Arial"), fontSize, fontStyle);

                int shiftAdjustment;
                var options = new DrawingOptions
                {
                    TextOptions = new TextOptions
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        DpiX = barcode.HoritontalResolution,
                        DpiY = barcode.VerticalResolution
                    }
                };
                switch (barcode.Alignment)
                {
                    case AlignmentPositions.Left:
                        shiftAdjustment = 0;
                        break;
                    case AlignmentPositions.Right:
                        shiftAdjustment = (barcode.Width % barcode.EncodedValue.Length);
                        break;
                    case AlignmentPositions.Center:
                    default:
                        shiftAdjustment = (barcode.Width % barcode.EncodedValue.Length) / 2;
                        break;
                }

                //g.DrawImage(img, (float)0, (float)0);
                //g.SmoothingMode = SmoothingMode.HighQuality;
                //g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                //g.CompositingQuality = CompositingQuality.HighQuality;
                //g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                var labelY = 0;

                // Default alignment for EAN13
                labelY = img.Height - labelFont.LineHeight;

                var w1 = barWidth * 4f; // Width of first block
                var w2 = barWidth * 42f; // Width of second block
                var w3 = barWidth * 42f; // Width of third block

                var s1 = (float)shiftAdjustment - barWidth;
                var s2 = s1 + (barWidth * 4f); // Start position of block 2
                var s3 = s2 + w2 + (barWidth * 5f); // Start position of block 3

                // Draw the background rectangles for each block
                img.Mutate(c => c.Fill(Brushes.Solid(barcode.BackColor), new RectangleF(s2, labelY, w2, labelFont.LineHeight)));
                img.Mutate(c => c.Fill(Brushes.Solid(barcode.BackColor), new RectangleF(s3, labelY, w3, labelFont.LineHeight)));

                // Draw datastring under the barcode image
                var smallFont = new Font(labelFont.Family, labelFont.Size * 0.5f * Barcode.DotsPerPointAt96Dpi, fontStyle);
                img.Mutate(c => c.DrawText(options, text.Substring(0, 1), smallFont, barcode.ForeColor, new PointF(s1, img.Height - (float)(smallFont.LineHeight * 0.9f))));
                img.Mutate(c => c.DrawText(options, text.Substring(1, 6), labelFont, barcode.ForeColor, new PointF(s2, labelY)));
                img.Mutate(c => c.DrawText(options, text.Substring(7), labelFont, barcode.ForeColor, new PointF(s3 - barWidth, labelY)));

                return img;
            }
            catch (Exception ex)
            {
                throw new Exception("ELABEL_EAN13-1: " + ex.Message);
            }
        }

        /// <summary>
        /// Draws Label for UPC-A barcodes
        /// </summary>
        /// <param name="img">Image representation of the barcode without the labels</param>
        /// <returns>Image representation of the barcode with labels applied</returns>
        public static Image<TPixel> LabelUPCA<TPixel>(Barcode barcode, Image<TPixel> img) where TPixel : unmanaged, IPixel<TPixel>
        {
            try
            {
                var barWidth = barcode.Width / barcode.EncodedValue.Length;
                var halfBarWidth = (int)(barWidth * 0.5);
                var text = barcode.Data;
                var fontSize = GetFontsize(barcode, (int)((barcode.Width - barcode.Width % barcode.EncodedValue.Length) * 0.9f), img.Height, text) * Barcode.DotsPerPointAt96Dpi;
                var fontStyle = FontStyle.Regular;
                var labelFont = new Font(SystemFonts.Find("Arial"), fontSize, fontStyle);
                
                int shiftAdjustment;
                switch (barcode.Alignment)
                {
                    case AlignmentPositions.Left:
                        shiftAdjustment = 0;
                        break;
                    case AlignmentPositions.Right:
                        shiftAdjustment = (barcode.Width % barcode.EncodedValue.Length);
                        break;
                    case AlignmentPositions.Center:
                    default:
                        shiftAdjustment = (barcode.Width % barcode.EncodedValue.Length) / 2;
                        break;
                }

                //g.DrawImage(img, (float)0, (float)0);
                //g.SmoothingMode = SmoothingMode.HighQuality;
                //g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                //g.CompositingQuality = CompositingQuality.HighQuality;
                //g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                var options = new DrawingOptions
                {
                    TextOptions = new TextOptions
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        DpiX = barcode.HoritontalResolution,
                        DpiY = barcode.VerticalResolution
                    }
                };
                int LabelY = 0;

                // Default alignment for UPCA
                LabelY = img.Height - labelFont.LineHeight;

                var w1 = barWidth * 4f; // Width of first block
                var w2 = barWidth * 34f; // Width of second block
                var w3 = barWidth * 34f; // Width of third block

                var s1 = (float)shiftAdjustment - barWidth;
                var s2 = s1 + (barWidth * 12f); // Start position of block 2
                var s3 = s2 + w2 + (barWidth * 5f); // Start position of block 3
                var s4 = s3 + w3 + (barWidth * 8f) - halfBarWidth;

                // Draw the background rectangles for each block
                img.Mutate(c => c.Fill(Brushes.Solid(barcode.BackColor), new RectangleF(s2, LabelY, w2, labelFont.LineHeight)));
                img.Mutate(c => c.Fill(Brushes.Solid(barcode.BackColor), new RectangleF(s3, LabelY, w3, labelFont.LineHeight)));

                // Draw data string under the barcode image
                var smallFont = new Font(labelFont.Family, labelFont.Size * 0.5f * Barcode.DotsPerPointAt96Dpi, fontStyle);
                img.Mutate(c => c.DrawText(options, text.Substring(0, 1), smallFont, barcode.ForeColor, new PointF(s1, (float)img.Height - smallFont.LineHeight)));
                img.Mutate(c => c.DrawText(options, text.Substring(1, 5), labelFont, barcode.ForeColor, new PointF(s2 - barWidth, (float)LabelY)));
                img.Mutate(c => c.DrawText(options, text.Substring(6, 5), labelFont, barcode.ForeColor, new PointF(s3 - barWidth, (float)LabelY)));
                img.Mutate(c => c.DrawText(options, text.Substring(6, 5), smallFont, barcode.ForeColor, new PointF(s4, (float)img.Height - smallFont.LineHeight)));

                return img;
            }
            catch (Exception ex)
            {
                throw new Exception("ELABEL_UPCA-1: " + ex.Message);
            }
        }

        public static int GetFontsize(Barcode barcode, int wid, int hgt, string lbl)
        {
            // Returns the optimal font size for the specified dimensions
            var fontSize = 10;

            if (lbl.Length > 0)
            {
                for (var i = 1; i <= 100; i++)
                {
                    var test_font = new Font(SystemFonts.Find("Arial"), i * Barcode.DotsPerPointAt96Dpi, FontStyle.Regular);
                    var rendererOptions = new RendererOptions(test_font, (float)barcode.EncodedImage.Metadata.HorizontalResolution, (float)barcode.EncodedImage.Metadata.VerticalResolution);

                    // See how much space the text would need, specifying a maximum width.
                    var lineBounds = TextMeasurer.Measure(lbl, rendererOptions);
                    if ((lineBounds.Width > wid) || (lineBounds.Height > hgt))
                    {
                        fontSize = i - 1;
                        break;
                    }
                }
            };

            return fontSize;
        }
    }
}
