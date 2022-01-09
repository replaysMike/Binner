using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Binner.Common.IO.Printing
{
    /// <summary>
    /// Barcode generator
    /// </summary>
    public interface IBarcodeGenerator
    {
        /// <summary>
        /// Generate a Code 128 Barcode
        /// </summary>
        /// <param name="partNumber">Part number to identify</param>
        /// <param name="foregroundColor">Foreground color to use</param>
        /// <param name="backgroundColor">Background color to use</param>
        /// <param name="width">Barcode image width</param>
        /// <param name="height">Barcode image height</param>
        /// <returns></returns>
        Image<Rgba32> GenerateBarcode(string partNumber, Color foregroundColor, Color backgroundColor, int width, int height);
    }
}
