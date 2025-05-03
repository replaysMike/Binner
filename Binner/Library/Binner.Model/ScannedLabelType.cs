namespace Binner.Common
{
    public enum ScannedLabelType
    {
        Unknown = 0,
        /// <summary>
        /// Product barcode
        /// </summary>
        Product,
        /// <summary>
        /// Packlist barcode
        /// </summary>
        Packlist,
        /// <summary>
        /// Line item on a packlist
        /// </summary>
        LineItem,
        /// <summary>
        /// Order barcode
        /// </summary>
        Order,
    }
}
