namespace Binner.Model.Requests
{
    public class OrderImportPartsRequest
    {
        /// <summary>
        /// Supplier specific Order Id
        /// </summary>
        public string OrderId { get; set; } = null!;

        /// <summary>
        /// Supplier specific InvoiceId (DigiKey specific, optional)
        /// </summary>
        public string? Invoice { get; set; }

        /// <summary>
        /// Supplier specific Packlist (DigiKey specific, optional)
        /// </summary>
        public string? Packlist { get; set; }

        /// <summary>
        /// Name of supplier
        /// </summary>
        public string Supplier { get; set; } = null!;

        /// <summary>
        /// List of parts to import
        /// </summary>
        public ICollection<CommonPart>? Parts { get; set; }
    }
}
