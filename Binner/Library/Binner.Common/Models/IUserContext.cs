namespace Binner.Common.Models
{
    public interface IUserContext
    {
        /// <summary>
        /// Email address
        /// </summary>
        string EmailAddress { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        string PhoneNumber { get; set; }

        /// <summary>
        /// User Id
        /// </summary>
        int UserId { get; set; }
    }
}