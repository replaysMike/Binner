namespace Binner.Model.Requests
{
    public class PrintPartRequest : IImagesToken
    {
        /// <summary>
        /// The main part number
        /// </summary>
        public string? PartNumber { get; set; }

        /// <summary>
        /// Optional part id
        /// </summary>
        public long PartId { get; set; }

        /// <summary>
        /// True to generate image only
        /// </summary>
        public bool GenerateImageOnly { get; set; }

        /// <summary>
        /// The image token to validate the insecure request with
        /// </summary>
        public string? Token { get; set; }
    }
}
