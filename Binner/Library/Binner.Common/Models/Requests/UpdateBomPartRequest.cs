namespace Binner.Common.Models.Requests
{
    public class UpdateBomPartRequest
    {
        public string? Project { get; set; }
        public int? ProjectId { get; set; }

        /// <summary>
        /// Part number to add
        /// </summary>
        public string? PartNumber { get; set; }

        /// <summary>
        /// Quantity of part required for BOM
        /// </summary>
        public int? Quantity { get; set; }

        /// <summary>
        /// Note for BOM part
        /// </summary>
        public string? Note { get; set; }
    }
}
