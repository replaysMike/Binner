namespace Binner.Model.Requests
{
    public class UpdatePartsRequest
    {
        public ICollection<Part> Parts { get; set; } = new List<Part>();
    }
}
