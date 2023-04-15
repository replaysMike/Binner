namespace Binner.Common.Models
{
    /// <summary>
    /// An images token is required for serving backend generated images
    /// </summary>
    public interface IImagesToken
    {
        /// <summary>
        /// The image token to validate the insecure request with
        /// </summary>
        string? Token { get; set; }
    }
}
