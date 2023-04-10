namespace Binner.Model
{
    public class ConnectionResponse
    {
        public bool IsSuccess { get; set; }
        public bool DatabaseExists { get; set; }
        public ICollection<string> Errors { get; set; } = new List<string>();
    }
}
