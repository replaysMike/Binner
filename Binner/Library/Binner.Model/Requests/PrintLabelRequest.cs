using Binner.Model.IO.Printing;

namespace Binner.Model.Requests
{
    public class PrintLabelRequest : IImagesToken
    {
        /// <summary>
        /// True to show diagnostic framing
        /// </summary>
        public bool ShowDiagnostic { get; set; }

        /// <summary>
        /// Label model number of label being printed
        /// </summary>
        public string? LabelName { get; set; }

        /// <summary>
        /// Printer paper source to use
        /// </summary>
        public LabelSource LabelSource { get; set; } = LabelSource.Auto;

        /// <summary>
        /// Lines to print
        /// </summary>
        public ICollection<LineConfiguration> Lines { get; set; } = new List<LineConfiguration>();

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
