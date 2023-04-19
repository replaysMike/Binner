namespace Binner.Model.Responses
{
    public class PartStoredFilesResponse : PartResponse
    {
        /// <summary>
        /// List of user uploaded files
        /// </summary>
        public ICollection<StoredFile>? StoredFiles { get; set; }
    }
}
