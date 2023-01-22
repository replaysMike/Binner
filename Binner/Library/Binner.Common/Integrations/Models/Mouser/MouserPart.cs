using System.Collections.Generic;

namespace Binner.Common.Integrations.Models.Mouser
{
    public class MouserPart
    {
        public string Availability { get; set; }
        public int AvailabilityInteger
        {
            get
            {
                var partCount = 0;
                if (!string.IsNullOrEmpty(Availability))
                {
                    var parts = Availability.Split(' ');
                    if (parts.Length > 0)
                        int.TryParse(parts[0], out partCount);
                }
                return partCount;
            }
        }
        public string DataSheetUrl { get; set; }
        public string Description { get; set; }
        public string FactoryStock { get; set; }
        public string ImagePath { get; set; }
        public string Category { get; set; }
        public string LeadTime { get; set; }
        public string LifecycleStatus { get; set; }
        public string Manufacturer { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public string Min { get; set; }
        public string Mult { get; set; }
        public string MouserPartNumber { get; set; }
        public ICollection<ProductAttribute> ProductAttributes { get; set; }
        public ICollection<PriceBreak> PriceBreaks { get; set; }
        public ICollection<AlternatePackaging> AlternatePackagings { get; set; }
        public string ProductDetailUrl { get; set; }
        public bool Reeling { get; set; }
        public string ROHSStatus { get; set; }
        public int MultiSimBlue { get; set; }
        public UnitWeightKg UnitWeightKg { get; set; }
        public string RestrictionMessage { get; set; }
        public string PID { get; set; }
        public ICollection<ProductCompliance> ProductCompliance { get; set; }
    }

    public class ProductAttribute
    {
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
    }

    public class PriceBreak
    {
        public long Quantity { get; set; }
        public string Price { get; set; }
        public string Currency { get; set; }
        public double Cost
        {
            get
            {
                return double.Parse(Price.Replace("$", ""));
            }
        }
    }

    public class AlternatePackaging
    {
        public string APMfrPN { get; set; }
    }

    public class UnitWeightKg
    {
        public double UnitWeight { get; set; }
    }

    public class ProductCompliance
    {
        public string ComplianceName { get; set; }
        public string ComplianceValue { get; set; }
    }
}
