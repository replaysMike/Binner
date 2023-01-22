using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binner.Common.Models
{
    public class PartNumberManufacturer
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int PartNumberManufacturerId { get; set; }

        /// <summary>
        /// The parent part number
        /// </summary>
        public int PartNumberId { get; set; }

        /// <summary>
        /// The default image for this part
        /// </summary>
        public int? DefaultPartNumberManufacturerImageMetadataId { get; set; }

        /// <summary>
        /// The default primary datasheet for this part
        /// </summary>
        public int? PrimaryDatasheetId { get; set; }

        /// <summary>
        /// The manufacturer's part number
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Comma separated list of alternate names
        /// </summary>
        public string? AlternateNames { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Alternate Description
        /// </summary>
        public string? AlternateDescription { get; set; }

        /// <summary>
        /// Part type category
        /// </summary>
        public long? PartTypeId { get; set; }

        /// <summary>
        /// Manufacturer, null indicates unknown
        /// </summary>
        public int? ManufacturerId { get; set; }

        /// <summary>
        /// Manufacturer name
        /// </summary>
        public string? ManufacturerName { get; set; }

        /// <summary>
        /// The source that created this record
        /// </summary>
        public DataSource Source { get; set; } = DataSource.ManualInput;

        /// <summary>
        /// The original part number processing record that created this entry
        /// </summary>
        public int? SwarmPartNumberId { get; set; }

        /// <summary>
        /// Date record has been manually updated by an admin
        /// </summary>
        public DateTime? DatePrunedUtc { get; set; }

        /// <summary>
        /// True if part is obsolete
        /// </summary>
        public bool IsObsolete { get; set; }

        /// <summary>
        /// If this part was imported from a supplier API (DigiKey, Mouser) indicate which one
        /// </summary>
        public int? CreatedFromSupplierId { get; set; }

        /// <summary>
        /// Date the part was created
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        public ICollection<PartNumberManufacturerParametric> Parametrics { get; set; } = null!;
        public ICollection<PartNumberManufacturerSupplierBasic> Suppliers { get; set; } = null!;
        public ICollection<PartNumberManufacturerImageMetadata> ImageMetadata { get; set; } = null!;
        public ICollection<Keyword> Keywords { get; set; } = null!;
        public ICollection<DatasheetBasic> Datasheets { get; set; } = null!;
        public ICollection<Package> Package { get; set; } = null!;
    }
}
