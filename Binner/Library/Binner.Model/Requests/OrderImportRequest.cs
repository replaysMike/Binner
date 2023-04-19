namespace Binner.Model.Requests
{
    public class OrderImportRequest
    {
        /// <summary>
        /// Supplier specific Order Id
        /// </summary>
        public string? OrderId { get; set; }

        /// <summary>
        /// Name of supplier
        /// </summary>
        public string? Supplier { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string? Username { get;set;}

        /// <summary>
        /// Password
        /// </summary>
        public string? Password { get;set;}
    }
}
