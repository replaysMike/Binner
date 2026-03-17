namespace Binner.Model.Integrations.DigiKey.V3
{
    public class SearchOrdersResponse
    {
        public int TotalOrders { get; set; }
        public List<OrderSearchResponse> Orders { get; set; } = new List<OrderSearchResponse>();
    }
}
