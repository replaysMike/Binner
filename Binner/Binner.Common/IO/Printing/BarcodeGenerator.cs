using Binner.Common.Barcode;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;
using System.Runtime.Versioning;

namespace Binner.Common.IO.Printing
{
    /// <summary>
    /// Barcode generator
    /// </summary>
    public class BarcodeGenerator : IBarcodeGenerator
    {
        /// <summary>
        /// Generate a Code 128 Barcode
        /// </summary>
        /// <param name="partNumber">Part number to identify</param>
        /// <param name="width">Barcode image width</param>
        /// <param name="height">Barcode image height</param>
        /// <returns></returns>
        [SupportedOSPlatform("windows")]
        public Image<Rgba32> GenerateBarcode(string partNumber, int width, int height)
        {
            var barcode = new Barcode.Barcode();
            var barcodeImage = barcode.Encode(BarcodeType.Code128, partNumber, width, height);
            // convert system.drawing.image to Imagesharp Image
            return ((Bitmap)barcodeImage).ToImageSharpImage<Rgba32>();
        }        
    }
}
