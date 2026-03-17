namespace Binner.Model.Integrations.Mouser
{
    public class OrderHistoryResponseRoot
    {
        public List<ErrorEntity> Errors { get; set; } = new List<ErrorEntity>();
        public int NumberOfOrders { get; set; }
        public List<OrderHistoryBaseObject> OrderHistoryItems { get; set; } = new List<OrderHistoryBaseObject>();
    }

    public class OrderHistoryBaseObject
    {
        public DateTime DateCreated { get; set; }
        public string SalesOrderNumber { get; set; } = string.Empty;
        public string WebOrderNumber { get; set; } = string.Empty;
        public string PoNumber { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty;
        public string OrderStatusDisplay { get; set; } = string.Empty;
    }
}
