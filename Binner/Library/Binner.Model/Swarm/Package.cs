namespace Binner.Model.Swarm
{
    public class Package
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int PackageId { get; set; }

        /// <summary>
        /// Associated part number
        /// </summary>
        public int? PartNumberId { get; set; }

        /// <summary>
        /// Package name
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Number of pins on part
        /// </summary>
        public int PinCount { get; set; }

        /// <summary>
        /// Identifies the number of pin rows
        /// Quad packages = 4, DIP/SOP/SOIC = 2, TO packages = 1, > 4 = BGA/LGA
        /// </summary>
        public int PinRows { get; set; }

        /// <summary>
        /// Identifies the number of pin rows
        /// > 1 = BGA/LGA. Normally pins only go in 1 direction.
        /// </summary>
        public int PinColumns { get; set; }

        /// <summary>
        /// The pin spacing in mm
        /// </summary>
        public double PinSpacingMm { get; set; }

        /// <summary>
        /// Width in millimeters
        /// </summary>
        public double SizeWidthMm { get; set; }

        /// <summary>
        /// Height in millimeters
        /// </summary>
        public double SizeHeightMm { get; set; }

        /// <summary>
        /// Depth in millimeters
        /// </summary>
        public double SizeDepthMm { get; set; }

        public Guid GlobalId { get; set; }
    }
}
