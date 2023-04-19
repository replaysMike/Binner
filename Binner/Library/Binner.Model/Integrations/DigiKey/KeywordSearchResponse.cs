namespace Binner.Model.Integrations.DigiKey
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
        public string DigiKeyPartNumber { get; set; } = null!;
        public int QuantityAvailable { get; set; }
        public string ManufacturerPartNumber { get; set; } = null!;
        public Manufacturer? Manufacturer { get; set; }
        public string? ProductDescription { get; set; }
        public string? DetailedDescription { get; set; }
        public int MinimumOrderQuantity { get; set; }
        public bool NonStock { get; set; }
        public int QuantityOnOrder { get; set; }
        public int ManufacturerPublicQuantity { get; set; }
        public double UnitPrice { get; set; }
        public string? ProductUrl { get; set; }
        public string? ProductStatus { get; set; }
        public string? PrimaryDatasheet { get; set; }
        public string? PrimaryPhoto { get; set; }
        public string? PrimaryVideo { get; set; }
        public string? RoHSStatus { get; set; }
        public string? LeadStatus { get; set; }
        public string? TariffDescription { get; set; } = null!;
        public Family? Family { get; set; }
        public Category? Category { get; set; }
        public Series? Series { get; set; }
        public Packaging? Packaging { get; set; }
        public ICollection<ParameterObject> Parameters { get; set; } = new List<ParameterObject>();

        public ICollection<AlternatePackaging> AlternatePackaging { get; set; } = new List<AlternatePackaging>();

        public override string ToString()
        {
            return $"{DigiKeyPartNumber} {ProductDescription} | x{QuantityAvailable} {UnitPrice}";
        }
    }

    public class Category : ParameterObject
    {
    }

    public class Family : ParameterObject
    {
    }

    public class AlternatePackaging : Product
    {
        public int MaxQuantityForDistribution { get; set; }
        public bool BackOrderNotAllowed { get; set; }
        public bool DKPlusRestriction { get; set; }
        public bool Marketplace { get; set; }
        public bool SupplierDirectShip { get; set; }
        public string PimProductName { get; set; } = null!;
        public string Supplier { get; set; } = null!;
        public int SupplierId { get; set; }
        public bool IsNcnr { get; set; }
    }

    public class Packaging : ParameterObject
    {
    }

    public class Manufacturer : ParameterObject
    {
    }

    public class StandardPricing
    {
        public int BreakQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public override string ToString()
        {
            return $"{BreakQuantity} = {UnitPrice}";
        }
    }

    public class Series : ParameterObject
    {
    }

    public class ParameterObject
    {
        public int ParameterId { get; set; }
        public string ValueId { get; set; } = null!;
        public string Parameter { get; set; } = null!;
        public string? Value { get; set; }
        public override string ToString()
        {
            return $"{Parameter} = {Value}";
        }
    }

    public class LimitedTaxonomy
    {
        public ICollection<LimitedTaxonomy>? Children { get; set; }
        public int ProductCount { get; set; }
        public int NewProductCount { get; set; }
        public int ParameterId { get; set; }
        public string? ValueId { get; set; }
        public string? Parameter { get; set; }
        public string? Value { get; set; }
        public override string ToString()
        {
            return $"{Parameter} = {Value}";
        }
    }

    public class LimitedParameter
    {
        public ICollection<ValuePair>? Values { get; set; }
        public int ParameterId { get; set; }
        public string? Parameter { get; set; }
        public override string ToString()
        {
            if (Values != null)
                return $"{Parameter} = {string.Join(",", Values)}";
            return $"{Parameter} = ";
        }
    }

    public class ValuePair
    {
        public string? ValueId { get; set; }
        public string? Value { get; set; }
        public override string ToString()
        {
            return $"{ValueId} = {Value}";
        }
    }

    public class IsoSearchLocale
    {
        public string? Site { get; set; }
        public string? Language { get; set; }
        public string? Currency { get; set; }
        public string? ShipToCountry { get; set; }
        public override string ToString()
        {
            return $"{Site} {Language} {Currency}";
        }
    }

}
