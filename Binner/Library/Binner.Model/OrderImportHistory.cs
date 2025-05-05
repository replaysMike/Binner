using System.ComponentModel.DataAnnotations;

namespace Binner.Model
{
    public class OrderImportHistory
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public long OrderImportHistoryId { get; set; }

        [Required]
        public string Supplier { get; set; } = null!;

        [Required]
        public string SalesOrder { get; set; } = null!;

        public string? Invoice { get; set; }

        public string? Packlist { get; set; }

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

        public ICollection<OrderImportHistoryLineItem>? OrderImportHistoryLineItems { get; set; }
    }
}
