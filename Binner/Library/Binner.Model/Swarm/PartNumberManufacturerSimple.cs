namespace Binner.Model.Swarm
{
    public class PartNumberManufacturerSimple
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int PartNumberManufacturerId { get; set; }

        /// <summary>
        /// The manufacturer's part number
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Manufacturer name
        /// </summary>
        public string? ManufacturerName { get; set; }

        /// <summary>
        /// Part type name
        /// </summary>
        public string? PartType { get; set; }
    }
}
