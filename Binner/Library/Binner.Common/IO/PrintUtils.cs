namespace Binner.Common.IO
{
    public static class PrintUtils
    {
        /// <summary>
        /// Calculate the tape length in millimeters
        /// </summary>
        /// <param name="tapeWidthMm">Width of tape in millimeters</param>
        /// <param name="topMarginMm">Top margin in millimeters</param>
        /// <param name="bottomMarginMm">Bottom margin in millimeters</param>
        /// <param name="imageWidth">Image width in pixels</param>
        /// <param name="imageHeight">Image height in pixels</param>
        /// <returns></returns>
        public static float CalculateTapeLengthMm(float tapeWidthMm, float topMarginMm, float bottomMarginMm, int imageWidth, int imageHeight)
        {
            var maxHeightPixels = MmToPixels(tapeWidthMm - topMarginMm - bottomMarginMm);
            var bitmapRatio = maxHeightPixels / imageHeight;
            var bitmapWidth = bitmapRatio * imageWidth;
            var tapeLengthMm = PixelsToMm(bitmapWidth);
            return tapeLengthMm;
        }

        /// <summary>
        /// Convert millimeters to pixels
        /// </summary>
        /// <param name="pixels"></param>
        /// <param name="dpi"></param>
        /// <returns></returns>
        public static float MmToPixels(float pixels, int dpi = 96)
        {
            return pixels * (dpi / 25.4f);
        }

        /// <summary>
        /// Convert pixels to millimeters
        /// </summary>
        /// <param name="pixels"></param>
        /// <param name="dpi"></param>
        /// <returns></returns>
        public static float PixelsToMm(float pixels, int dpi = 96)
        {
            return pixels * (25.4f / dpi);
        }
    }
}
