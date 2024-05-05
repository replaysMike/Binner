namespace Binner.Model.Integrations.Tme
{
    public class ProductPriceList
    {
        /// <summary>
        /// Unique product identifier
        /// </summary>
        public string Symbol { get; set; } = string.Empty;

        /// <summary>
        /// Symbol of unit used to describe amount of product e.g. "pcs" (pieces)
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// Tax value (percent)
        /// </summary>
        public int VatRate { get; set; }

        /// <summary>
        /// Type of tax ("VAT" or "RC")
        /// </summary>
        public VatTypes VatType { get; set; }

        /// <summary>
        /// Amount of products in stock
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// Price list
        /// </summary>
        public List<PriceList> PriceList { get; set; } = new();
    }

    public enum VatTypes
    {
        VAT,
        RC
    }

    public enum PriceType
    {
        NET,
        GROSS
    }
}
