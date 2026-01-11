namespace Binner.Model.Requests
{
    public class PartQuantityRequest
    {
        /// <summary>
        /// The part id (optional)
        /// </summary>
        public long? PartId { get; set; }

        /// <summary>
        /// The part number (optional if PartId is specified)
        /// </summary>
        public string? PartNumber { get; set; }

        /// <summary>
        /// The quantity to modify
        /// </summary>
        public int Quantity { get; set; } = 1;
    }
}
