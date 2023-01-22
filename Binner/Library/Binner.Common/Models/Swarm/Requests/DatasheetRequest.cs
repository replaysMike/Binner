namespace Binner.Common.Models.Swarm.Requests
{
    public class DatasheetRequest
    {
        /// <summary>
        /// The Url of the datasheet
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Optional comma delimited list of keywords associated with datasheet
        /// </summary>
        public string Keywords { get; set; }


    }
}
