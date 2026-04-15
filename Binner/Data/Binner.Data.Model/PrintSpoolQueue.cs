using Binner.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Binner.Data.Model
{
    /// <summary>
    /// Stores print queue data
    /// </summary>
    public class PrintSpoolQueue : IGlobalData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PrintSpoolerId { get; set; }

        /// <summary>
        /// Global reference id
        /// </summary>
        public Guid GlobalId { get; set; }

        /// <summary>
        /// The type of print job
        /// </summary>
        public PrintTypes PrintType { get; set; }

        /// <summary>
        /// The item being printed
        /// </summary>
        public string Json { get; set; } = string.Empty;

        /// <summary>
        /// The label object
        /// </summary>
        public string LabelJson { get; set; } = string.Empty;

        /// <summary>
        /// The template object
        /// </summary>
        public string TemplateJson { get; set; } = string.Empty;

        /// <summary>
        /// Rendered print image
        /// </summary>
        public byte[]? Image { get; set; }

        /// <summary>
        /// Checksum of json contents
        /// </summary>
        public int Crc32 { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        /// <summary>
        /// Associated organization
        /// </summary>
        public int OrganizationId { get; set; }

        /// <summary>
        /// User who created the record
        /// </summary>
        public int? UserId { get; set; }
    }
}
