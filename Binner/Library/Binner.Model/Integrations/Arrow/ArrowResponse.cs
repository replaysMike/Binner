using Newtonsoft.Json;

namespace Binner.Model.Integrations.Arrow
{
    public class ArrowResponse
    {
        [JsonProperty("itemserviceresult")]
        public ItemServiceResult? ItemServiceResult { get; set; }
    }

    public class ItemServiceResult
    {
        public ICollection<IDictionary<string, string?>>? ServiceMetaData { get; } = new List<IDictionary<string, string?>>();
        public ICollection<TransactionAreaRow>? TransactionArea = new List<TransactionAreaRow>();
        public ICollection<Data?>? Data { get; set; }
    }

    public class Data
    {
        public ICollection<Resource> Resources { get; set; } = new List<Resource>();
        public ICollection<Part>? PartList { get; set; } = new List<Part>();
    }

    public class Part
    {
        public int ItemId { get; set; }
        public string? PartNum { get; set; }
        public Manufacturer? Manufacturer { get; set; }
        public string? Desc { get; set; }
        public string? PackageType { get; set; }
        public ICollection<Resource> Resources { get; set; } = new List<Resource>();
        public EnvData? EnvData { get; set; }
        public InvOrg? InvOrg { get; set; }
        public bool HasDatasheet { get; set; }
        public string? CategoryName { get; set; }
        public string? Status { get; set; }
    }

    public class InvOrg
    {
        public ICollection<Website> WebSites { get; set; } = new List<Website>();
    }

    public class Website
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public ICollection<Source> Sources { get; set; } = new List<Source>();
    }

    public class Source
    {
        public string? Currency { get; set; }
        public string? SourceCd { get; set; }
        public string? DisplayName { get; set; }
        public ICollection<SourcePart> SourceParts { get; set; } = new List<SourcePart>();
    }

    public class SourcePart
    {
        public int PackSize { get; set; }
        public int MinimumOrderQuantity { get; set; }
        public string? SourcePartId { get; set; }
        public Prices Prices { get; set; } = new Prices();
        public ICollection<AvailabilityItem> Availability { get; set; } = new List<AvailabilityItem>();
        public ICollection<CustomerSpecificPricing> CustomerSpecificPricing { get; set; } = new List<CustomerSpecificPricing>();
        public ICollection<CustomerSpecificInventory> CustomerSpecificInventory { get; set; } = new List<CustomerSpecificInventory>();
        public string? DateCode { get; set; }
        public ICollection<Resource> Resources { get; set; } = new List<Resource>();
        public bool InStock { get; set; }
        public int MfrLeadTime { get; set; }
        public bool TariffFlag { get; set; }
        public string? ShipsFrom { get; set; }
        public string? ShipsIn { get; set; }
        public string? ArrowLeadTime { get; set; }
        public bool IsNcnr { get; set; }
        public bool IsNpi { get; set; }
        public string? ProductCode { get; set; }
        public string? ContainerType { get; set; }
    }

    public class Prices
    {
        public ICollection<PriceItem> ResaleList { get; set; } = new List<PriceItem>();
    }

    public class CustomerSpecificInventory
    {

    }

    public class CustomerSpecificPricing
    {

    }

    public class AvailabilityItem
    {
        public int FohQty { get; set; }
        public string? AvailabilityCd { get; set; }
        public string? AvailabilityMessage { get; set; }
        public ICollection<Pipeline> Pipeline { get; set; } = new List<Pipeline>();
    }

    public class Pipeline
    {

    }

    public class PriceItem
    {
        public string? DisplayPrice { get; set; }
        public double Price { get; set; }
        public int MinQty { get; set; }
        public int MaxQty { get; set; }
    }

    public class EnvData
    {
        public ICollection<ComplianceValue> Compliance { get; set; } = new List<ComplianceValue>();
    }

    public class ComplianceValue
    {
        public string? DisplayLabel { get; set; }
        public string? DisplayValue { get; set; }
    }

    public class Manufacturer
    {
        public string? MfrCd { get; set; }
        public string? MfrName { get; set; }
    }

    public class TransactionAreaRow
    {
        public ResponseDetails? Response { get; set; }
        public ResponseSequence? ResponseSequence { get; set; }
    }

    public class ResponseDetails
    {
        public string? ReturnCode { get; set; }
        public string? ReturnMsg { get; set; }
        public bool Success { get; set; }
    }

    public class ResponseSequence
    {
        public string? TransactionTime { get; set; }
        public string? QueryTime { get; set; }
        public string? DbTime { get; set; }
        public int TotalItems { get; set; }
        public ICollection<Resource>? Resources { get; set; }
        public int Qq { get; set; }
    }

    public class Resource
    {
        public string? Type { get; set; }
        public string? Uri { get; set; }
    }
}
