namespace Binner.Model.Requests
{
    public class ImportProjectRequest
    {
        /// <summary>
        /// Name of project
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Description of project
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// File to import from
        /// </summary>
        public string? File { get; set; }
    }
}
