namespace Binner.Model.Responses
{
    /// <summary>
    /// A user defined project
    /// </summary>
    public class BomBasicResponse
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public long ProjectId { get; set; }

        /// <summary>
        /// Project name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Project description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Project location
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Project color
        /// </summary>
        public int Color { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateModifiedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// List of parts in the project
        /// </summary>
        public int PartCount { get; set; }

        /// <summary>
        /// Number of pcb's in the project
        /// </summary>
        public int PcbCount { get; set; }
    }
}
