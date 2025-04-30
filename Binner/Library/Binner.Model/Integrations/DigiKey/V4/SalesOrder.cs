namespace Binner.Model.Integrations.DigiKey.V4
{
    public class SalesOrder
    {
        public int CustomerId { get; set; }
        public Contact Contact { get; set; } = new();
        public long SalesOrderId { get; set; }
        public SalesOrderStatusInfo Status { get; set; } = new();
        public string? PurchaseOrder { get; set; }
        public double TotalPrice { get; set; }

        public DateTime DateEntered { get; set; }
        public long OrderNumber { get; set; }
        public string? ShipMethod { get; set; }
        public string Currency { get; set; } = string.Empty;
        public Address ShippingAddress { get; set; } = new();
        public ICollection<LineItem> LineItems { get; set; } = new List<LineItem>();
    }

    public class Contact
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
    }

    public class SalesOrderStatusInfo
    {
        public SalesOrderStatus SalesOrderStatus { get; set; }
        public string? ShortDescription { get; set; }
        public string? LongDescription { get; set; }
    }

    public enum SalesOrderStatus
    {
        Unknown,
        Received,
        Processing,
        Processing3rdParty,
        ProcessingPartialShipment,
        ProcessingAwaitingBackorders,
        ProcessingShipBackorder,
        ProcessingScheduledShipmentMultipleRelease,
        ProcessingScheduledShipmentSingleRelease,
        ProcessingScheduledShipmentMsc,
        Shipped,
        Delivered,
        GenericDelay,
        Canceled,
        Proforma,
        ActionRequiredWireTransfer
    }

    public class Address
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? CompanyName { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? ZipCode { get; set; }
        public string? IsoCode { get; set; }
        public string? Phone { get; set; }
        public int InvoiceId { get; set; }
    }

    public class LineItem
    {
        public int SalesOrderId { get; set; }
        public int DetailId { get; set; }
        public double TotalPrice { get; set; }
        public string? PurchaseOrder { get; set; }
        public string? CustomerReference { get; set; }
        public string? CountryOfOrigin { get; set; }
        public string DigiKeyProductNumber { get; set; } = string.Empty;
        public string? ManufacturerProductNumber { get; set; }
        public string? Description { get; set; }
        public PackTypes PackType { get; set; }
        public int QuantityInitialRequested { get; set; }
        public int QuantityOrdered { get; set; }
        public int QuantityShipped { get; set; }
        public int QuantityReserved { get; set; }
        public int QuantityBackOrder { get; set; }
        public double UnitPrice { get; set; }
        public string? PoLineItemNumber { get; set; }
        public ICollection<ItemShipInfo> ItemShipments { get; set; } = new List<ItemShipInfo>();
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }

    public class Schedule
    {
        public int QuantityScheduled { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? DigiKeyReleaseDate { get; set; }
    }

    public class ItemShipInfo
    {
        public int QuantityShipped { get; set; }
        public int InvoiceId { get; set; }
        public DateTime ShippedDate { get; set; }
        public string? TrackingNumber { get; set; }
        public string? ExpectedDeliveryDate { get; set; }
    }

    public enum PackTypes
    {
        None,
        TapeReel,
        CutTape,
        Bulk,
        TapeBox,
        Tube,
        Tray,
        Box,
        Bag,
        Spools,
        DigiReel,
        Strip,
        Bottle,
        Canister,
        Book,
        Dispenser,
        Sheet,
        Pail,
        Can,
        Case,
        RetailPkg,
        DigiSpool,
        ElectronicDelivery
    }
}
