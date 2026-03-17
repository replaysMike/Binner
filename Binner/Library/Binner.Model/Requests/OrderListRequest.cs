namespace Binner.Model.Requests
{
    public class OrderListRequest
    {
        /// <summary>
        /// Start date to list
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date to list
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Page number to list
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of items to return
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Name of supplier
        /// </summary>
        public string? Supplier { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string? Password { get; set; }
    }
}
