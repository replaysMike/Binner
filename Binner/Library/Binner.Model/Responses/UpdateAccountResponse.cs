namespace Binner.Model.Responses
{
    public class UpdateAccountResponse
    {
        /// <summary>
        /// The user's account
        /// </summary>
        public Account Account { get; set; } = null!;

        /// <summary>
        /// True if operation was successful
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string? Message { get; set; }
    }
}
