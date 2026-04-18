using Binner.Model.Configuration;

namespace Binner.Model.IO.Printing
{
    /// <summary>
    /// Printer settings
    /// </summary>
    public interface IPrinterSettings
    {
        /// <summary>
        /// Choose the print mode to use for printing labels.
        /// </summary>
        PrintModes PrintMode { get; set; }

        /// <summary>
        /// The type of printer hardware being used.
        /// </summary>
        PrintHardwares PrintHardware { get; set; }

        /// <summary>
        /// Full name of printer
        /// </summary>
        string PrinterName { get; set; }

        /// <summary>
        /// Label model number for printing parts
        /// </summary>
        string PartLabelName { get; set; } // "30346";

        /// <summary>
        /// Label paper source for printing parts
        /// </summary>
        LabelSource PartLabelSource { get; set; }

        /// <summary>
        /// List of label definitions
        /// </summary>
        IEnumerable<LabelDefinition> LabelDefinitions { get; set; }

        /// <summary>
        /// Template for printing part labels
        /// </summary>
        PartLabelTemplate PartLabelTemplate { get; set; }
    }
}
