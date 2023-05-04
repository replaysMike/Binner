namespace Binner.Model.Requests
{
    public class MoveBomPartRequest
    {
        public string? Project { get; set; }
        public int? ProjectId { get; set; }

        public ICollection<long> Ids { get; set; } = new List<long>();
        /// <summary>
        /// Destination to move to
        /// </summary>
        public int PcbId { get; set; }
    }
}
