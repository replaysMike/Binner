namespace Binner.Model.Requests
{
    public class GetPartImageRequest : IImagesToken
    {
        /// <summary>
        /// The main part number
        /// </summary>
        public string PartNumber { get; set; } = null!;

        /// <summary>
        /// The image token to validate the insecure request with
        /// </summary>
        public string? Token { get; set; }
    }
}
