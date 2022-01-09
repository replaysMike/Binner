namespace Binner.Common.Models
{
    public class CreateProjectRequest
    {
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

        /// <summary>
        /// Color of project
        /// </summary>
        public int Color { get; set; }
    }
}
