using Binner.Model.Swarm;

namespace Binner.Model
{
    /// <summary>
    /// Pinout images
    /// </summary>
    public class Pinout
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int PinoutId { get; set; }

        /// <summary>
        /// The part name associated with the pinout
        /// </summary>
        public string? PartName { get; set; }

        /// <summary>
        /// Associated part number
        /// </summary>
        public int? PartNumberId { get; set; }

        /// <summary>
        /// Image the pinout was generated from
        /// </summary>
        public int? ImportImageId { get; set; }

        /// <summary>
        /// Image of the rendered/generated pinout
        /// </summary>
        public int? ExportImageId { get; set; }

        /// <summary>
        /// The associated manufacturer part number
        /// </summary>
        public int? PartNumberManufacturerId { get; set; }

        /// <summary>
        /// The package name of the part
        /// </summary>
        public string? PackageName { get; set; }

        /// <summary>
        /// The package type of the part
        /// </summary>
        public int? PackageId { get; set; }

        /// <summary>
        /// True if the pinout has a pin 1 indicator
        /// </summary>
        public bool HasPin1Indicator { get; set; }

        /// <summary>
        /// The json formatted pinout definition
        /// </summary>
        public string? PinoutDefinition { get; set; }

        /// <summary>
        /// The number of pins in the package
        /// </summary>
        public int PinCount { get; set; }

        /// <summary>
        /// A transitive pinout means that it was created from a part designated to other manufacturers.
        /// It may not be accurate for this specific manufacturer part number.
        /// </summary>
        public bool IsTransitive { get; set; }

        public DateTime DateCreatedUtc { get; set; }

        public Guid GlobalId { get; set; }

        /// <summary>
        /// Manufacturer name
        /// </summary>
        public string? ManufacturerName { get; set; }

        public Image? ImportImage { get; set; }
        public Image? ExportImage { get; set; }

        /// <summary>
        /// The date this pinout was verified by a person
        /// </summary>
        public DateTime? DateVerifiedUtc { get; set; }
    }
}
