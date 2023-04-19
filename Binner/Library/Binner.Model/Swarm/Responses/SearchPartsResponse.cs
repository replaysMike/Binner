namespace Binner.Model.Swarm.Responses
{
    public class SearchPartsResponse
    {
        /// <summary>
        /// The parts returned from the search
        /// </summary>
        public ICollection<PartNumber> Parts { get; set; } = new List<PartNumber>();
    }
}
