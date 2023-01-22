namespace Binner.Common.Models.Swarm.Requests
{
    public class PartRequest
    {
        public string PartNumber { get; set; } = null!;

        /// <summary>
        /// Optional comma-separated list of keywords
        /// </summary>
        public string? Keywords { get; set; }

        /// <summary>
        /// Optional serialized response type, the class/type name representing <see cref="SerializedResponse"/>
        /// </summary>
        public string? SerializedResponseType { get; set; }

        /// <summary>
        /// Optional serialized response
        /// </summary>
        public string? SerializedResponse { get; set; }
    }
}
