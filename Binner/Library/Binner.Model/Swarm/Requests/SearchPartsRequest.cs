namespace Binner.Model.Swarm.Requests
{
    /// <summary>
    /// A parts search request
    /// </summary>
    public class SearchPartsRequest
    {
        /// <summary>
        /// Part Number
        /// </summary>
        public string? PartNumber { get; set; }

        /// <summary>
        /// Type of part
        /// </summary>
        public string? PartType { get; set; }

        /// <summary>
        /// Mounting type
        /// </summary>
        public string? MountingType { get; set; }

        /// <summary>
        /// Max number of records to return
        /// </summary>
        public int MaxRecords { get; set; } = 25;

        public SearchPartsRequest() { }

        public SearchPartsRequest(string partNumber, string partType, string mountingType)
        {
            PartNumber = partNumber;
            PartType = partType;
            MountingType = mountingType;
        }
    }
}
