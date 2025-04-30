namespace Binner.Model.Integrations.DigiKey.V4
{
    /// <summary>
    ///     Very simple version of Keyword Search request for WebApp and IntegrationExams
    /// </summary>
    public class KeywordSearchRequest
    {
        /// <summary>
        /// The keywords to search for.
        /// </summary>
        public string? Keywords { get; set; }

        /// <summary>
        /// The number of records to retrieve.
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// The record start position.
        /// </summary>
        public int Offset { get; set; }

        public FilterOptionsRequest FilterOptionsRequest { get; set; } = new();
        public SortOptions SortOptions { get; set; } = new();
    }

    public class FilterOptionsRequest
    {
        public ICollection<FilterId> ManufacturerFilter { get; set; } = new List<FilterId>();
        public ICollection<FilterId> CategoryFilter { get; set; } = new List<FilterId>();
        public ICollection<FilterId> StatusFilter { get; set; } = new List<FilterId>();
        public ICollection<FilterId> PackagingFilter { get; set; } = new List<FilterId>();
        public MarketPlaceFilter MarketPlaceFilter { get; set; } = MarketPlaceFilter.NoFilter;
        public ICollection<FilterId> SeriesFilter { get; set; } = new List<FilterId>();
        public int MinimumQuantityAvailable { get; set; } = 0;
        public ParameterFilterRequest ParameterFilterRequest { get; set; } = new();
        public ICollection<SearchOptions> SearchOptions { get; set; } = new List<SearchOptions>();
    }

    public class ParameterFilterRequest
    {
        public FilterId CategoryFilter { get; set; } = new();
        public List<ParametricCategory> ParameterFilters { get; set; } = new();
    }
}
