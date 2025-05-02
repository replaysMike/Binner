namespace Binner.Model.Responses
{
    public class BulkPartResponse
    {
        public ICollection<PartResponse> Added { get; set; } = new List<PartResponse>();
        public ICollection<PartResponse> Updated { get; set; } = new List<PartResponse>();
    }
}
