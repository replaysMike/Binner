using System;

namespace Binner.Common.Models
{
    /// <summary>
    /// A user defined project
    /// </summary>
    public class PartTypeResponse
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public long PartTypeId { get; set; }

        /// <summary>
        /// Project name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The parent part type
        /// </summary>
        public long? ParentPartTypeId { get; set; }

        /// <summary>
        /// The number of parts assigned to the project
        /// </summary>
        public long Parts { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; } = DateTime.UtcNow;
    }
}
