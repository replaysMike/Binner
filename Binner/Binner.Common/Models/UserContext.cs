namespace Binner.Common.Models
{
    /// <summary>
    /// A user context
    /// </summary>
    public class UserContext
    {
        /// <summary>
        /// User Id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Name of user
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Email address of user
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Phone number of user
        /// </summary>
        public string PhoneNumber { get; set; }
    }
}
