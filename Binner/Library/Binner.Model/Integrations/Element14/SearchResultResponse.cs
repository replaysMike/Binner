using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Binner.Model.Integrations.Element14
{
    public class Element14SearchResult
    {
        [JsonProperty("keywordSearchReturn")]
        public KeywordSearchReturn? KeywordSearchReturn { get; set; }
    }

    public class KeywordSearchReturn
    {
        [JsonProperty("numberOfResults")]
        public int? NumberOfResults { get; set; }

        [JsonProperty("products")]
        public List<Product>? Products { get; set; }
    }

    public class Product
    {
        public string? sku { get; set; }
        public string? displayName { get; set; }
        public string? productURL { get; set; }
        public string? productStatus { get; set; }
        public string? rohsStatusCode { get; set; }
        public int? packSize { get; set; }
        public string? unitOfMeasure { get; set; }
        public string? unitOfMeasureCode { get; set; }
        public string? id { get; set; }
        public Image? image { get; set; }
        public List<Datasheet>? datasheets { get; set; }
        public ProductOverview? productOverview { get; set; }
        public List<Price>? prices { get; set; }
        public int? inv { get; set; }
        public string? vendorId { get; set; }
        public string? vendorName { get; set; }
        public string? brandLogoURL { get; set; }
        public string? brandName { get; set; }
        public string? translatedManufacturerPartNumber { get; set; }
        public int? translatedMinimumOrderQuality { get; set; }
        public List<Attribute>? attributes { get; set; }
        public Related? related { get; set; }
        public Stock? stock { get; set; }
        public string? countryOfOrigin { get; set; }
        public bool? comingSoon { get; set; }
        public int? inventoryCode { get; set; }
        public string? nationalClassCode { get; set; }
        public string? publishingModule { get; set; }
        public string? vatHandlingCode { get; set; }
        public int? releaseStatusCode { get; set; }
        public bool? isSpecialOrder { get; set; }
        public bool? isAwaitingRelease { get; set; }
        public bool? reeling { get; set; }
        public int? discountReason { get; set; }
        public string? brandId { get; set; }
        public string? commodityClassCode { get; set; }
        public string? orderMultiples { get; set; }
        public string? packageName { get; set; }

        // not in the result but needed for converting to the common part
        public string? currency { get; set; }
    }

    public class Image
    {
        public string? baseName { get; set; }
        public string? vrntPath { get; set; }
        public string? mainImageURL { get; set; }
        public string? thumbNailImageURL { get; set; }
    }

    public class ProductOverview
    {
        public string? description { get; set; }
        public string? bullets { get; set; }
        public string? applications { get; set; }
        public string? warnings { get; set; }
        public string? footnotes { get; set; }
    }

    public class Price
    {
        public int? to { get; set; }
        public int? from { get; set; }
        public decimal? cost { get; set; }
    }

    public class Attribute
    {
        public string? attributeLabel { get; set; }
        public string? attributeUnit { get; set; }
        public string? attributeValue { get; set; }
    }

    public class Related
    {
        public bool? containAlternatives { get; set; }
        public bool? containcontainRoHSAlternatives { get; set; }
        public bool? containAccessories { get; set; }
        public bool? containcontainRoHSAccessories { get; set; }
    }

    public class Stock
    {
        public int? level { get; set; }
        public int? leastLeadTime { get; set; }
        public int? status { get; set; }
        public bool? shipsFromMultipleWarehouses { get; set; }
        public List<Breakdown>? breakdown { get; set; }
        public List<RegionalBreakdown>? regionalBreakdown { get; set; }
    }

    public class Breakdown
    {
        public int? inv { get; set; }
        public string? region { get; set; }
        public int? lead { get; set; }
        public string? warehouse { get; set; }
    }

    public class RegionalBreakdown
    {
        public int? level { get; set; }
        public int? leastLeadTime { get; set; }
        public int? status { get; set; }
        public string? warehouse { get; set; }
        public bool? shipsFromMultipleWarehouses { get; set; }
    }

    public class Datasheet
    {
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("url")]
        public string? Url { get; set; }
    }
}
