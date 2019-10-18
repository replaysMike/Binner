using System.Collections.Generic;

namespace Binner.Common.Integrations.Models.Digikey
{
    public class KeywordSearchResponse
    {
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public int ProductsCount { get; set; }
        public ICollection<Product> ExactManufacturerProducts { get; set; } = new List<Product>();
        public int ExactManufacturerProductsCount { get; set; }
        public ICollection<Product> ExactDigiKeyProduct { get; set; } = new List<Product>();
        public LimitedTaxonomy LimitedTaxonomy { get; set; } = new LimitedTaxonomy();
        public ICollection<LimitedParameter> FilterOptions { get; set; } = new List<LimitedParameter>();
        public IsoSearchLocale SearchLocaleUsed { get; set; } = new IsoSearchLocale();
    }

    public class Product
    {
        public string DigiKeyPartNumber { get; set; }
        public int QuantityAvailable { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public Manufacturer Manufacturer { get; set; } = new Manufacturer();
        public string ProductDescription { get; set; }
        public string DetailedDescription { get; set; }
        public int MinimumOrderQuantity { get; set; }
        public bool NonStock { get; set; }
        public int QuantityOnOrder { get; set; }
        public int ManufacturerPublicQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string ProductUrl { get; set; }
        public string ProductStatus { get; set; }
        public string PrimaryDatasheet { get; set; }
        public string PrimaryPhoto { get; set; }
        public string RoHSStatus { get; set; }
        public string LeadStatus { get; set; }
        public string TariffDescription { get; set; }
        public Series Series { get; set; } = new Series();
        public ICollection<ParameterObject> Parameters { get; set; } = new List<ParameterObject>();
        // todo: AlternatePackaging

    }

    public class Manufacturer
    {
        public int ParameterId { get; set; }
        public string ValueId { get; set; }
        public string Parameter { get; set; }
        public string Value { get; set; }
    }

    public class StandardPricing
    {
        public int BreakQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class Series
    {
        public int ParameterId { get; set; }
        public string ValueId { get; set; }
        public string Parameter { get; set; }
        public string Value { get; set; }
    }

    public class ParameterObject
    {
        public int ParameterId { get; set; }
        public string ValueId { get; set; }
        public string Parameter { get; set; }
        public string Value { get; set; }
    }

    public class LimitedTaxonomy
    {
        public ICollection<LimitedTaxonomy> Children { get; set; }
        public int ProductCount { get; set; }
        public int NewProductCount { get; set; }
        public int ParameterId { get; set; }
        public string ValueId { get; set; }
        public string Parameter { get; set; }
        public string Value { get; set; }
    }

    public class LimitedParameter
    {
        public ICollection<ValuePair> Values { get; set; }
        public int ParameterId { get; set; }
        public string Parameter { get; set; }
    }

    public class ValuePair
    {
        public string ValueId { get; set; }
        public string Value { get; set; }
    }

    public class IsoSearchLocale
    {
        public string Site { get; set; }
        public string Language { get; set; }
        public string Currency { get; set; }
        public string ShipToCountry { get; set; }
    }

}
