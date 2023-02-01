using Binner.Model.Common;
using System.IO;

namespace Binner.Common.IO
{
    public class UserUploadedFile
    {
        public string Filename { get; set; }

        public Stream Stream { get; set; }

        /// <summary>
        /// The associated part id
        /// </summary>
        public long PartId { get; set; }

        /// <summary>
        /// The stored file type
        /// </summary>
        public StoredFileType StoredFileType { get; set; } = StoredFileType.Other;

        public UserUploadedFile(string filename, Stream stream, long partId, StoredFileType storedFileType)
        {
            Filename = filename;
            Stream = stream;
            PartId = partId;
            StoredFileType = storedFileType;
        }
    }
}
