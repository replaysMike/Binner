namespace Binner.Data.Model
{
    public interface IOptionalUserData
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
