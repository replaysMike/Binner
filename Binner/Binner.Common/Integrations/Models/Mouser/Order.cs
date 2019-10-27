using System.Collections.Generic;

namespace Binner.Common.Integrations.Models.Mouser
{
    public class Order
    {
        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
        public string CurrencyCode { get; set; }
        public decimal MerchandiseTotal { get; set; }
        public decimal OrderTotal { get; set; }
        public string OrderType { get; set; }
        public string CartGUID { get; set; }
        public OrderAddressType BillingAddress { get; set; }
        public OrderAddressType ShippingAddress { get; set; }
        public OrderShipping ShippingMethod { get; set; }
        public OrderPayment PaymentMethod { get; set; }
        public decimal TaxAmount { get; set; }
        public string OrderID { get; set; }
        public string TaxCertificateId { get; set; }
        public ICollection<ErrorEntity> Errors { get; set; } = new List<ErrorEntity>();
        public bool SubmitOrder { get; set; }
    }

    public class OrderLine
    {
        public ICollection<ErrorEntity> Errors { get; set; } = new List<ErrorEntity>();
        public int MouserATS { get; set; }
        public int Quantity { get; set; }
        public int PartsPerReel { get; set; }
        public IDictionary<string, int> ScheduledReleases { get; set; } = new Dictionary<string, int>();
        public ICollection<string> InfoMessages { get; set; } = new List<string>();
        public string MouserPartNumber { get; set; }
        public string MfrPartNumber { get; set; }
        public string Description { get; set; }
        public string CartItemCustPartNumber { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal ExtendedPrice { get; set; }
        public string LifeCycle { get; set; }
        public string Manufacturer { get; set; }
        public int SalesMultipleQty { get; set; }
        public int SalesMinimumOrderQty { get; set; }
    }

    public class ErrorEntity
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string ResourceKey { get; set; }
        public string ResourceFormatString { get; set; }
        public string ResourceFormatString2 { get; set; }
        public string PropertyName { get; set; }
    }

    public class ApiError
    {
        public string ErrorType { get; set; }
        public ICollection<ErrorEntity> Errors { get; set; } = new List<ErrorEntity>();
    }

    public class OrderAddressType
    {
        public ApiError Error { get; set; }
        public string AddressLocationTypeID { get; set; }
        public string CountryCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AttentionLine { get; set; }
        public string Company { get; set; }
        public string AddressOne { get; set; }
        public string AddressTwo { get; set; }
        public string City { get; set; }
        public string StateOrProvince { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneExtension { get; set; }
        public string EmailAddress { get; set; }
    }

    public class OrderShipping
    {
        public ApiError Error { get; set; }
        public int PrimaryCode { get; set; }
        public int SecondaryCode { get; set; }
        public string PrimaryMethod { get; set; }
        public string SecondaryMethod { get; set; }
        public decimal PrimaryShippingRate { get; set; }
        public decimal SecondaryShippingRate { get; set; }
        public string PrimaryFreightCollectAccount { get; set; }
        public string SecondaryFreightCollectAccount { get; set; }
    }

    public class OrderPayment
    {
        public ApiError Error { get; set; }
        public string PONumber { get; set; }
        public int Method { get; set; }
        public string Name { get; set; }
        public string VatAccountNumber { get; set; }
        public OrderAddressType VatInvoiceAddress { get; set; }

    }
}
