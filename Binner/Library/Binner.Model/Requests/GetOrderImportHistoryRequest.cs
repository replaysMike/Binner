namespace Binner.Model.Requests
{
    public class GetOrderImportHistoryRequest
    {
        /// <summary>
        /// Order number
        /// </summary>
        public string OrderNumber { get; set; } = null!;

        /// <summary>
        /// Supplier name
        /// </summary>
        public string Supplier { get; set; } = null!;

        /// <summary>
        /// True to include the order's line items in the results
        /// </summary>
        public bool IncludeLineItems { get; set; }
    }
}
