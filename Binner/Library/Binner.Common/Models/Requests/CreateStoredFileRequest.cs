using Binner.Model.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Binner.Common.Models
{
    public class CreateStoredFileRequest
    {
        /// <summary>
        /// Filename of the file
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Original uploaded filename
        /// </summary>
        public string? OriginalFileName { get; set; }

        /// <summary>
        /// Stored file type
        /// </summary>
        public StoredFileType StoredFileType { get; set; }

        /// <summary>
        /// Part id associated with the file
        /// </summary>
        public long PartId { get; set; }

        /// <summary>
        /// File length
        /// </summary>
        public int FileLength { get; set; }

        /// <summary>
        /// Checksum of the file
        /// </summary>
        public int Crc32 { get; set; }
    }
}
