namespace Binner.Global.Common
{
    public interface IOrganization
    {
        /// <summary>
        /// Organization Id
        /// </summary>
        int OrganizationId { get; set; }

        /// <summary>
        /// Organization name
        /// </summary>
        string? Name { get; set; }

        /// <summary>
        /// Description of organization
        /// </summary>
        string? Description { get; set; }

        /// <summary>
        /// Set if the organization is locked and users cannot login
        /// </summary>
        public DateTime? DateLockedUtc { get; set; }

        public Guid GlobalId { get; set; }
    }
}
