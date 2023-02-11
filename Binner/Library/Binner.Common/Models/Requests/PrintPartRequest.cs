namespace Binner.Common.Models
{
    public class PrintPartRequest
    {
        /// <summary>
        /// The main part number
        /// </summary>
        public string? PartNumber { get; set; }
        
        /// <summary>
        /// True to generate image only
        /// </summary>
        public bool GenerateImageOnly { get; set; }
    }
}
