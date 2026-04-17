using Binner.Model.Configuration;
using Binner.Model.IO.Printing;

namespace Binner.Model.Responses
{
    public class PrinterSettingsResponse
    {
        /// <summary>
        /// Choose the print mode to use for printing labels.
        /// </summary>
        public PrintModes PrintMode { get; set; } = PrintModes.Direct;

        /// <summary>
        /// The type of printer hardware being used.
        /// </summary>
        public PrintHardwares PrintHardware { get; set; }

        /// <summary>
        /// Full name of printer
        /// </summary>
        public string? PrinterName { get; set; }

        /// <summary>
        /// Label model number
        /// </summary>
        public string? PartLabelName { get; set; }

        /// <summary>
        /// Tape Width in Mm (must match Brother definitions)
        /// </summary>
        public string? TapeWidthMm { get; set; }

        /// <summary>
        /// Label paper source
        /// </summary>
        public LabelSource? PartLabelSource { get; set; } = LabelSource.Auto;

        /// <summary>
        /// If using a remote printer, specify the url.
        /// Requires Binner print spooler
        /// </summary>
        public string? RemoteAddressUrl { get; set; } = string.Empty;

        /// <summary>
        /// Printer part label template (main lines)
        /// </summary>
        public IEnumerable<LineConfiguration>? Lines { get; set; }

        /// <summary>
        /// Printer part label template (side identifiers)
        /// </summary>
        public IEnumerable<LineConfiguration>? Identifiers { get; set; }
    }
}
