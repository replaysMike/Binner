namespace Binner.Model
{
    public class OrderImportHistoryLineItem
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public long OrderImportHistoryLineItemId { get; set; }

        public long OrderImportHistoryId { get; set; }

        public string? PartNumber { get; set; }

        public string? Manufacturer { get; set; }

        public string? ManufacturerPartNumber { get; set; }

        public string? Supplier { get; set; }

        public double Cost { get; set; }

        public long? PartId { get; set; }

        public string? Description { get; set; }

        public string? CustomerReference { get; set; }

        public long Quantity { get; set; }

        /// <summary>
        /// Associated user
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Associated organization
        /// </summary>
        public int? OrganizationId { get; set; }

        /// <summary>
        /// The date the record was created
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        /// <summary>
        /// The date the record was modified
        /// </summary>
        public DateTime DateModifiedUtc { get; set; }
    }
}
