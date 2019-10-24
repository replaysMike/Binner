using System.Collections.Generic;

namespace Binner.Common.Models
{
    /// <summary>
    /// A part common to all integration api's
    /// </summary>
    public class CommonPart
    {
        /// <summary>
        /// Name of supplier
        /// </summary>
        public string Supplier { get; set; }

        /// <summary>
        /// Supplier specific part number
        /// </summary>
        public string SupplierPartNumber { get; set; }

        /// <summary>
        /// Base part number
        /// </summary>
        public string BasePartNumber { get; set; }

        /// <summary>
        /// Any additional part numbers
        /// </summary>
        public ICollection<string> AdditionalPartNumbers { get; set; } = new List<string>();

        /// <summary>
        /// Manufacturer of part
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// Manufacturer part number
        /// </summary>
        public string ManufacturerPartNumber { get; set; }

        /// <summary>
        /// Cost for part from this supplier
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Cost currency
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Description of part
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// List of known datasheets
        /// </summary>
        public ICollection<string> DatasheetUrls { get; set; } = new List<string>();

        /// <summary>
        /// Type name of part
        /// </summary>
        public string PartType { get; set; }

        /// <summary>
        /// Mounting type of part
        /// </summary>
        public int MountingTypeId { get; set; }

        /// <summary>
        /// List of computed keywords
        /// </summary>
        public ICollection<string> Keywords { get; set; } = new List<string>();

        /// <summary>
        /// Image url of part
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Product url for more details, may be supplier specific
        /// </summary>
        public string ProductUrl { get; set; }

        /// <summary>
        /// Package type of part
        /// </summary>
        public string PackageType { get; set; }

        /// <summary>
        /// Status of part (Active, Inactive)
        /// </summary>
        public string Status { get; set; }
    }
}
