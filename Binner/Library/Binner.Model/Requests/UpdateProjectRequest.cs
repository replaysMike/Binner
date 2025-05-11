﻿namespace Binner.Model.Requests
{
    public class UpdateProjectRequest : ICustomFields
    {
        /// <summary>
        /// Project id
        /// </summary>
        public long ProjectId { get; set; }

        /// <summary>
        /// Name of project
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Description of project
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Location of project
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Color of project
        /// </summary>
        public int Color { get; set; }

        public ICollection<CustomValue> CustomFields { get; set; } = new List<CustomValue>();
    }
}
