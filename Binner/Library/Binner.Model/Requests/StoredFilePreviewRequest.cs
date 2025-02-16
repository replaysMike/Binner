namespace Binner.Model.Requests
{
    public class StoredFilePreviewRequest
    {
        /// <summary>
        /// The stored file to load
        /// </summary>
        public string? Filename { get; set; }

        /// <summary>
        /// The image token to validate the insecure request with
        /// </summary>
        public string? Token { get; set; }
    }
}
