namespace Binner.Model.Integrations.DigiKey.V4
{
    public class KeywordSearchResponse
    {
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public int ProductsCount { get; set; }
        public ICollection<Product> ExactMatches { get; set; } = new List<Product>();
        public FilterOptions FilterOptions { get; set; } = new();
        public IsoSearchLocale SearchLocaleUsed { get; set; } = new IsoSearchLocale();
        public ICollection<Parameter> AppliedParametricFiltersDto { get; set; } = new List<Parameter>();
    }
}
