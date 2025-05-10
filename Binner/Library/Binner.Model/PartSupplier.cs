namespace Binner.Model
{
    public class PartSupplier
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public long PartSupplierId { get; set; }

        /// <summary>
        /// The associated part
        /// </summary>
        public long PartId { get; set; }

        /// <summary>
        /// Supplier name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Supplier part number
        /// </summary>
        public string? SupplierPartNumber { get; set; }

        /// <summary>
        /// Part cost
        /// </summary>
        public double? Cost { get; set; }

        /// <summary>
        /// Quantity of part available
        /// </summary>
        public int QuantityAvailable { get; set; }

        /// <summary>
        /// Minimum order quantity
        /// </summary>
        public int MinimumOrderQuantity { get; set; }

        /// <summary>
        /// Url to product page
        /// </summary>
        public string? ProductUrl { get; set; }

        /// <summary>
        /// Url to image to supplier part
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateModifiedUtc { get; set; } = DateTime.UtcNow;

        public Part Part { get; set; } = null!;

        public ICollection<CustomValue> CustomFields { get; set; } = new List<CustomValue>();

        /// <summary>
        /// Optional user id to associate
        /// </summary>
        public int? UserId { get; set; }
    }
}
