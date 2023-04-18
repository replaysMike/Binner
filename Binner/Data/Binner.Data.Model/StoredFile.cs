﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
            IUserData
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long StoredFileId { get; set; }

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
        public Binner.Model.Common.StoredFileType StoredFileType { get; set; } = Binner.Model.Common.StoredFileType.Other;

        /// <summary>
        /// The associated part
        /// </summary>
        public long? PartId { get; set; }

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
        public Part? Part { get;set; }

        public ICollection<PcbStoredFileAssignment>? PcbStoredFileAssignments { get; set; }

    }
}
