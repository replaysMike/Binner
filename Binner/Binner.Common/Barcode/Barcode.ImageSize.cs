namespace Binner.Common.Barcode
{

    public partial class Barcode
    {
        /// <summary>
        /// Represents the size of an image in real world coordinates (millimeters or inches).
        /// </summary>
        public class ImageSize
        {
            public ImageSize(double width, double height, bool metric)
            {
                Width = width;
                Height = height;
                Metric = metric;
            }

            public double Width { get; set; }
            public double Height { get; set; }
            public bool Metric { get; set; }
        }
    }
}