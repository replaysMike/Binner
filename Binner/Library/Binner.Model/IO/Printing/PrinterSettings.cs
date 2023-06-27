using Binner.Model.Configuration;

namespace Binner.Model.IO.Printing
{
    public class PrinterSettings : IPrinterSettings
    {
        /// <summary>
        /// Choose the print mode to use for printing labels.
        /// </summary>
        public PrintModes PrintMode { get; set; } = PrintModes.Direct;

        public string PrinterName { get; set; } = "Dymo LabelWriter 450 Twin Turbo";
        
        public string PartLabelName { get; set; } = "30346"; // LW 1/2" x 1 7/8"
        
        public LabelSource PartLabelSource { get; set; } = LabelSource.Auto;

        /// <summary>
        /// List of label definitions
        /// </summary>
        public IEnumerable<LabelDefinition> LabelDefinitions { get; set; } = new List<LabelDefinition>();

        /// <summary>
        /// Template for printing part labels
        /// </summary>
        public PartLabelTemplate PartLabelTemplate { get; set; } = new PartLabelTemplate();
    }
}
