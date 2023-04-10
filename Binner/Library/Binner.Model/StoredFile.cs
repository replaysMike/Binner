using System.ComponentModel.DataAnnotations;

namespace Binner.Model
{
    public class StoredFile
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

        public DateTime DateModifiedUtc { get; set; }
    }
}
