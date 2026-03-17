namespace Binner.Model.Responses
{
    public class ExternalOrderListResponse
    {
        public List<ExternalOrderBasic> Orders { get; set; } = new List<ExternalOrderBasic>();
    }

    public class ExternalOrderBasic
    {
        public string? OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public int OrderItemsTotal { get; set; }
        public string? OrderStatus { get; set; }
    }
}
