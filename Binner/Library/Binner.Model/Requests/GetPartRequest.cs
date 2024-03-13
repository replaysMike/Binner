namespace Binner.Model.Requests
{
    public class GetPartRequest
    {
        /// <summary>
        /// The main part number
        /// </summary>
        public string? PartNumber { get; set; }

        /// <summary>
        /// Optional part id
        /// </summary>
        public long PartId { get; set; }
    }
}
