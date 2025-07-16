namespace Binner.Model
{
    public class UpdateSystemMessagesRequest
    {
        public ICollection<Guid> MessageIds { get; set; } = new List<Guid>();
    }
}
