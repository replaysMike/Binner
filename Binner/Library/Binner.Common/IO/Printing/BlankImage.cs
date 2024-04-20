using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;

namespace Binner.Common.IO.Printing
{
    /// <summary>
    /// Generate a blank image with optional text
    /// </summary>
    public class BlankImage
    {
        private const string DefaultFontName = "Segoe UI";
        private const string FontsPath = "./Fonts";
        private static FontFamily _fontFamily;
        private static readonly Lazy<FontCollection> FontCollection = new(LoadFonts);

        public Image<Rgba32> Image { get; private set; }

        private static FontCollection LoadFonts()
        {
            var fontCollection = new FontCollection();
            // Load fonts
            var fontFiles = new FontScanner().GetFonts(FontsPath);
            foreach (var fontFile in fontFiles)
            {
                if (File.Exists(fontFile))
                {
                    try
                    {
                        fontCollection.Add(fontFile);
                    }
                    catch (Exception)
                    {
                        // can't use font
                    }
                }
            }
            _fontFamily = fontCollection.Get(DefaultFontName);
            return fontCollection;
        }
        
        /// <summary>
        /// Generate a blank image with optional text
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="background"></param>
        /// <param name="foreground"></param>
        /// <param name="text"></param>
        /// <param name="fontFamily"></param>
        /// <param name="fontSize"></param>
        /// <param name="drawGrid"></param>
        public BlankImage(int width = 500, int height = 50, Color? background = null, Color? foreground = null, string? text = null, FontFamily? fontFamily = null, int? fontSize = null, bool drawGrid = true)
        {
            FontFamily ff;
            if (fontFamily == null)
            {
                if (!FontCollection.Value.TryGet(DefaultFontName, out ff))
                    ff = _fontFamily;
            }
            else
                ff = fontFamily.Value;
            Image = new Image<Rgba32>(width, height);
            Image.Mutate(c => c.Fill(background ?? Color.White));
            if (drawGrid)
            {
                for (var x = 0; x < width; x += 20)
                    Image.Mutate(c => c.DrawLine(Brushes.Solid(Color.LightGray), 1f, new PointF(x, 0), new PointF(x, height)));
                for (var y = 0; y < height; y += 20)
                    Image.Mutate(c => c.DrawLine(Brushes.Solid(Color.LightGray), 1f, new PointF(0, y), new PointF(width, y)));
            }
            if (text != null)
            {
                var font = ff.CreateFont(fontSize ?? 24);
                var len = TextMeasurer.MeasureSize(text, new TextOptions(font));
                Image.Mutate(c => c.DrawText(text, font, foreground ?? Color.Black, new PointF((width / 2f) - (len.Width / 2f), (height / 2f) - (len.Height / 2f))));
            }
        }
    }
}
