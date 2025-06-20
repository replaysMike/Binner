using Binner.Common;
using Binner.Model;
using Binner.Model.Barcode;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    /// <summary>
    /// Stores the history of part labels scanned
    /// </summary>
    public class PartScanHistory : IEntity, IOptionalUserData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PartScanHistoryId { get; set; }

        public long? PartId { get; set; }

        /// <summary>
        /// The raw scan string
        /// </summary>
        [Required]
        public string RawScan { get; set; } = null!;

        /// <summary>
        /// A crc of the RawScan value
        /// </summary>
        public int Crc { get; set; }

        public BarcodeTypes BarcodeType { get; set; }

        public ScannedLabelType ScannedLabelType { get; set; }

        public Suppliers Supplier { get; set; }
        
        public string? ManufacturerPartNumber { get; set; }

        public string? SupplierPartNumber { get; set; }

        public string? SalesOrder { get; set; }

        public string? Invoice { get; set; }

        public int Quantity { get; set; }

        public string? Mid { get; set; }

        public string? LotCode { get; set; }

        public string? Description { get; set; }

        public string? CountryOfOrigin { get; set; }

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

        [ForeignKey(nameof(PartId))]
        public Part? Part { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}
