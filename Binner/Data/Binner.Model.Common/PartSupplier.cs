using System.ComponentModel.DataAnnotations;

namespace Binner.Model.Common
{
    /// <summary>
    /// A user defined project
    /// </summary>
    public class PartSupplier : IEntity, IEquatable<PartSupplier>
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
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

        /// <summary>
        /// Optional user id to associate
        /// </summary>
        public int? UserId { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is PartSupplier project)
                return Equals(project);
            return false;
        }

        public bool Equals(PartSupplier? other)
        {
            return other != null && PartSupplierId == other.PartSupplierId && UserId == other.UserId;
        }

        public override int GetHashCode()
        {
#if (NET462 || NET471)
            return PartSupplierId.GetHashCode() ^ (UserId?.GetHashCode() ?? 0);
#else
            return HashCode.Combine(PartSupplierId, UserId);
#endif

        }

        public override string ToString()
        {
            return $"{PartSupplierId}: {Name}";
        }
    }
}
