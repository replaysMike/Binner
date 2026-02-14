using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    public class Organization : IEntity, IGlobalData
    {
        /// <summary>
        /// Organization groups users together and may signify a company or grouping
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrganizationId { get; set; }

        public Guid GlobalId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Organization name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Organization description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Set if the organization is locked and users cannot login
        /// </summary>
        public DateTime? DateLockedUtc { get; set; }

        public DateTime DateCreatedUtc { get; set; }

        public DateTime DateModifiedUtc { get; set; }

        public ICollection<User>? Users { get; set; }

        public ICollection<OrganizationConfiguration>? OrganizationConfigurations { get; set; }

        public ICollection<OrganizationIntegrationConfiguration>? OrganizationIntegrationConfigurations { get; set; }
    }
}
