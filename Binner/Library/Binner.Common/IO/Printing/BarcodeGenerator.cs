using AnyBarcode;
using Binner.Model.IO.Printing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

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
        /// <param name="foregroundColor">Foreground color to use</param>
        /// <param name="backgroundColor">Background color to use</param>
        /// <param name="width">Barcode image width</param>
        /// <param name="height">Barcode image height</param>
        /// <returns></returns>
        public Image<Rgba32> GenerateBarcode(string partNumber, Color foregroundColor, Color backgroundColor, int width, int height)
        {
            var barcode = new Barcode();
            try
            {
                var barcodeImage = barcode.Encode<Rgba32>(partNumber, BarcodeType.Code128, foregroundColor, backgroundColor, width, height);
                return barcodeImage;
            }
            catch (Exception)
            {
                return new Image<Rgba32>(width, height);
            }
        }
    }
}
