namespace Binner.Model.Requests
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
        /// Cost of part (for parts without a part match)
        /// </summary>
        public double Cost { get; set; }

        /// <summary>
        /// Currency of part
        /// </summary>
        public string? Currency { get; set; }

        /// <summary>
        /// Quantity of part required for BOM
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// The quantity of parts available, used when a part is not associated with this entry (BOM).
        /// Otherwise, the part's quantity (in stock) should be used.
        /// </summary>
        public int QuantityAvailable { get; set; }

        /// <summary>
        /// Note for BOM part
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Custom reference Id for customer
        /// </summary>
        public string? ReferenceId { get; set; }

        /// <summary>
        /// Custom reference Id that can be used as a custom designator for the part on the PCB (silkscreen values)
        /// </summary>
        public string? SchematicReferenceId { get; set; }

        public string? CustomDescription { get; set; }
    }
}
