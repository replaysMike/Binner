using System.Collections.Generic;
using System.Drawing;

namespace Binner.Common.IO.Printing
{
    /// <summary>
    /// Label printer
    /// </summary>
    public interface ILabelPrinter
    {
        /// <summary>
        /// Printer settings
        /// </summary>
        IPrinterSettings PrinterSettings { get; set; }

        /// <summary>
        /// Print a single or multi-line label
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="labelSource">Optionally override label source</param>
        Image PrintLabel(ICollection<string> lines, LabelSource? labelSource = null);
    }
}
