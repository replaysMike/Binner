namespace Binner.Model.Requests
{
    public class PartBase : IPreventDuplicateResource
    {
        /// <summary>
        /// The main part number
        /// </summary>
        public string? PartNumber { get; set; }

        /// <summary>
        /// True to allow potential duplicate of this part
        /// </summary>
        public bool AllowPotentialDuplicate { get; set; }

        /// <summary>
        /// Quantity on hand
        /// </summary>
        public long Quantity { get; set; }

        /// <summary>
        /// The part should be reordered when it gets below this value
        /// </summary>
        public int LowStockThreshold { get; set; }

        /// <summary>
        /// The part cost
        /// </summary>
        public double Cost { get; set; }

        /// <summary>
        /// Currency of part cost
        /// </summary>
        public string? Currency { get; set; }

        /// <summary>
        /// Project Id
        /// </summary>
        public long? ProjectId { get; set; }

        /// <summary>
        /// The optional Digikey part number
        /// </summary>
        public string? DigiKeyPartNumber { get; set; }

        /// <summary>
        /// The optional Mouser part number
        /// </summary>
        public string? MouserPartNumber { get; set; }

        /// <summary>
        /// The optional Arrow part number
        /// </summary>
        public string? ArrowPartNumber { get; set; }

        /// <summary>
        /// The optional TME part number
        /// </summary>
        public string? TmePartNumber { get; set; }

        /// <summary>
        /// Description of part
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Product Url
        /// </summary>
        public string? ProductUrl { get; set; }

        /// <summary>
        /// The supplier that provides the lowest cost
        /// </summary>
        public string? LowestCostSupplier { get; set; }

        /// <summary>
        /// The product page Url for the lowest cost supplier
        /// </summary>
        public string? LowestCostSupplierUrl { get; set; }

        /// <summary>
        /// Image url
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Type of package
        /// </summary>
        public string? PackageType { get; set; }

        /// <summary>
        /// Type of part
        /// </summary>
        public string? PartTypeId { get; set; }

        /// <summary>
        /// Mounting Type of part
        /// </summary>
        public string? MountingTypeId { get; set; }

        /// <summary>
        /// Additional keywords
        /// </summary>
        public string? Keywords { get; set; }

        /// <summary>
        /// Datasheet URL
        /// </summary>
        public string? DatasheetUrl { get; set; }

        /// <summary>
        /// Location of part (i.e. warehouse, room)
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Bin number
        /// </summary>
        public string? BinNumber { get; set; }

        /// <summary>
        /// Secondary Bin number
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
        /// The raw barcode that was scanned
        /// </summary>
        public string? Barcode { get; set; }

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
        /// Parametric part value
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// A unique 10 character part identifier (for Binner use) that can be used to identify the inventory part
        /// </summary>
        public string? ShortId { get; set; }

        public ICollection<CustomValue>? CustomFields { get; set; }

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
        /// List of all parametrics for part
        /// </summary>
        public ICollection<PartParametric>? Parametrics { get; set; }

        /// <summary>
        /// List of all cad models for part
        /// </summary>
        public ICollection<PartModel>? Models { get; set; }
    }
}
