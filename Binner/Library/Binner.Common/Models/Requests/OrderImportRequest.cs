namespace Binner.Common.Models
{
    public class OrderImportRequest
    {
        /// <summary>
        /// Supplier specific Order Id
        /// </summary>
        public string? OrderId { get; set; }

        /// <summary>
        /// Name of supplier
        /// </summary>
        public string? Supplier { get; set; }
    }
}
