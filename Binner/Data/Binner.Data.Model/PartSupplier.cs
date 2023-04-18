using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    public class PartSupplier : IEntity, IUserData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
        /// Last modified date
        /// </summary>
        public DateTime DateModifiedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional user id to associate
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Associated organization
        /// </summary>
        public int? OrganizationId { get; set; }

        [ForeignKey(nameof(PartId))]
        public Part? Part { get; set; }

#if INITIALCREATE
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
#endif
    }
}
