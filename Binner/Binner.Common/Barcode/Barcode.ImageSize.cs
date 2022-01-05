namespace Binner.Common.Barcode
{
    public partial class Barcode
    {
        /// <summary>
        /// Represents the size of an image in real world units (millimeters or inches).
        /// </summary>
        public class ImageSize
        {
            /// <summary>
            /// Specifies an image size
            /// </summary>
            /// <param name="width">Width of the image</param>
            /// <param name="height">Height of the image</param>
            /// <param name="units">Specify the real world units</param>
            public ImageSize(double width, double height, ImageUnits units)
            {
                Width = width;
                Height = height;
                Units = units;
            }

            public double Width { get; set; }
            public double Height { get; set; }
            public ImageUnits Units { get; set; }
        }

        public enum ImageUnits
        {
            /// <summary>
            /// Inches
            /// </summary>
            Inches,
            /// <summary>
            /// Millimeters
            /// </summary>
            Millimeters
        }
    }
}