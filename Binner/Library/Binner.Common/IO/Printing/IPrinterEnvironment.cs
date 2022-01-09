using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Binner.Common.IO.Printing
{
    /// <summary>
    /// Printer hardware abstraction
    /// </summary>
    public interface IPrinterEnvironment
    {
        /// <summary>
        /// Print a label image
        /// </summary>
        /// <param name="options">User print options</param>
        /// <param name="labelProperties">The label media properties</param>
        /// <param name="labelImage">Label image to print in Rgba32 format</param>
        /// <returns></returns>
        PrinterResult PrintLabel(PrinterOptions options, LabelDefinition labelProperties, Image<Rgba32> labelImage);
    }
}
