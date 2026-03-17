namespace Binner.Model.Integrations.DigiKey.V4
{
    public class OrderSearchResponse
    {
        public int TotalOrders { get; set; }
        public List<Order> Orders { get; set; } = new List<Order>();
    }
}
