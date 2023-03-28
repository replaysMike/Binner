using System.Collections.Generic;

namespace Binner.Common.Integrations.Models.Nexar
{
    /*
     * Use the Nexar portal IDE to view/create graphQl queries/responses
     * https://portal.nexar.com/applications/
     */

    public class NexarPartResults
    {
        public ICollection<NexarPart> Parts { get; set; } = new List<NexarPart>();
    }

    public class NexarPart
    {
        public string? Name { get; set; }
        public string? BaseNumber { get; set; }
        public string? ManufacturerName { get; set; }
        public string? ManufacturerPartNumber { get; set; }
        public string? ManufacturerUrl { get; set; }
        public ICollection<Image> Images { get; set; } = new List<Image>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public string? BestImageUrl { get; set; }
        public Document? BestDatasheet { get; set; }
        public ICollection<Spec> Specs { get; set; } = new List<Spec>();
        public PriceValue? MedianPrice1000 { get; set; }
        public string? ShortDescription { get; set; }
        public ICollection<Description> Descriptions { get; set; } = new List<Description>();
        public int TotalAvail { get; set; }
        public Category? Category { get; set; }
        public ICollection<Seller> Sellers { get; set; } = new List<Seller>();
        public string? GenericMpn { get;set;}
        public Manufacturer? Manufacturer { get;set;}
    }

    public class Image
    {
        public string? Url { get;set;}
        public string? CreditUrl { get;set;}
        public string? CreditString { get;set;}
    }

    public class Manufacturer
    {
        public string? Name { get;set;}
        public string? Id { get;set;}
        public ICollection<string> Aliases { get; set; } = new List<string>();
        public string? Url { get;set;}
    }

    public class Seller
    {
        public Company? Company { get; set; }
        public bool IsAuthorized { get; set; }
        public ICollection<Offer> Offers { get; set; } = new List<Offer>();
    }

    public class Offer
    {
        public string? Url { get;set;}
        public int InventoryLevel { get;set;}
        public int Moq { get;set;}
        public string? Packaging { get;set;}
    }

    public class Company
    {
        public string? Name { get; set; }
        public string? Url { get; set; }
        public string? Id { get; set; }
    }

    public class Category
    {
        public string? Id { get; set; }
        public string? ParentId { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
    }

    public class Description
    {
        public string? Text { get; set; }
        public string? CreditUrl { get; set; }
        public string? CreditString { get; set; }
    }

    public class PriceValue
    {
        public double Price { get; set; }
        public string Currency { get; set; } = null!;
        public int Quantity { get; set; }
    }

    public class Spec
    {
        public string? DisplayValue { get; set; }
        public string? Value { get; set; }
        public string? ValueType { get; set; }
        public string? Units { get; set; }
        public string? UnitsName { get; set; }
        public Attribute? Attribute { get; set; }
    }

    public class Attribute
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string ShortName { get; set; } = null!;
    }

    public class Document
    {
        public string? Url { get; set; }
        public int PageCount { get; set; }
        public string? SourceUrl { get; set; }
        public string? Name { get; set; }
    }
}
