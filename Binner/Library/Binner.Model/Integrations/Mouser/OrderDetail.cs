namespace Binner.Model.Integrations.Mouser
{
    public class OrderDetail
    {
        public ICollection<OrderLineItem> OrderLines { get; set; } = new List<OrderLineItem>();
        public string? SalesOrderId { get; set; }
        public int? WebOrderId { get; set; }
        public int OrderStatus { get; set; }
        public string? OrderStatusName { get; set; }
        public DateTime OrderDate { get; set; }
        public AddressModel? BillingAddress { get; set; }
        public AddressModel? ShippingAddress { get; set; }
        public PaymentModel? PaymentDetail { get; set; }
        public DeliveryModel? DeliveryDetail { get; set; }
        public string? CurrencyCode { get; set; }
        public bool IsScheduled { get; set; }
        public bool IsPendingOrder { get; set; }
        public string? BuyerName { get; set; }
        public OrderDetailSummary SummaryDetail { get; set; } = new OrderDetailSummary();
    }

    public class OrderLineItem
    {
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double ExtPrice { get; set; }
        public string? FormattedUnitPrice { get; set; }
        public string? FormattedExtendedPrice { get; set; }
        public ICollection<OHAdditionalFee> AdditionalFees { get; set; } = new List<OHAdditionalFee>();
        public OrderLineProduct ProductInfo { get; set; } = new OrderLineProduct();
        public ICollection<OrderLineActivity> Activities { get; set; } = new List<OrderLineActivity>();
    }

    public class AddressModel
    {
        public string? CountryCode { get; set; }
        public string? AttentionLine { get; set; }
        public string? CompanyName { get; set; }
        public string? AddressOne { get; set; }
        public string? AddressTwo { get; set; }
        public string? City { get; set; }
        public string? StateOrProvince { get; set; }
        public string? PostalCode { get; set; }
    }

    public class OHAdditionalFee
    {
        public string? Amount { get; set; }
        public string? ExtendedAmount { get; set; }
        public string? Code { get; set; }
    }

    public class OrderLineProduct
    {
        public string? MouserPartNumber { get; set; }
        public string? CustomerPartNumber { get; set; }
        public string? ManufacturerName { get; set; }
        public string? ManufacturerPartNumber { get; set; }
        public string? PartDescription { get; set; }
    }

    public class OrderLineActivity
    {
        public int? InvoiceNumber { get; set; }
        public DateTime Date { get; set; }
    }

    public class OrderDetailSummary
    {
        public string? MerchandiseTotal { get; set; }
        public string? OrderTotal { get; set; }
        public string? AdditionalFeesTotal { get; set; }
    }

    public class PaymentModel
    {
        public string? PoNumber { get; set; }
        public string? PaymentMethodName { get; set; }
    }

    public class DeliveryModel
    {
        public string? ShippingMethodName { get; set; }
        public string? BackOrderShippingMethodName { get; set; }
        public ICollection<TrackingModel> TrackingDetails { get; set; } = new List<TrackingModel>();
    }

    public class TrackingModel
    {
        public string? Number { get; set; }
        public string? Link { get; set; }
    }
}
