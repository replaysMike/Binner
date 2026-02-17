namespace Binner.Model.Requests
{
    public class DeletePartsRequest
    {
        public ICollection<long> PartIds { get; set; } = new List<long>();
    }
}
