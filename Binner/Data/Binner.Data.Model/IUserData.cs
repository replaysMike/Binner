namespace Binner.Data.Model
{
    public interface IUserData
    {
        /// <summary>
        /// Associated User Id
        /// </summary>
        int? UserId { get; set; }

        /// <summary>
        /// Associated Organization Id
        /// </summary>
        int? OrganizationId { get; set; }
    }
}
