namespace Binner.Common.Models.Requests
{
    public class AddBomPartRequest
    {
        public string? Project { get; set; }
        public int? ProjectId { get; set; }

        /// <summary>
        /// Part number
        /// </summary>
        public string PartNumber { get; set; } = null!;

        public int? PcbId { get; set; }

        /// <summary>
        /// Quantity of part required for BOM
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Note for BOM part
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Custom reference Id for customer
        /// </summary>
        public string? ReferenceId { get; set; }
    }
}
