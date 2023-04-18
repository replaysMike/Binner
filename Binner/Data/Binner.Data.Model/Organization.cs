using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    public class Organization : IEntity
    {
        /// <summary>
        /// Organization groups users together and may signify a company or grouping
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrganizationId { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public DateTime DateCreatedUtc { get; set; }

        public DateTime DateModifiedUtc { get; set; }
    }
}
