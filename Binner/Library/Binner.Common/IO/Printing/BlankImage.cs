using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Binner.Common.IO.Printing
{
    /// <summary>
    /// Generate a blank image
    /// </summary>
    public class BlankImage
    {
        public Image<Rgba32> Image { get; private set; }

        public BlankImage(int width = 500, int height = 50, Color? color = null, string text = null, FontFamily? fontFamily = null)
        {
            Image = new Image<Rgba32>(width, height);
            if (color is null)
            {
                color = Color.White;
            }
            Image.Mutate(c => c.Fill(color.Value));
            if (text != null && fontFamily != null)
            {
                var font = new Font(fontFamily.Value, 30);
                var len = TextMeasurer.Measure(text, new SixLabors.Fonts.TextOptions(font));
                Image.Mutate(c => c.DrawText(text, font, Color.Black, new PointF((width / 2f) - (len.Width / 2f), (height / 2f) - (len.Height / 2f))));
            }
        }
    }
}
