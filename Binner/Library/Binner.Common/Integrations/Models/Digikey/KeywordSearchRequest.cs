using System.Collections.Generic;

namespace Binner.Common.Integrations.Models.Digikey
{
    public class KeywordSearchRequest
    {
        public string Keywords { get; set; }
        public int RecordCount { get; set; } = 25;
        public int RecordStartPosition { get; set; } = 0;
        public Filters Filters { get; set; } = new Filters();
        public SortParameters Sort { get; set; } = new SortParameters();
        public int RequestedQuantity { get; set; }
        public ICollection<SearchOptions> SearchOptions { get; set; } = new List<SearchOptions> { };
    }

    public enum SearchOptions
    {
        LeadFree,
        CollapsePackagingTypes,
        ExcludeNonStock,
        Has3DModel,
        InStock,
        ManufacturerPartSearch,
        NewProductsOnly,
        RoHSCompliant,
        HasMentorFootprint
    }

    public class Filters
    {
        public ICollection<int> TaxonomyIds { get; set; }
        public ICollection<int> ManufacturerIds { get; set; }
        public ICollection<ParametricFilter> ParametricFilters { get; set; } = new List<ParametricFilter>();
    }

    public class ParametricFilter 
    {
        public int ParameterId { get; set; }    
        public string ValueId { get; set; }
    }

    public class SortParameters
    {
        public SortOptions SortOption { get; set; } = SortOptions.SortByQuantityAvailable;
        public SortDirection Direction { get; set; } = SortDirection.Descending;
        public int SortParameterId { get; set; }
    }
    
    public enum SortOptions
    {
        SortByDigiKeyPartNumber, 
        SortByManufacturerPartNumber, 
        SortByDescription, 
        SortByManufacturer, 
        SortByMinimumOrderQuantity, 
        SortByQuantityAvailable, 
        SortByUnitPrice, 
        SortByParameter
    }

    public enum SortDirection
    {
        Ascending,
        Descending
    }

}
