namespace Binner.Model.Integrations
{
    public class KiCadSettings
    {
        public bool Enabled { get; set; } = true;
        public ICollection<KiCadExportPartField> ExportFields { get; set; } = new List<KiCadExportPartField>()
        {
            // default fields to export
            new KiCadExportPartField { Enabled = true, Field = "ReferenceDesignator", KiCadFieldName = "Reference" },
            new KiCadExportPartField { Enabled = true, Field = "PartNumber", KiCadFieldName = "Part Number" },
            new KiCadExportPartField { Enabled = true, Field = "Description", KiCadFieldName = "Description" },
            new KiCadExportPartField { Enabled = true, Field = "Value", KiCadFieldName = "Value" },
            new KiCadExportPartField { Enabled = true, Field = "FootprintName", KiCadFieldName = "Footprint" },
            new KiCadExportPartField { Enabled = true, Field = "Keywords", KiCadFieldName = "Keywords" },
            new KiCadExportPartField { Enabled = true, Field = "DatasheetUrl", KiCadFieldName = "Datasheet" },
            new KiCadExportPartField { Enabled = true, Field = "ProductUrl", KiCadFieldName = "Product Url" },
            new KiCadExportPartField { Enabled = true, Field = "DigiKeyPartNumber", KiCadFieldName = "DigiKey" },
            new KiCadExportPartField { Enabled = true, Field = "MouserPartNumber", KiCadFieldName = "Mouser" },
            new KiCadExportPartField { Enabled = true, Field = "ArrowPartNumber", KiCadFieldName = "Arrow" },
            new KiCadExportPartField { Enabled = true, Field = "TmePartNumber", KiCadFieldName = "Tme" },
            new KiCadExportPartField { Enabled = true, Field = "ExtensionValue1", KiCadFieldName = "ExtensionValue1" },
            new KiCadExportPartField { Enabled = true, Field = "ExtensionValue2", KiCadFieldName = "ExtensionValue2" },
            new KiCadExportPartField { Enabled = true, Field = "Location", KiCadFieldName = "Location" },
            new KiCadExportPartField { Enabled = true, Field = "BinNumber", KiCadFieldName = "Bin" },
            new KiCadExportPartField { Enabled = true, Field = "BinNumber2", KiCadFieldName = "Bin2" },
            new KiCadExportPartField { Enabled = true, Field = "Quantity", KiCadFieldName = "Qty In Stock" },
            new KiCadExportPartField { Enabled = true, Field = "Manufacturer", KiCadFieldName = "Manufacturer" },
            new KiCadExportPartField { Enabled = true, Field = "ManufacturerPartNumber", KiCadFieldName = "Manufacturer Part Number" },
            new KiCadExportPartField { Enabled = true, Field = "Cost", KiCadFieldName = "Cost" },
            new KiCadExportPartField { Enabled = true, Field = "ShortId", KiCadFieldName = "ShortId" },
            new KiCadExportPartField { Enabled = true, Field = "MountingType", KiCadFieldName = "Mounting Type" },
            new KiCadExportPartField { Enabled = true, Field = "PartType", KiCadFieldName = "Part Type" },
        };
    }

    public class KiCadExportPartField
    {
        /// <summary>
        /// True to enable this mapping
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Binner part field to export
        /// </summary>
        public string Field { get; set; } = string.Empty;

        /// <summary>
        /// KiCad field name to export as
        /// </summary>
        public string KiCadFieldName { get; set; } = string.Empty;
    }
}
