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
        /// Alternate notation for this package.
        /// Example: Package SOIC with 8 pins is also known as 8-SOIC
        /// </summary>
        public string? AlternateNotation { get; set; }

        /// <summary>
        /// Optional comma-delimited list of alternate names
        /// </summary>
        public ICollection<string>? AlternateNames { get; set; }

        /// <summary>
        /// Optional comma-delimited list of features part of the package.
        /// </summary>
        public ICollection<string>? Features { get; set; }

        /// <summary>
        /// Number of pins on part
        /// </summary>
        public int PinCount { get; set; }

        /// <summary>
        /// Minimum number of pins in package definition (if known)
        /// </summary>
        public int MinPinCount { get; set; }

        /// <summary>
        /// Maximum number of pins in package definition (if known)
        /// </summary>
        public int MaxPinCount { get; set; }

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
