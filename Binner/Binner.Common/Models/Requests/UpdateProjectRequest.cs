namespace Binner.Common.Models
{
    public class UpdateProjectRequest
    {
        /// <summary>
        /// Project id
        /// </summary>
        public long ProjectId { get; set; }

        /// <summary>
        /// Name of project
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of project
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Location of project
        /// </summary>
        public string Location { get; set; }
    }
}
