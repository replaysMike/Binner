namespace Binner.Common.Integrations.Models.Mouser
{
    public class SearchByKeywordRequest
    {
        public string? Keyword { get; set; }

        /// <summary>
        /// Used to specify how many records the method should return.
        /// </summary>
        public int Records { get; set; }

        /// <summary>
        /// Indicates where in the total recordset the return set should begin. From the startingRecord, the number of records specified will be returned up to the end of the recordset. This is useful for paging through the complete recordset of parts matching keyword.
        /// </summary>
        public int StartingRecord { get; set; }

        /// <summary>
        /// Optional. If not provided, the default is None. Refers to options supported by the search engine. Only one value at a time is supported.
        /// </summary>
        public SearchOptions SearchOptions { get; set; }

        /// <summary>
        /// Optional. If not provided, the default is false. Used when searching for keywords in the language specified when you signed up for Search API. Can use string representation: true
        /// </summary>
        public string? SearchWithYourSignUpLanguage { get; set; }
    }

    public enum SearchOptions
    {
        None,
        Rohs,
        InStock,
        RohsAndInStock
    }
}
