namespace Binner.Model
{
    public class PartNumberManufacturerSupplier
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int PartNumberManufacturerSupplierId { get; set; }

        /// <summary>
        /// The part number
        /// </summary>
        public int PartNumberManufacturerId { get; set; }

        /// <summary>
        /// Supplier
        /// </summary>
        public int SupplierId { get; set; }

        /// <summary>
        /// Name of the supplier
        /// </summary>
        public string? SupplierName { get; set; }

        /// <summary>
        /// The supplier's part number
        /// </summary>
        public string SupplierPartNumber { get; set; } = null!;

        /// <summary>
        /// Product Url
        /// </summary>
        public string? ProductUrl { get; set; }

        /// <summary>
        /// Supplier cost
        /// </summary>
        public double? Cost { get; set; }

        /// <summary>
        /// Currency (USD/CAD)
        /// </summary>
        public string? Currency { get; set; }

        /// <summary>
        /// If available, the quantity of parts available
        /// </summary>
        public int? QuantityAvailable { get; set; }

        /// <summary>
        /// The minimum number of items that can be ordered at this price
        /// </summary>
        public int? MinimumOrderQuantity { get; set; }

        /// <summary>
        /// The packaging for this price representation
        /// </summary>
        public string? Packaging { get; set; }

        /// <summary>
        /// The stock currently available by the manufacturer/factory according to this supplier
        /// </summary>
        public int? StockAvailable { get; set; }

        /// <summary>
        /// Manufacturer/factory lead time
        /// </summary>
        public string? LeadTime { get; set; }

        /// <summary>
        /// Creation time
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        /// <summary>
        /// The date the cost/quantity was last updated
        /// </summary>
        public DateTime? StockLastUpdatedUtc { get; set; }

        /// <summary>
        /// The part number manufacturer
        /// </summary>
        public PartNumberManufacturerBasic? PartNumberManufacturer { get; set; }
    }
}
