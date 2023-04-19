namespace Binner.Model.Swarm
{
    public class PartNumberManufacturerImageMetadata
    {
        /// <summary>
        /// True if image is default image
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Primary key
        /// </summary>
        public int PartNumberManufacturerImageMetadataId { get; set; }

        /// <summary>
        /// The part number
        /// </summary>
        public int PartNumberManufacturerId { get; set; }

        /// <summary>
        /// Unique ImageId relative to the Part Number's resourceId.
        /// </summary>
        public int ImageId { get; set; }

        /// <summary>
        /// The resource server Url of the datasheet and it's associated content
        /// </summary>
        public string ResourceSourceUrl { get; set; } = null!;

        /// <summary>
        /// The resource path of the datasheet and it's associated content
        /// </summary>
        public string ResourcePath { get; set; } = null!;

        /// <summary>
        /// The detected or configured image type
        /// </summary>
        public ImageTypes ImageType { get; set; } = ImageTypes.ProductShot;

        /// <summary>
        /// The original url the image was downloaded from
        /// </summary>
        public string? OriginalUrl { get; set; }

        /// <summary>
        /// If this part was imported from a supplier API (DigiKey, Mouser) indicate which one
        /// </summary>
        public int? CreatedFromSupplierId { get; set; }
    }
}
