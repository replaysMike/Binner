namespace Binner.Model.Swarm
{
    public class Package
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int PackageId { get; set; }

        /// <summary>
        /// Package name
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Number of pins on part
        /// </summary>
        public int PinCount { get; set; }

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
    }
}
