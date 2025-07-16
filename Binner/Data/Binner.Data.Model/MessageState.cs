using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    public class MessageState : IOptionalUserData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MessageStateId { get; set; }

        /// <summary>
        /// The unique message id
        /// </summary>
        public Guid MessageId { get; set; }

        /// <summary>
        /// True if the message has been read by the user, false otherwise
        /// </summary>
        public DateTime? ReadDateUtc { get; set; }

        /// <summary>
        /// Associated user
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Associated organization
        /// </summary>
        public int? OrganizationId { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }
    }
}
