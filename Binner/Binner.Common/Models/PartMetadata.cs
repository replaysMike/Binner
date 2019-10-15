using System.Collections.Generic;

namespace Binner.Common.Models
{
    /// <summary>
    /// Metadata about a part number
    /// </summary>
    public class PartMetadata
    {
        /// <summary>
        /// Part number
        /// </summary>
        public string PartNumber { get; set; }

        /// <summary>
        /// Datasheet Url
        /// </summary>
        public string DatasheetUrl { get; set; }

        /// <summary>
        /// Additional datasheet url's
        /// </summary>
        public ICollection<string> AdditionalDatasheets { get; set; } = new List<string>();

        /// <summary>
        /// Product Url
        /// </summary>
        public string ProductUrl { get; set; }

        /// <summary>
        /// The part description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The part description
        /// </summary>
        public string DetailedDescription { get; set; }

        /// <summary>
        /// The part's cost
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Package type
        /// </summary>
        public string Package { get; set; }

        /// <summary>
        /// Manufacturer name
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// The manufacturer part number
        /// </summary>
        public string ManufacturerPartNumber { get; set; }

        /// <summary>
        /// Product status
        /// </summary>
        public string ProductStatus { get; set; }
    }
}
