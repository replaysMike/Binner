namespace Binner.Common.IO.Printing
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
        /// Label model number
        /// </summary>
        string LabelName { get; set; }

        /// <summary>
        /// Label paper source
        /// </summary>
        LabelSource LabelSource { get; set; }

        /// <summary>
        /// Template for printing part labels
        /// </summary>
        PartLabelTemplate PartLabelTemplate { get; set; }
    }
}
