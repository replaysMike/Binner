namespace Binner.Model.Requests
{
    public class CreateBomPcbRequest
    {
        public int ProjectId { get; set; }

        /// <summary>
        /// Name of pcb
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Description of pcb
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The serial format template. Example: SN000000000
        /// </summary>
        public string? SerialNumberFormat { get; set; }

        /// <summary>
        /// The initial serial number
        /// </summary>
        public string? SerialNumber { get; set; }

        /// <summary>
        /// Quantity of PCBs that are produced on every production
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Cost to produce a single PCB board
        /// </summary>
        public double Cost { get; set; }
    }
}
