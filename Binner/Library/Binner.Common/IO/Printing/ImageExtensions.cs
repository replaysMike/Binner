using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;
using System.IO;
using System.Runtime.Versioning;

namespace Binner.Common.IO.Printing
{
    public static class ImageExtensions
    {
        /// <summary>
        /// Convert ImageSharp image to a System.Drawing.Bitmap
        /// </summary>
        /// <typeparam name="TPixel"></typeparam>
        /// <param name="image"></param>
        /// <returns></returns>
        [SupportedOSPlatform("windows")]
        public static Bitmap ToBitmap<TPixel>(this Image<TPixel> image) where TPixel : unmanaged, IPixel<TPixel>
        {
            using var memoryStream = new MemoryStream();
            var imageEncoder = image.Configuration.ImageFormatsManager.GetEncoder(PngFormat.Instance);
            image.Save(memoryStream, imageEncoder);
            memoryStream.Seek(0, SeekOrigin.Begin);
            var bitmap = new Bitmap(memoryStream);
            bitmap.SetResolution((float)image.Metadata.HorizontalResolution, (float)image.Metadata.VerticalResolution);
            return bitmap;
        }

        /// <summary>
        /// Convert a System.Drawing.Bitmap to an ImageSharp image
        /// </summary>
        /// <typeparam name="TPixel"></typeparam>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        [SupportedOSPlatform("windows")]
        public static Image<TPixel> ToImageSharpImage<TPixel>(this Bitmap bitmap) where TPixel : unmanaged, IPixel<TPixel>
        {
            using var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return SixLabors.ImageSharp.Image.Load<TPixel>(memoryStream);
        }
    }
}
