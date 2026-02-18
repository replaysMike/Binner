using Binner.Model.Swarm;

namespace Binner.Model
{
    public class Image
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int ImageId { get; set; }

        /// <summary>
        /// Uniquely identifies this image in the resource server
        /// </summary>
        public Guid ResourceId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The detected or configured image type
        /// </summary>
        public ImageTypes ImageType { get; set; } = ImageTypes.Unknown;

        /// <summary>
        /// The saved image size
        /// </summary>
        public ImageSizes ImageSize { get; set; } = ImageSizes.Original;

        /// <summary>
        /// Image width
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Image height
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Horizontal resolution in dpi
        /// </summary>
        public double DpiX { get; set; }

        /// <summary>
        /// Vertical resolution in dpi
        /// </summary>
        public double DpiY { get; set; }

        /// <summary>
        /// The original url the image was downloaded from
        /// </summary>
        public string? OriginalUrl { get; set; }

        /// <summary>
        /// If this image was imported from a supplier API (DigiKey, Mouser) indicate which one
        /// </summary>
        public int? CreatedFromSupplierId { get; set; }

        /// <summary>
        /// The crc 32 checksum of the image
        /// </summary>
        public int Crc32 { get; set; }

        /// <summary>
        /// The resource server Url of the datasheet and it's associated content
        /// </summary>
        public string ResourceSourceUrl { get; set; } = null!;

        /// <summary>
        /// The resource path of the datasheet and it's associated content
        /// </summary>
        public string ResourcePath { get; set; } = null!;

        public Guid GlobalId { get; set; }
    }
}
