using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Binner.Model;

namespace Binner.Data.Model
{
    /// <summary>
    /// A user uploaded file
    /// </summary>
    public class StoredFile
#if INITIALCREATE
        : IEntity,
#else
        : IPartialEntity,
#endif
            IOptionalUserData, IGlobalData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long StoredFileId { get; set; }

        public Guid GlobalId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Filename of the uploaded file
        /// </summary>
        public string FileName { get; set; } = null!;

        /// <summary>
        /// Original filename of the uploaded file
        /// </summary>
        public string OriginalFileName { get; set; } = null!;

        /// <summary>
        /// The type of file stored
        /// </summary>
        public StoredFileType StoredFileType { get; set; } = StoredFileType.Other;

        /// <summary>
        /// The associated part
        /// </summary>
        public long? PartId { get; set; }

        /// <summary>
        /// The associated record (that isn't a part)
        /// </summary>
        public long? RecordId { get; set; }

        /// <summary>
        /// The type of record associated with the stored file
        /// </summary>
        public RecordType RecordType { get; set; }

        /// <summary>
        /// The file length in bytes
        /// </summary>
        public int FileLength { get; set; }

        /// <summary>
        /// The crc 32 checksum of the file
        /// </summary>
        public int Crc32 { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime DateCreatedUtc { get; set; }

        /// <summary>
        /// Optional user id to associate
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Associated organization
        /// </summary>
        public int? OrganizationId { get; set; }

#if INITIALCREATE
        public DateTime DateModifiedUtc { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
#endif
        public Part? Part { get; set; }

        public ICollection<PcbStoredFileAssignment>? PcbStoredFileAssignments { get; set; }

    }
}
