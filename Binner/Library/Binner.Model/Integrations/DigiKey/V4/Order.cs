namespace Binner.Model.Integrations.DigiKey.V4
{
    public class Order
    {
        public long OrderNumber { get; set; }
        public int CustomerId { get; set; }
        public DateTime DateEntered { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string PONumber { get; set; } = string.Empty;
        public OrderStatusInfo EntireOrderStatus { get; set; } = new OrderStatusInfo();
        public List<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    }

    public class OrderStatusInfo
    {
        public SalesOrderStatus Status { get; set; }
        public string? ShortDescription { get; set; }
        public string? LongDescription { get; set; }
    }
}
