using System.ComponentModel.DataAnnotations;

namespace Binner.Model
{
    /// <summary>
    /// A paginated request
    /// </summary>
    public class PaginatedRequest : ISortable, IPaginated
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

        /// <summary>
        /// Property to filter by
        /// </summary>
        public string? By { get; set; }

        /// <summary>
        /// Property value to filter by
        /// </summary>
        public string? Value { get; set; }
    }
}
