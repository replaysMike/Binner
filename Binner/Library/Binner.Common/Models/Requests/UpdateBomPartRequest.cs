namespace Binner.Common.Models.Requests
{
    public class UpdateBomPartRequest
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public long ProjectPartAssignmentId { get; set; }

        /// <summary>
        /// The associated project id
        /// </summary>
        public long ProjectId { get; set; }

        /// <summary>
        /// The associated part id (optional, if using PartName)
        /// </summary>
        public long? PartId { get; set; }

        /// <summary>
        /// The associated pcb id (optional)
        /// </summary>
        public long? PcbId { get; set; }

        /// <summary>
        /// If a part number isn't matched, a custom part name can be added to identify the part by name only
        /// </summary>
        public string? PartName { get; set; }

        /// <summary>
        /// The cost of the part
        /// </summary>
        public double Cost { get; set; }

        /// <summary>
        /// Currency of part
        /// </summary>
        public string? Currency { get; set; }

        /// <summary>
        /// The quantity of parts needed (BOM)
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// The quantity of parts available, used when a part is not associated with this entry (BOM).
        /// Otherwise, the part's quantity (in stock) should be used.
        /// </summary>
        public int QuantityAvailable { get; set; }

        /// <summary>
        /// Notes about this part
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Custom reference Id that can be used as a custom designator for the part
        /// </summary>
        public string? ReferenceId { get; set; }

        /// <summary>
        /// Custom reference Id that can be used as a custom designator for the part on the PCB (silkscreen values)
        /// </summary>
        public string? SchematicReferenceId { get; set; }

        public string? CustomDescription { get; set; }

        public PartResponse? Part { get; set; }
    }
}
