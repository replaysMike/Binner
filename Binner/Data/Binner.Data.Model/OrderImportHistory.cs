using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Binner.Data.Model
{
    public class OrderImportHistory : IEntity, IUserData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long OrderImportHistoryId { get; set; }

        public string Supplier { get; set; } = null!;

        public string SalesOrder { get; set; } = null!;

        public string? Invoice { get; set; }

        public string? Packlist { get; set; }

        /// <summary>
        /// Associated user
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Associated organization
        /// </summary>
        public int? OrganizationId { get; set; }

        /// <summary>
        /// The date the record was created
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        /// <summary>
        /// The date the record was modified
        /// </summary>
        public DateTime DateModifiedUtc { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        public ICollection<OrderImportHistoryLineItem>? OrderImportHistoryLineItems { get; set; }
    }
}
