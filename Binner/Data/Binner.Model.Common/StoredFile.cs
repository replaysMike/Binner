using System.ComponentModel.DataAnnotations;

namespace Binner.Model.Common
{
    /// <summary>
    /// A user uploaded file
    /// </summary>
    public class StoredFile : IEntity
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
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
        public StoredFileType StoredFileType { get; set; } = StoredFileType.Other;

        /// <summary>
        /// The associated part
        /// </summary>
        public long PartId { get; set; }

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
        public DateTime DateCreatedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional user id to associate
        /// </summary>
        public int? UserId { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is StoredFile storedFile)
                return Equals(storedFile);
            return false;
        }

        public bool Equals(StoredFile? other)
        {
            return other != null && StoredFileId == other.StoredFileId && UserId == other.UserId;
        }

        public override int GetHashCode()
        {
#if (NET462 || NET471)
            return ProjectId.GetHashCode() ^ (UserId?.GetHashCode() ?? 0);
#else
            return HashCode.Combine(StoredFileId, UserId);
#endif

        }

        public override string ToString()
        {
            return $"{StoredFileId}: {FileName}";
        }
    }
}
