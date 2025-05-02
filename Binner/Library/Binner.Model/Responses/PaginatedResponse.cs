namespace Binner.Model.Responses
{
    /// <summary>
    /// Paginated data response
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PaginatedResponse<T>
    {
        /// <summary>
        /// The total number of items available
        /// </summary>
        public int TotalItems { get; }

        /// <summary>
        /// The page size that was requested
        /// </summary>
        public int PageSize { get; }

        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);

        /// <summary>
        /// The page number being returned
        /// </summary>
        public int PageNumber { get; }

        /// <summary>
        /// The page of items requested
        /// </summary>
        public ICollection<T> Items { get; }

        public PaginatedResponse(int totalItems, ICollection<T> items)
        {
            TotalItems = totalItems;
            Items = items;
            PageSize = -1;
            PageNumber = -1;
        }

        public PaginatedResponse(int totalItems, int pageSize, int pageNumber, ICollection<T> items)
        {
            TotalItems = totalItems;
            PageSize = pageSize;
            PageNumber = pageNumber;
            Items = items;
        }
    }
}
