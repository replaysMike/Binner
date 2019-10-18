namespace Binner.Common.Integrations.Models.Mouser
{
    public class SearchByPartRequest
    {
        /// <summary>
        /// Used for searching parts by part number. Enter up to 10 part numbers using pipe delimited for your search, e.g.: 2n2222a|2n2222b|2n2222c
        /// </summary>
        public string MouserPartNumber { get; set; }

        /// <summary>
        /// Optional. If not provided, the default is None. Refers to options supported by the search engine. Only one value at a time is supported.
        /// </summary>
        public PartSearchOptions PartSearchOptions { get; set; }
    }

    public enum PartSearchOptions
    {
        None,
        Exact,
        BeginsWith
    }
}
