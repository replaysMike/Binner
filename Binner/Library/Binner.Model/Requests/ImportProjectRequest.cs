using Microsoft.AspNetCore.Http;

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
        /// File to import
        /// </summary>
        public IFormFile? File { get; set; }
    }
}