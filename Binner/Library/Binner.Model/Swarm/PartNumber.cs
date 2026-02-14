namespace Binner.Model.Swarm
{
    public class PartNumber
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int PartNumberId { get; set; }

        /// <summary>
        /// The name of the part number
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Comma separated list of alternate names
        /// </summary>
        public string? AlternateNames { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Alternate description
        /// </summary>
        public string? AlternateDescription { get; set; }

        /// <summary>
        /// The default image for this part
        /// </summary>
        public int? DefaultImageId { get; set; }

        /// <summary>
        /// The resource server Url of the datasheet and it's associated content
        /// </summary>
        public string? DefaultImageResourceSourceUrl { get; set; }

        /// <summary>
        /// The resource path of the datasheet and it's associated content
        /// </summary>
        public string? DefaultImageResourcePath { get; set; }

        /// <summary>
        /// The default primary datasheet for this part
        /// </summary>
        public int? PrimaryDatasheetId { get; set; }

        /// <summary>
        /// Part type category
        /// </summary>
        public long? PartTypeId { get; set; }

        /// <summary>
        /// Unique resource Id.
        /// Indicates the file name & path on the resource server for associated images
        /// </summary>
        public Guid ResourceId { get; set; }

        /// <summary>
        /// The source that created this record
        /// </summary>
        public DataSource Source { get; set; } = DataSource.ManualInput;

        /// <summary>
        /// The original part number processing record that created this entry
        /// </summary>
        public int? SwarmPartNumberId { get; set; }

        /// <summary>
        /// Date record has been manually updated by an admin
        /// </summary>
        public DateTime? DatePrunedUtc { get; set; }

        /// <summary>
        /// Date added
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        /// <summary>
        /// If this part was imported from a supplier API (DigiKey, Mouser) indicate which one
        /// </summary>
        public int? CreatedFromSupplierId { get; set; }

        public Guid GlobalId { get; set; }

        /// <summary>
        /// Part numbers by manufacturer related to this part
        /// </summary>
        public ICollection<PartNumberManufacturer>? PartNumberManufacturers { get; set; }
    }
}
