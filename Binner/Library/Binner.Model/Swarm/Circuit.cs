namespace Binner.Model.Swarm
{
    /// <summary>
    /// Circuit
    /// </summary>
    public class Circuit
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int? CircuitId { get; set; }

        /// <summary>
        /// Image of the circuit
        /// </summary>
        public int? InputImageId { get; set; }

        /// <summary>
        /// Output image of the circuit
        /// </summary>
        public int? OutputImageId { get; set; }

        /// <summary>
        /// Circuit name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Circuit description
        /// </summary>
        public string? Description { get; set; }

        public DateTime DateCreatedUtc { get; set; }

        public Guid GlobalId { get; set; }

        public Image? InputImage { get; set; }

        public Image? OutputImage { get; set; }

        public ICollection<CircuitPartAssignment>? Parts { get; set; } = new List<CircuitPartAssignment>();
    }
}
