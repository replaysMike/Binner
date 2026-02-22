using Binner.Model;
using System.IO;

namespace Binner.Common.IO
{
    public class UserUploadedFile : UploadedFile
    {
        /// <summary>
        /// The associated part id
        /// </summary>
        public long PartId { get; set; }

        /// <summary>
        /// The stored file type
        /// </summary>
        public StoredFileType StoredFileType { get; set; } = StoredFileType.Other;

        public UserUploadedFile(string filename, Stream stream, long partId, StoredFileType storedFileType)
            : base(filename, stream)
        {
            PartId = partId;
            StoredFileType = storedFileType;
        }
    }
}
