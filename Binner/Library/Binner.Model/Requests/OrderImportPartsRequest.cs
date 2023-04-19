namespace Binner.Model.Requests
{
    public class OrderImportPartsRequest
    {
        /// <summary>
        /// Supplier specific Order Id
        /// </summary>
        public string? OrderId { get; set; }

        /// <summary>
        /// Name of supplier
        /// </summary>
        public string? Supplier { get; set; }

        /// <summary>
        /// List of parts to import
        /// </summary>
        public ICollection<CommonPart>? Parts { get; set; }
    }
}
