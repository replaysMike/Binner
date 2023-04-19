namespace Binner.Model.Integrations.Arrow
{
    public class OrderResponse
    {
        public string? No { get; set; }
        public string? InvoiceNo { get; set; }
        public string? CurrencyCode { get; set; }
        public string? CustomerNo { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentToken { get; set; }
        public string? BillToAddressId { get; set; }
        public string? ShipToAddressId { get; set; }
        public string? UcAddressId { get; set; }
        public string? DiscountCode { get; set; }
        public string? DiscountDescription { get; set; }
        public string? PoNumber { get; set; }
        public string? IpAddress { get; set; }
        public string? GaCustomerId { get; set; }
        public string? GaCampaignId { get; set; }
        public string? Source { get; set; }
        public double? TotalBeforeDiscountAmount { get; set; }
        public double? TotalBeforeDiscountAmountLCY { get; set; }
        public double? TotalDiscountAmount { get; set; }
        public double? TotalDiscountAmountLCY { get; set; }
        public double? ExchangeRate { get; set; }
        public double? TotalAmount { get; set; }
        public double? TotalAmountLCY { get; set; }
        public double? ItemTotalAmount { get; set; }
        public double? ItemTotalAmountLCY { get; set; }
        public double? FeeTotalAmount { get; set; }
        public double? FeeTotalAmountLCY { get; set; }
        public double? TaxTotalAmount { get; set; }
        public double? TaxTotalAmountLCY { get; set; }
        public string? TaxType { get; set; }
        public string? CustomerType { get; set; }
        public string? CommunicatedStatus { get; set; }
        public long OrderDate { get; set; }
        public long? ShipmentDate { get; set; }
        public string? ArrowCustomerId { get; set; }
        public string? WebCustomer { get; set; }
        public Address? BillToAddress { get; set; }
        public Address? ShipToAddress { get; set; }
        public Address? UcToAddress { get; set; }
        public string? CreditCard { get; set; }
        public ICollection<WebItem> WebItems { get; set; } = new List<WebItem>();
        public ICollection<WebShipment> WebShipments { get; set; } = new List<WebShipment>();
        public ICollection<ShipmentOption> WebShippingOptions { get; set; } = new List<ShipmentOption>();
        public ICollection<WebFee> WebFees { get; set; } = new List<WebFee>();
        public bool HasError { get; set; }
    }

    public class WebShipment
    {
        public int ShipmentNo { get; set; }
        public string? InvoiceNo { get; set; }
        public string? Status { get; set; }
        public int ShipmentDate { get; set; }
        public double UnitPrice { get; set; }
        public double UnitPriceLCY { get; set; }
        public double Amount { get; set; }
        public double AmountLCY { get; set; }
        public ICollection<WebItem> Items { get; set; } = new List<WebItem>();
        public ICollection<ShipmentOption> ShippingOptions { get; set; } = new List<ShipmentOption>();
    }

    public class ShipmentOption
    {
        public bool International { get; set; }
        public string? Agent { get; set; }
        public string? Service { get; set; }
        public string? Description { get; set; }
        public string? AccountNo { get; set; }
        public double UnitPriceLCY { get; set; }
        public double UnitPrice { get; set; }
        public double TotalPriceLCY { get; set; }
        public double TotalPrice { get; set; }
        public int DeliveryDays { get; set; }
        public int OldEntryNo { get; set; }
        public int? EstimatedShipmentDate { get; set; }
        public int? EstimatedDeliveryDate { get; set; }
        public string? TrackingNo { get; set; }
        public bool DefaultOpt { get; set; }
        public bool FastestCheapest { get; set; }
    }

    public class WebItem
    {
        public int LineNo { get; set; }
        public int ShipmentNo { get; set; }
        public string? ItemNo { get; set; }
        public string? Description { get; set; }
        public string? UnitOfMeasure { get; set; }
        public string? ShipsFromCountry { get; set; }
        public double Quantity { get; set; }
        public double? UnitCostLCY { get; set; }
        public bool CustomerSpecificPrice { get; set; }
        public double UnitPrice { get; set; }
        public double UnitPriceLCY { get; set; }
        public double Amount { get; set; }
        public double AmountLCY { get; set; }
        public string? CustomerPartNo { get; set; }
        public Detail? Detail { get; set; }
        public ICollection<WebFee> WebFees { get; set; } = new List<WebFee>();
    }

    public class WebFee
    {
        public int ShipmentNo { get; set; }
        public string? FeeType { get; set; }
        public string? No { get; set; }
        public string? AttachedToItemNo { get; set; }
        public string? Description { get; set; }
        public string? UnitOfMeasure { get; set; }
        public double Quantity { get; set; }
        public double? UnitCostLCY { get; set; }
        public double UnitPriceLCY { get; set; }
        public double UnitPrice { get; set; }
        public double AmountLCY { get; set; }
        public double Amount { get; set; }
        public double AmountIncludingTaxLCY { get; set; }
        public double AmountIncludingTax { get; set; }
        public double DiscountAmountLCY { get; set; }
        public double DiscountAmount { get; set; }
    }

    public class Detail
    {
        public string? BaseUnitOfMeasure { get; set; }
        public double MinimumOrderQuantity { get; set; }
        public double OrderMultiple { get; set; }
        public string? PartId { get; set; }
        public string? ManufacturerId { get; set; }
        public string? ManufacturerName { get; set; }
        public string? Mpn { get; set; }
        public string? VendorSiteLocation { get; set; }
        public string? VendorItemNo { get; set; }
        public string? VendorIpn { get; set; }
        public string? VendorItemPedigreeRating { get; set; }
        public int VendorSiteShippingLeadTime { get; set; }
        public int QuantityAvailable { get; set; }
        public double VendorPrice { get; set; }
        public string? CountryOfOrigin { get; set; }
        public bool CustomReelFlag { get; set; }
        public double? CustomReelUnitPriceLCY { get; set; }
        public bool ChinaTariffFlag { get; set; }
        public double? ChinaTariffUnitPriceLCY { get; set; }
        public string? DateCode { get; set; }
        public string? PackageType { get; set; }
        public string? ErpSiteId { get; set; }
        public string? ErpWarehouseId { get; set; }
        public string? ErpWarehouseName { get; set; }
        public string? ItemStatus { get; set; }
        public bool LeadFree { get; set; }
        public bool BackOrderItem { get; set; }
        public bool ShipAndDebitItem { get; set; }
        public bool RohsCompliant { get; set; }
        public bool Audited { get; set; }
        public string? OctopartId { get; set; }
        public string? DatasheetUrl { get; set; }
        public string? EccnCode { get; set; }
        public string? HtsCode { get; set; }
        public string? PartCategoryId { get; set; }
        public string? PartCategoryName { get; set; }

    }

    public class Address
    {
        public string? CustomerNo { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Company { get; set; }
        public string? Street1 { get; set; }
        public string? Street2 { get; set; }
        public string? City { get; set; }
        public string? StateProvince { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? PhoneNo { get; set; }
        public string? EmailAddress { get; set; }
        public string? WebAddressType { get; set; }
        public string? VatId { get; set; }
        public string? CrCode { get; set; }
        public bool UltimateConsignee { get; set; }
        public bool BillingAddress { get; set; }
        public bool ShippingAddress { get; set; }
        public bool TermsAddress { get; set; }
        public string? ResellerCertFilename { get; set; }
        public string? ResellerCertExpirationDate { get; set; }
        public string? ResellerCertStatus { get; set; }
        public string? ResellerCertAttachmentno { get; set; }
        public bool ResellerCertUploaded { get; set; }
        public bool ResellerCertAttached { get; set; }
        public string? ResellerCert { get; set; }
        public bool DefaultBillingAddress { get; set; }
        public bool DefaultShippingAddress { get; set; }
        public bool DefaultUCAddress { get; set; }
        public bool Saved { get; set; }
    }
}
