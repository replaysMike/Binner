using System.ComponentModel.DataAnnotations;

namespace Binner.Model.Requests
{
    public class PaginatedFilteredRequest : ISortable, IPaginated
    {
        /// <summary>
        /// [Range(1, 1000)]
        /// </summary>
        [Range(1, 1000)]
        public int Page { get; set; } = 1;

        /// <summary>
        /// Number of results to return
        /// </summary>
        [Range(1, 1000)]
        public int Results { get; set; } = 10;

        /// <summary>
        /// Property to order by
        /// </summary>
        public string? OrderBy { get; set; }

        /// <summary>
        /// Direction to sort
        /// </summary>
        public SortDirection Direction { get; set; } = SortDirection.Ascending;

        public string? PartNames { get; set; }
        public string? PartTypes { get; set; }
        public string? Keywords { get; set; }
        public string? Manufacturers { get; set; }
        public string? MountingTypes { get; set; }
        public string? Locations { get; set; }
        public string? BinNumbers { get; set; }
        public string? BinNumbers2 { get; set; }
    }
}
