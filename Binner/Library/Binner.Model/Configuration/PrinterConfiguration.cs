using Binner.Model.IO.Printing;

namespace Binner.Model.Configuration
{
    public class PrinterConfiguration
    {
        /// <summary>
        /// Choose the print mode to use for printing labels.
        /// </summary>
        public PrintModes PrintMode { get; set; } = PrintModes.Direct;

        /// <summary>
        /// Full name of printer
        /// Default: Dymo LabelWriter 450
        /// </summary>
        public string PrinterName { get; set; } = "Dymo LabelWriter 450 Twin Turbo";

        /// <summary>
        /// Label model number
        /// Default: 30346
        /// </summary>
        public string PartLabelName { get; set; } = "30346"; // LW 1/2" x 1 7/8"

        /// <summary>
        /// Label paper source
        /// </summary>
        public LabelSource PartLabelSource { get; set; } = LabelSource.Auto;

        /// <summary>
        /// If using a remote printer, specify the url.
        /// Requires Binner print spooler
        /// </summary>
        public string RemoteAddressUrl { get; set; } = string.Empty;

        /// <summary>
        /// Template for printing part labels
        /// </summary>
        public PartLabelTemplate PartLabelTemplate { get; set; } = new PartLabelTemplate();

        /// <summary>
        /// List of label definitions
        /// </summary>
        public List<LabelDefinition> LabelDefinitions { get; set; } = new List<LabelDefinition>();
    }
}
