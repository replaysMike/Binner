namespace Binner.Model.Requests
{
    public class UploadUserFilesRequest<T>
    {
        /// <summary>
        /// Part id associated with the file
        /// </summary>
        public long PartId { get; set; }

        /// <summary>
        /// Stored file type
        /// </summary>
        public StoredFileType StoredFileType { get; set; } = StoredFileType.Other;

        /// <summary>
        /// List of uploaded files
        /// </summary>
        public ICollection<T>? Files { get; set; }
    }
}
