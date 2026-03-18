namespace Binner.Model.Requests
{
    public class PartIdsRequest
    {
        public ICollection<long> PartIds { get; set; } = new List<long>();
    }
}
