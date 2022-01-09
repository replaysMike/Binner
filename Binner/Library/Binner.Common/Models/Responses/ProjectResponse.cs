using System;

namespace Binner.Common.Models
{
    /// <summary>
    /// A user defined project
    /// </summary>
    public class ProjectResponse
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public long ProjectId { get; set; }

        /// <summary>
        /// Project name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Project description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Project location
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// The number of parts assigned to the project
        /// </summary>
        public long Parts { get; set; }

        /// <summary>
        /// Project color
        /// </summary>
        public int Color { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; } = DateTime.UtcNow;
    }
}
