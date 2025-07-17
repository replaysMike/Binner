namespace Binner.Model.Requests
{
    public class ImportProjectRequest<T>
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
        /// File to import
        /// </summary>
        public required T File { get; set; }
    }
}