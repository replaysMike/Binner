using Binner.Global.Common;

namespace Binner.Model
{
    public class Organization : IOrganization
    {
        public int OrganizationId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? DateLockedUtc { get; set; }

        public Guid GlobalId { get; set; }
    }
}
