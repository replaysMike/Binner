namespace Binner.Services.Integrations
{
    public static class ApiConstants
    {
        /// <summary>
        /// Maximum number of records to return in a single API call.
        /// </summary>
        public const int MaxRecords = 50;

        /// <summary>
        /// Maximum number of records to fetch full part information for an order.
        /// </summary>
        public const int OrderFullPartInfoMaxRecords = 25;

        /// <summary>
        /// Maxmimum number of system messages to return from the Binner API.
        /// </summary>
        public const int BinnerSystemMessagesCount = 10;

        /// <summary>
        /// Endpoint for fetching system messages from the Binner API.
        /// </summary>
        public const string BinnerSystemMessageUrl = "https://binner.io/api/messages";
    }
}
