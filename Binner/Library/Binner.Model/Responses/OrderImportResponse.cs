namespace Binner.Model.Responses
{
    public class OrderImportResponse
    {
        public string? OrderId { get; set; }

        /// <summary>
        /// Supplier specific InvoiceId (DigiKey specific, optional)
        /// </summary>
        public string? Invoice { get; set; }

        /// <summary>
        /// Supplier specific Packlist (DigiKey specific, optional)
        /// </summary>
        public string? Packlist { get; set; }

        public string? Supplier { get; set; }

        public ICollection<ImportPartResponse> Parts { get; set; } = new List<ImportPartResponse>();
    }

    public class ImportPartResponse : CommonPart
    {
        public long QuantityExisting { get; set; }
        public long QuantityAdded { get; set; }
        public bool IsImported { get; set; }
        public string ErrorMessage { get; set; } = null!;
    }
}
