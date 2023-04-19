using Binner.Model.Integrations.DigiKey;
using Binner.Model.Integrations.Mouser;

namespace Binner.Model
{
    /// <summary>
    /// Metadata about a part number
    /// </summary>
    public class PartMetadata
    {
        /// <summary>
        /// Part number
        /// </summary>
        public string? PartNumber { get; set; }

        /// <summary>
        /// Part type
        /// </summary>
        public string? PartType { get; set; }

        /// <summary>
        /// Part keywords
        /// </summary>
        public ICollection<string> Keywords { get; set; } = new List<string>();

        /// <summary>
        /// Digikey's part number for this item
        /// </summary>
        public string? DigikeyPartNumber { get; set; }

        /// <summary>
        /// Mouser's part number for this item
        /// </summary>
        public string? MouserPartNumber { get; set; }

        /// <summary>
        /// Datasheet Url
        /// </summary>
        public string? DatasheetUrl { get; set; }

        /// <summary>
        /// Additional datasheet url's
        /// </summary>
        public ICollection<string> AdditionalDatasheets { get; set; } = new List<string>();

        /// <summary>
        /// Product Url
        /// </summary>
        public string? ProductUrl { get; set; }

        /// <summary>
        /// Image url
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// The part description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The part description
        /// </summary>
        public string? DetailedDescription { get; set; }

        /// <summary>
        /// The part's cost
        /// </summary>
        public decimal? Cost { get; set; }

        /// <summary>
        /// The part's cost currency
        /// </summary>
        public string? Currency { get; set; }

        /// <summary>
        /// The supplier that provides the lowest cost
        /// </summary>
        public string? LowestCostSupplier { get; set; }

        /// <summary>
        /// The product page Url for the lowest cost supplier
        /// </summary>
        public string? LowestCostSupplierUrl { get; set; }

        /// <summary>
        /// Mounting type
        /// </summary>
        public string? MountingType { get; set; }

        /// <summary>
        /// Manufacturer name
        /// </summary>
        public string? Manufacturer { get; set; }

        /// <summary>
        /// The manufacturer part number
        /// </summary>
        public string? ManufacturerPartNumber { get; set; }

        /// <summary>
        /// Product status
        /// </summary>
        public string? ProductStatus { get; set; }

        /// <summary>
        /// Api Specific product data
        /// </summary>
        public ApiIntegrations? Integrations { get; set; }
    }

    public class ApiIntegrations
    {
        public Product? Digikey { get; set; }
        public MouserPart? Mouser { get; set; }
        public object? AliExpress { get; set; }
    }
}
