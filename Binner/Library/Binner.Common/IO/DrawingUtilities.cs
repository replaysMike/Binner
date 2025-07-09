namespace Binner.Common.IO
{
    public static class DrawingUtilities
    {
        /// <summary>
        /// Convert points to pixels
        /// </summary>
        /// <param name="pointSize"></param>
        /// <returns></returns>
        public static float PointToPixel(float pointSize)
        {
            return pointSize / 72f * 96f;
        }
    }
}
