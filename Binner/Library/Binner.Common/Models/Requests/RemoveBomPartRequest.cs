namespace Binner.Common.Models.Requests
{
    public class RemoveBomPartRequest
    {
        public string? Project { get; set; }
        public int? ProjectId { get; set; }

        /// <summary>
        /// Part number to remove
        /// </summary>
        public string? PartNumber { get; set; }
    }
}
