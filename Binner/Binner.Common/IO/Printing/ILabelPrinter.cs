using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

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
        /// Print a label
        /// </summary>
        /// <param name="content"></param>
        /// <param name="options">Printer options overrides</param>
        /// <returns></returns>
        Image<Rgba32> PrintLabel(LabelContent content, PrinterOptions options);

        /// <summary>
        /// Print a custom label
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        Image<Rgba32> PrintLabel(ICollection<LineConfiguration> lines, PrinterOptions options);
    }
}
