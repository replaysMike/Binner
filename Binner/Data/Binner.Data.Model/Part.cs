﻿using Binner.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    /// <summary>
    /// A Part
    /// </summary>
    public class Part
#if INITIALCREATE
        : IEntity,
#else
        : IPartialEntity,
#endif
        IOptionalUserData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PartId { get; set; }

        /// <summary>
        /// A unique 10 character part identifier (for Binner use) that can be used to identify the inventory part
        /// </summary>
        public string? ShortId { get; set; }

        /// <summary>
        /// Associated user
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Associated organization
        /// </summary>
        public int? OrganizationId { get; set; }

        /// <summary>
        /// The number of items in stock
        /// </summary>
        public long Quantity { get; set; }

        /// <summary>
        /// The part should be reordered when it gets below this value
        /// </summary>
        public int LowStockThreshold { get; set; } = SystemDefaults.LowStockThreshold;

        /// <summary>
        /// The part cost
        /// </summary>
        public double Cost { get; set; }

        /// <summary>
        /// Currency of part cost
        /// </summary>
        public string? Currency { get; set; }

        /// <summary>
        /// The main part number
        /// </summary>
        [MaxLength(64)]
        public string? PartNumber { get; set; }

        /// <summary>
        /// The Digikey part number
        /// </summary>
        public string? DigiKeyPartNumber { get; set; }

        /// <summary>
        /// The Mouser part number
        /// </summary>
        public string? MouserPartNumber { get; set; }

        /// <summary>
        /// The Arrow part number
        /// </summary>
        public string? ArrowPartNumber { get; set; }

        /// <summary>
        /// The TME part number
        /// </summary>
        public string? TmePartNumber { get; set; }

        /// <summary>
        /// The Element14 part number
        /// </summary>
        public string? Element14PartNumber { get; set; }

        /// <summary>
        /// Description of part
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Type of part
        /// </summary>
        public long PartTypeId { get; set; }

        /// <summary>
        /// Mounting Type
        /// </summary>
        public int MountingTypeId { get; set; }

        /// <summary>
        /// Package Type (eg. DIP8)
        /// </summary>
        public string? PackageType { get; set; }

        /// <summary>
        /// Product Url
        /// </summary>
        public string? ProductUrl { get; set; }

        /// <summary>
        /// Image url
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// The supplier that provides the lowest cost
        /// </summary>
        public string? LowestCostSupplier { get; set; }

        /// <summary>
        /// The product page Url for the lowest cost supplier
        /// </summary>
        public string? LowestCostSupplierUrl { get; set; }

        /// <summary>
        /// Project associated with the part
        /// </summary>
        public long? ProjectId { get; set; }

        /// <summary>
        /// Additional keywords
        /// </summary>
        public string? Keywords { get; set; }

        /// <summary>
        /// Additional keywords
        /// </summary>
        [NotMapped]
        public ICollection<string>? KeywordsList { get; set; }

        /// <summary>
        /// Datasheet URL
        /// </summary>
        public string? DatasheetUrl { get; set; }

        /// <summary>
        /// Location of part (i.e. warehouse, room)
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Bin number (i.e. Shelf, Cabinet #)
        /// </summary>
        public string? BinNumber { get; set; }

        /// <summary>
        /// Secondary Bin number (i.e. Bin)
        /// </summary>
        public string? BinNumber2 { get; set; }

        /// <summary>
        /// Manufacturer name
        /// </summary>
        public string? Manufacturer { get; set; }

        /// <summary>
        /// The manufacturer part number
        /// </summary>
        public string? ManufacturerPartNumber { get; set; }

        /// <summary>
        /// KiCad symbol name
        /// </summary>
        public string? SymbolName { get; set; }

        /// <summary>
        /// KiCad footprint name
        /// </summary>
        public string? FootprintName { get; set; }

        /// <summary>
        /// Extension value 1 (can be used to store custom information)
        /// </summary>
        public string? ExtensionValue1 { get; set; }

        /// <summary>
        /// Extension value 2 (can be used to store custom information)
        /// </summary>
        public string? ExtensionValue2 { get; set; }

        /// <summary>
        /// Add a link to the part number manufacturer (Swarm)
        /// </summary>
        public int? SwarmPartNumberManufacturerId { get; set; }

        /// <summary>
        /// Part value. '1k', '4.7uf', or the model name of the chip.
        /// Used for KiCad part value.
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Lead time for ordering new parts
        /// </summary>
        public string? LeadTime { get; set; }

        /// <summary>
        /// The status of the part, typically 'In Stock', 'Out of Stock', 'Backordered', etc.
        /// </summary>
        public string? ProductStatus { get; set; }

        /// <summary>
        /// The base product number, if available
        /// </summary>
        public string? BaseProductNumber { get; set; }

        /// <summary>
        /// The name of the product series, if available
        /// </summary>
        public string? Series { get; set; }

        /// <summary>
        /// Rohs status
        /// </summary>
        public string? RohsStatus { get; set; }

        /// <summary>
        /// Reach status
        /// </summary>
        public string? ReachStatus { get; set; }

        /// <summary>
        /// Moisture sensitivity level
        /// </summary>
        public string? MoistureSensitivityLevel { get; set; }

        /// <summary>
        /// Export control class
        /// </summary>
        public string? ExportControlClassNumber { get; set; }

        /// <summary>
        /// Htsus code
        /// </summary>
        public string? HtsusCode { get; set; }

        /// <summary>
        /// A comma delimited list of other names for the product
        /// </summary>
        public string? OtherNames { get; set; }

        public PartDataSources DataSource { get; set; } = PartDataSources.ManualInput;

        /// <summary>
        /// If a data source other than manual input, indicate the order id it came from
        /// </summary>
        public string? DataSourceId { get; set; }

        /// <summary>
        /// The date the part metadata was last updated from external sources
        /// </summary>
        public DateTime MetadataLastUpdatedUtc { get; set; }

        /// <summary>
        /// The date the part metadata was last syncrhonized to/from swarm
        /// </summary>
        public DateTime? LastSwarmSyncUtc { get; set; }

        /// <summary>
        /// The date the record was created
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

#if INITIALCREATE
        /// <summary>
        /// The date the record was modified
        /// </summary>
        public DateTime DateModifiedUtc { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
#endif

        [ForeignKey(nameof(ProjectId))]
        public Project? Project { get; set; }

        [ForeignKey(nameof(PartTypeId))]
        public PartType? PartType { get; set; }
        public ICollection<PartScanHistory>? PartScanHistories { get; set; }
        public ICollection<ProjectPartAssignment>? ProjectPartAssignments { get; set; }
        public ICollection<StoredFile>? StoredFiles { get; set; }
        public ICollection<PartModel>? PartModels { get; set; }
        public ICollection<PartSupplier>? PartSuppliers { get; set; }
        public ICollection<PartParametric>? PartParametrics { get; set; }
    }
}
