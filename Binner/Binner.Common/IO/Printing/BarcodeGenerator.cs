using AnyBarcode;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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
        public Image<Rgba32> GenerateBarcode(string partNumber, int width, int height)
        {
            var barcode = new Barcode();
            var barcodeImage = barcode.Encode<Rgba32>(partNumber, BarcodeType.Code128, width, height);
            return barcodeImage;
        }        
    }
}
