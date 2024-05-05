namespace Binner.Model.Integrations.Tme
{
    public class PriceListResponse
    {
        /// <summary>
        /// Language
        /// </summary>
        public string Language { get; set; } = string.Empty;

        /// <summary>
        /// Currency
        /// </summary>
        public string Currency { get; set; } = string.Empty;

        /// <summary>
        /// Type of price provided
        /// </summary>
        public PriceType PriceType { get; set; }

        public List<ProductPriceList> ProductList { get; set; } = new();
    }
}
