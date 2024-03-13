namespace Binner.Model.Integrations.Mouser
{
    public class OrderHistory
    {
        public ICollection<OrderHistoryLine> OrderLines { get; set; } = new List<OrderHistoryLine>();
        public string? SalesOrderId { get; set; }
        public string? WebOrderId { get; set; }
        public int OrderStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderAddressType? BillingAddress { get; set; }
        public OrderAddressType? ShippingAddress { get; set; }
        public PaymentDetail? PaymentDetail { get; set; }
        public DeliveryDetail? DeliveryDetail { get; set; }
        public string? CurrencyCode { get; set; }
        public bool IsScheduled { get; set; }
        public bool IsPendingOrder { get; set; }
        public string? BuyerName { get; set; }
        public SummaryDetail? SummaryDetail { get; set; }
    }

    public class OrderHistoryLine
    {
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double ExtPrice { get; set; }
        public string? FormattedUnitPrice { get; set; }
        public string? FormattedExtendedPrice { get; set; }
        public ProductInfo ProductInfo { get; set; } = new ProductInfo();
        public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    }

    public class ProductInfo
    {
        public string? MouserPartNumber { get; set; }
        public string? CustomerPartNumber { get; set; }
        public string? ManufacturerName { get; set; }
        public string? ManufacturerPartNumber { get; set; }
        public string? PartDescription { get; set; }
    }

    public class Activity
    {
        public string? InvoiceNumber { get; set; }
        public DateTime Date { get; set; }
    }

    public class SummaryDetail
    {
        public string? MerchandiseTotal { get; set; }
        public string? OrderTotal { get; set; }
    }

    public class PaymentDetail
    {
        public string? PoNumber { get; set; }
        public string? PaymentMethodName { get; set; }
    }

    public class DeliveryDetail
    {
        public string? ShippingMethodName { get; set; }
        public string? BackOrderShippingMethodName { get; set; }
    }
}
