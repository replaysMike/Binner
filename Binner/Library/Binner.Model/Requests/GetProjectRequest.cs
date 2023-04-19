namespace Binner.Model.Requests
{
    public class GetProjectRequest
    {
        /// <summary>
        /// The project id
        /// </summary>
        public long ProjectId { get; set; }

        /// <summary>
        /// Project name
        /// </summary>
        public string? Name { get; set; }
    }
}
