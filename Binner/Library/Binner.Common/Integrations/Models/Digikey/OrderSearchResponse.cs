using System.Collections.Generic;

namespace Binner.Common.Integrations.Models.DigiKey
{
    public class OrderSearchResponse
    {
        public int SalesorderId { get; set; }
        public int CustomerId { get; set; }
        public int BillingAccount { get; set; }
        public string Email { get; set; }
        public string PurchaseOrder { get; set; }
        public string PaymentMethod { get; set; }
        public string ShippingMethod { get; set; }
        public string BackorderShippingMethod { get; set; }
        public string ShipperAccountNumber { get; set; }
        public string BackorderShipperAccountNumber { get; set; }
        public string ShipmentType { get; set; }
        public string Currency { get; set; }
        public Address ShippingAddress { get; set; }
        public Address BillingAddress { get; set; }
        public ICollection<ShippingDetail> ShippingDetails { get; set; } = new List<ShippingDetail>();
        public ICollection<LineItem> LineItems { get; set; } = new List<LineItem>();
    }

    public class Address
    {
        public string Company { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AddressLineOne { get; set; }
        public string AddressLineTwo { get; set; }
        public string AddressLineThree { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }

    public class ShippingDetail
    {
        public string Carrier { get; set; }
        public string CarrierPackageId { get; set; }
        public string DateTransaction { get; set; }
        public string ShippingMethod { get; set; }
        public string TrackingUrl { get; set; }
        public int InvoiceId { get; set; }
        public bool CanceledOrVoided { get; set; }
    }

    public class LineItem
    {
        public string DigiKeyPartNumber { get; set; }
        public string ProductDescription { get; set; }
        public int Quantity { get; set; }
        public string CustomerReference { get; set; }
        public double UnitPrice { get; set; }
        public double TotalPrice { get; set; }
        public int QuantityBackorder { get; set; }
        public int QuantityShipped { get; set; }
        public int InvoiceId { get; set; }
    }
}
