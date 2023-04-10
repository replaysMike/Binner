namespace Binner.Model.IO.Printing
{
    /// <summary>
    /// Printer settings
    /// </summary>
    public interface IPrinterSettings
    {
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
