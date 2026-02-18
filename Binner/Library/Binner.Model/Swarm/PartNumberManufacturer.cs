namespace Binner.Model.Swarm
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
        /// The stock currently available by the manufacturer/factory
        /// </summary>
        public int? FactoryStockAvailable { get; set; }

        /// <summary>
        /// Manufacturer/factory lead time
        /// </summary>
        public string? FactoryLeadTime { get; set; }

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
        /// If this part was imported from a supplier API (DigiKey, Mouser) indicate which one
        /// </summary>
        public int? CreatedFromSupplierId { get; set; }

        /// <summary>
        /// Date the part was created
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        public ICollection<PartNumberManufacturerModel> Models { get; set; } = new List<PartNumberManufacturerModel>();
        public ICollection<PartNumberManufacturerParametric> Parametrics { get; set; } = new List<PartNumberManufacturerParametric>();
        public ICollection<PartNumberManufacturerSupplierBasic> Suppliers { get; set; } = new List<PartNumberManufacturerSupplierBasic>();
        public ICollection<PartNumberManufacturerImageMetadata> ImageMetadata { get; set; } = new List<PartNumberManufacturerImageMetadata>();
        public ICollection<Keyword> Keywords { get; set; } = new List<Keyword>();
        public ICollection<DatasheetBasic> Datasheets { get; set; } = new List<DatasheetBasic>();
        public ICollection<Pinout> Pinouts { get; set; } = new List<Pinout>();
        public ICollection<Package> Package { get; set; } = null!;
    }
}
