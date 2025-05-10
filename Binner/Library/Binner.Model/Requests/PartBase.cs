﻿namespace Binner.Model.Requests
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
    }
}
