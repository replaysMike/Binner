namespace Binner.Model.Responses
{
    public class ExternalOrderResponse
    {
        public string? OrderId { get; set; }
        public string? Supplier { get; set; }
        public DateTime OrderDate { get; set; }
        public double Amount { get; set; }
        public string? Currency { get; set; }
        public string? CustomerId { get; set; }
        public string? TrackingNumber { get; set; }
        public IEnumerable<Message> Messages { get; set; } = new List<Message>();
        public ICollection<CommonPart> Parts { get; set; } = new List<CommonPart>();
    }

    public class Message
    {
        public bool IsError { get; set; }
        public string Description { get; set; } = null!;

        public static Message FromError(string description) => new Message()
        {
            IsError = true,
            Description = description
        };

        public static Message FromInfo(string description) => new Message()
        {
            IsError = false,
            Description = description
        };
    }
}
