namespace Binner.Model.Swarm
{
    /// <summary>
    /// Part within a circuit
    /// </summary>
    public class CircuitPartAssignment
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int CircuitPartAssignmentId { get; set; }

        /// <summary>
        /// The part number that belongs to the circuit, if known.
        /// Assignments can be created with a PartName only, which will not be associated to an internal part.
        /// </summary>
        public int? PartNumberId { get; set; }

        /// <summary>
        /// The manufacturer part number that belongs to the circuit, if known.
        /// Assignments can be created with a PartName only, which will not be associated to an internal part.
        /// </summary>
        public int? PartNumberManufacturerId { get; set; }

        /// <summary>
        /// If PartNumberId / PartNumberManufacturerId is not known, the text name of the part
        /// </summary>
        public string? PartName { get; set; }

        /// <summary>
        /// If text part type of the part
        /// </summary>
        public string? PartType { get; set; }

        /// <summary>
        /// The quantity of this part required for the circuit design
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// The reference label of this part on the circuit - i.e. Q1, R1, C1 etc.
        /// </summary>
        public string? Reference { get; set; }

        /// <summary>
        /// Optional description for drawing the part
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Optional color for drawing the reference
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// True if this part came directly from AI generated values
        /// </summary>
        public bool IsAiGenerated { get; set; }

        public PartNumberSimple? PartNumber { get; set; }
        public PartNumberManufacturerSimple? PartNumberManufacturer { get; set; }
    }
}
