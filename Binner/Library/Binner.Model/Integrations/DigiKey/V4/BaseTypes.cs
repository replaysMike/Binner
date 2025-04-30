namespace Binner.Model.Integrations.DigiKey.V4
{
    public class Parameter
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Priority { get; set; }
    }

    public class Description
    {
        /// <summary>
        /// Description of the product. If you entered a valid Locale and Language in the input, we will return the description in that language. 
        /// Otherwise, we’ll return the English description.
        /// </summary>
        public string? ProductDescription { get; set; }

        /// <summary>
        /// Detailed description of the product. If you entered a valid Locale and Language in the input, we will return the description in that language. 
        /// Otherwise, we’ll return the English description.
        /// </summary>
        public string? DetailedDescription { get; set; }

        public override string ToString()
            => ProductDescription;
    }

    public class Product
    {
        public Description Description { get; set; } = new();
        public Manufacturer? Manufacturer { get; set; }
        public string? ManufacturerProductNumber { get; set; } = null!;
        public double UnitPrice { get; set; }
        public string? ProductUrl { get; set; }
        public string? DatasheetUrl { get; set; }
        public string? PhotoUrl { get; set; }
        public ICollection<ProductVariation> ProductVariations { get; set; } = new List<ProductVariation>();
        public long QuantityAvailable { get; set; }
        public ProductStatusV4 ProductStatus { get; set; } = new();
        public bool BackOrderNotAllowed { get; set; }
        public bool NormallyStocking { get; set; }
        public bool Discontinued { get; set; }
        public bool EndOfLife { get; set; }
        public bool Ncnr { get; set; }
        public string? PrimaryVideoUrl { get; set; }
        public ICollection<ParameterValue> Parameters { get; set; } = new List<ParameterValue>();
        public BaseProduct BaseProductNumber { get; set; } = new();
        public CategoryNode Category { get; set; } = new();
        public DateTime? DateLastBuyChance { get; set; }
        public string? ManufacturerLeadWeeks { get; set; }
        public int ManufacturerPublicQuantity { get; set; }
        public Series Series { get; set; } = new();
        public string? ShippingInfo { get; set; }
        public Classifications Classifications { get; set; } = new();
        /// <summary>
        /// Other Names may include former part numbers (changed through manufacturer acquisition), cross references for parts we don't carry, among other reasons.
        /// </summary>
        public ICollection<string> OtherNames { get; set; } = new List<string>();
        public int ProductsCount { get; set; }
        public ICollection<Product> ExactMatches { get; set; } = new List<Product>();
        public FilterOptions FilterOptions { get; set; } = new();
        public IsoSearchLocale SearchLocaleUsed { get; set; } = new IsoSearchLocale();
        public ICollection<Parameter> AppliedParametricFiltersDto { get; set; } = new List<Parameter>();
        public override string ToString()
            => $"{BaseProductNumber.Name} - {Manufacturer?.Name} - {Description}";
    }

    public class RecommendedProduct
    {
        public string DigiKeyProductNumber { get; set; } = string.Empty;
        public string? ManufacturerProductNumber { get; set; }
        public string? ManufacturerName { get; set; }
        public string? PrimaryPhoto { get; set; }
        public string? ProductDescription { get; set; }
        public long QuantityAvailable { get; set; }
        public double UnitPrice { get; set; }
        public string? ProductUrl { get; set; }
        public ICollection<string> OtherNames { get; set; } = new List<string>();
        public IsoSearchLocale SearchLocaleUsed { get; set; } = new();
    }

    public class ProductSubstitute
    {
        public string SubstituteType { get; set; } = string.Empty;
        public string ProductUrl { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Manufacturer Manufacturer { get; set; } = new();
        public string ManufacturerProductNumber { get; set; } = string.Empty;
        public string UnitPrice { get; set; } = string.Empty;
        public int QuantityAvailable { get; set; }
        public string DigiKeyProductNumber { get; set; } = string.Empty;
    }

    public class ProductDetails
    {
        public IsoSearchLocale SearchLocaleUsed { get; set; } = new();
        public Product Product { get; set; } = new();
    }

    public class Classifications
    {
        public string? ReachStatus { get; set; }
        public string? RohsStatus { get; set; }

        /// <summary>
        /// Code for Moisture Sensitivity Level of the product
        /// </summary>
        public string? MoistureSensitivityLevel { get; set; }

        /// <summary>
        /// Export control class number. See documentation from the U.S. Department of Commerce.
        /// </summary>
        public string? ExportControlClassNumber { get; set; }

        /// <summary>
        /// Harmonized Tariff Schedule of the United States. See documentation from the U.S. International Trade Commission.
        /// </summary>
        public string? HtsusCode { get; set; }
    }

    public class AlternatePackagingV4
    {
        public ICollection<ProductSummary> AlternatePackaging { get; set; } = new List<ProductSummary>();
    }

    public class ProductSummary
    {
        public string ProductUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Manufacturer Manufacturer { get; set; } = new();
        public string ManufacturerProductNumber { get; set; } = string.Empty;
        public string UnitPrice { get; set; } = string.Empty;
        public int QuantityAvailable { get; set; }
        public string DigiKeyProductNumber { get; set; } = string.Empty;
    }

    public class ProductPricing
    {
        public string ManufacturerProductNumber { get; set; } = string.Empty;
        public Manufacturer Manufacturer { get; set; } = new();
        public Description Description { get; set; } = new();
        public int QuantityAvailable { get; set; }
        public string ProductUrl { get; set; } = string.Empty;
        public bool IsDiscontinued { get; set; }
        public bool NormallyStocking { get; set; }
        public bool IsObsolete { get; set; }
        public string? ManufacturerLeadWeeks { get; set; }
        public int ManufacturerPublicQuantity { get; set; }
        public int StandardPackage { get; set; }
        public string? ExportControlClassNumber { get; set; }
        public string? HtsusCode { get; set; }
        public string? MoistureSensitivityLevel { get; set; }
        public bool IsBoNotAllowed { get; set; }
        public bool IsNcnr { get; set; }
        public ICollection<CategoryType> Categories { get; set; } = new List<CategoryType>();
        public bool ContainsLithium { get; set; }
        public bool ContainsMercury { get; set; }
        public bool IsEndOfLife { get; set; }
        public ICollection<string> OtherNames { get; set; } = new List<string>();
        public ICollection<ProductPricingVariation> ProductVariations { get; set; } = new List<ProductPricingVariation>();
    }

    public class ProductPricingVariation
    {
        public string DigiKeyProductNumber { get; set; } = string.Empty;
        public int QuantityAvailableforPackageType { get; set; }
        public int MinimumOrderQuantity { get; set; }
        public int MaxQuantityForDistribution { get; set; }
        public PackageType PackageType { get; set; } = new();
        public bool MarketPlace { get; set; }
        public PriceBreak StandardPricing { get; set; } = new();
        public PriceBreak MyPricing { get; set; } = new();
        public bool IsTariffActive { get; set; }
        public double DigiReelingFee { get; set; }
    }

    public class CategoryType
    {
        public int CategoryId { get; set; }
        public int Level { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class ParameterValue
    {
        public int ParameterId { get; set; }
        public string ParameterText { get; set; } = string.Empty;
        public ParameterTypes ParameterType { get; set; }
        public string ValueId { get; set; } = string.Empty;
        public string ValueText { get; set; } = string.Empty;
        public override string ToString()
            => $"{ParameterText} [{ParameterId}] {ValueText} [{ValueId}]";
    }

    public class IdName
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public override string ToString()
            => $"{Name} [{Id}]";
    }

    public class CategoryNode
    {
        public int CategoryId { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public long? ProductCount { get; set; }
        public long? NewProductCount { get; set; }
        public string? ImageUrl { get; set; }
        public string? SeoDescription { get; set; }
        public ICollection<CategoryNode> ChildCategories { get; set; } = new List<CategoryNode>();
        public override string ToString()
            => $"{Name} [{CategoryId}]";
    }

    public class BaseProduct : IdName
    {
    }

    public class ProductStatusV4
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ProductVariation
    {
        public string DigiKeyProductNumber { get; set; } = string.Empty;
        public PackageType PackageType { get; set; } = new();
        public ICollection<PriceBreak> StandardPricing { get; set; } = new List<PriceBreak>();
        public ICollection<PriceBreak> MyPricing { get; set; } = new List<PriceBreak>();
        public bool MarketPlace { get; set; }
        public bool TariffActive { get; set; }
        public Supplier Supplier { get; set; } = new();
        public int QuantityAvailableforPackageType { get; set; }
        public long MaxQuantityForDistribution { get; set; }
        public int MinimumOrderQuantity { get; set; }
        public int StandardPackage { get; set; }
        public double DigiReelFee { get; set; }
    }

    public class Supplier : IdName
    {
    }

    public class PriceBreak
    {
        public int BreakQuantity { get; set; }
        public double UnitPrice { get; set; }
        public double TotalPrice { get; set; }
    }

    public class PackageType : IdName
    {
        public string Description { get; set; } = string.Empty;


    }

    public class Manufacturer : IdName
    {
    }

    public class ManufacturerInfo : IdName
    {
    }

    public class Series : IdName
    {
    }


    public class MediaLinks
    {
        public string MediaType { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string SmallPhoto { get; set; } = string.Empty;
        public string Thumbnail { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class LimitedTaxonomy
    {
        public ICollection<LimitedTaxonomy>? Children { get; set; }
        public int? ProductCount { get; set; }
        public int? NewProductCount { get; set; }
        public int ParameterId { get; set; }
        public string? ValueId { get; set; }
        public string? Parameter { get; set; }
        public string? Value { get; set; }
    }

    public class BaseFilterV4
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
        public long? ProductCount { get; set; }
    }

    public class FilterOptions
    {
        public ICollection<BaseFilterV4>? Manufacturers { get; set; } = new List<BaseFilterV4>();
        public ICollection<BaseFilterV4>? Packaging { get; set; } = new List<BaseFilterV4>();
        public ICollection<BaseFilterV4>? Status { get; set; } = new List<BaseFilterV4>();
        public ICollection<BaseFilterV4>? Series { get; set; } = new List<BaseFilterV4>();
        public ICollection<ParametricFilterOptionsResponse>? ParametricFilters { get; set; } = new List<ParametricFilterOptionsResponse>();
        public ICollection<TopCategory>? TopCategories { get; set; } = new List<TopCategory>();
        public ICollection<MarketPlaceFilters> MarketPlaceFilters { get; set; } = new List<MarketPlaceFilters>();
    }

    public class Recommendation
    {
        public string ProductNumber { get; set; } = string.Empty;
        public ICollection<RecommendedProduct> RecommendedProducts { get; set; } = new List<RecommendedProduct>();
        public IsoSearchLocale SearchedLocaleUsed { get; set; } = new();
    }

    public class ParametricCategory
    {
        public int ParameterId { get; set; }
        public ICollection<FilterId> FilterValues { get; set; } = new List<FilterId>();
    }

    public class FilterId
    {
        public string Id { get; set; } = string.Empty;
        public FilterId() { }
        public FilterId(string id)
        {
            Id = id;
        }
    }

    public class SettingsUsed
    {
        public int CustomerId { get; set; }
        public IsoSearchLocale SearchLocale { get; set; } = new();
        public Filters Filters { get; set; } = new();
    }

    public class TopCategory
    {
        public TopCategoryNode RootCategory { get; set; } = new();
        public TopCategoryNode Category { get; set; } = new();
        public double Score { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class TopCategoryNode : IdName
    {
        public long? ProductCount { get; set; }
    }

    public class ParametricFilterOptionsResponse
    {
        public BaseFilterV4 Category { get; set; } = new();
        public string ParameterType { get; set; } = string.Empty;
        public int ParameterId { get; set; }
        public string ParameterName { get; set; } = string.Empty;
        public ICollection<FilterValue> FilterValues { get; set; } = new List<FilterValue>();
    }

    public class FilterValue
    {
        public long? ProductCount { get; set; }
        public string ValueId { get; set; } = string.Empty;
        public string ValueName { get; set; } = string.Empty;
        public RangeFilterTypes? RangeFilterType { get; set; }
        public override string ToString()
            => $"{ValueName} [{ValueId}] - {ProductCount}";
    }

    public class IsoSearchLocale
    {
        public string Site { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
    }

    public class Filters
    {
        public bool IsInStock { get; set; }
        public bool ExcludeMarketplace { get; set; }
    }

    public class SortOptions
    {
        public SortFields Field { get; set; } = SortFields.None;
        public SortDirection SortOrder { get; set; } = SortDirection.Ascending;
    }

    public enum MarketPlaceFilter
    {
        NoFilter,
        ExcludeMarketPlace,
        MarketPlaceOnly
    }

    public enum SortFields
    {
        None,
        Packaging,
        ProductStatus,
        DigiKeyProductNumber,
        ManufacturerProductNumber,
        Manufacturer,
        MinimumQuantity,
        QuantityAvailable,
        Price,
        Supplier,
        PriceManufacturerStandardPackage
    }

    public enum SearchOptions
    {
        ChipOutpost,
        Has3DModel,
        HasCadModel,
        HasDatasheet,
        HasProductPhoto,
        InStock,
        NewProduct,
        NonRohsCompliant,
        NormallyStocking,
        RohsCompliant
    }

    public enum ParameterTypes
    {
        String,
        Integer,
        Double,
        UnitOfMeasure,
        CoupledUnitOfMeasure,
        RangeUnitOfMeasure
    }

    public enum RangeFilterTypes
    {
        Min,
        Max,
        Range
    }

    public enum MarketPlaceFilters
    {
        NoFilter,
        ExcludeMarketPlace,
        MarketPlaceOnly
    }
}
