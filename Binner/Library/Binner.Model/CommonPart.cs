﻿namespace Binner.Model
{
    /// <summary>
    /// A part common to all integration api's
    /// </summary>
    public class CommonPart
    {
        public int Rank { get; set; }

        /// <summary>
        /// The local part id, if it exists in inventory
        /// </summary>
        public long? PartId { get; set; }

        /// <summary>
        /// A unique key to reference this record
        /// </summary>
        public Guid Key { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Add a link to the part number manufacturer (Swarm)
        /// </summary>
        public int? SwarmPartNumberManufacturerId { get; set; }

        /// <summary>
        /// Name of supplier
        /// </summary>
        public string? Supplier { get; set; }

        /// <summary>
        /// Supplier specific part number
        /// </summary>
        public string? SupplierPartNumber { get; set; }

        /// <summary>
        /// Base part number
        /// </summary>
        public string? BasePartNumber { get; set; }

        /// <summary>
        /// Any additional part numbers
        /// </summary>
        public ICollection<string> AdditionalPartNumbers { get; set; } = new List<string>();

        /// <summary>
        /// Manufacturer of part
        /// </summary>
        public string? Manufacturer { get; set; }

        /// <summary>
        /// Manufacturer part number
        /// </summary>
        public string? ManufacturerPartNumber { get; set; }

        /// <summary>
        /// Cost for part from this supplier
        /// </summary>
        public double Cost { get; set; }

        /// <summary>
        /// Total Cost for part (order line item total cost)
        /// </summary>
        public double TotalCost { get; set; }

        /// <summary>
        /// Cost currency
        /// </summary>
        public string? Currency { get; set; }

        /// <summary>
        /// Description of part
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// List of known datasheets
        /// </summary>
        public ICollection<string> DatasheetUrls { get; set; } = new List<string>();

        /// <summary>
        /// Type name of part
        /// </summary>
        public string? PartType { get; set; }

        /// <summary>
        /// Type id of part
        /// </summary>
        public long PartTypeId { get; set; }

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
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Product url for more details, may be supplier specific
        /// </summary>
        public string? ProductUrl { get; set; }

        /// <summary>
        /// Package type of part
        /// </summary>
        public string? PackageType { get; set; }

        /// <summary>
        /// Status of part (Active, Inactive)
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Quantity of parts available for part
        /// </summary>
        public long QuantityAvailable { get; set; }

        /// <summary>
        /// Quantity of parts in order
        /// </summary>
        public long Quantity { get; set; }

        /// <summary>
        /// Minimum order quanity
        /// </summary>
        public int MinimumOrderQuantity { get; set; }

        /// <summary>
        /// The stock currently available by the manufacturer/factory according to this supplier
        /// </summary>
        public int? FactoryStockAvailable { get; set; }

        /// <summary>
        /// Manufacturer/factory lead time
        /// </summary>
        public string? FactoryLeadTime { get; set; }

        /// <summary>
        /// Custom reference value
        /// </summary>
        public string? Reference { get; set; }

        /// <summary>
        /// The internal part id for manually added suppliers
        /// </summary>
        public long? PartSupplierId { get; set; }
    }
}
