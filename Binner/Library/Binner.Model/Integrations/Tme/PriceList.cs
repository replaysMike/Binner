namespace Binner.Model.Integrations.Tme
{
    public class PriceList
    {
        /// <summary>
        /// - Amount from current quantity threshold
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// Price value for current quantity threshold
        /// </summary>
        public double PriceValue { get; set; }

        /// <summary>
        /// Special price for the customer
        /// </summary>
        public bool Special { get; set; }
    }
}
