namespace Binner.Model.Requests
{
    public class RemoveBomPartRequest
    {
        public string? Project { get; set; }
        public int? ProjectId { get; set; }

        public ICollection<long> Ids { get; set; } = new List<long>();
    }
}
