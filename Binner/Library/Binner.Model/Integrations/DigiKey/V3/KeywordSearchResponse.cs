namespace Binner.Model.Integrations.DigiKey.V3
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
}
