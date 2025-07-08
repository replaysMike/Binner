using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Binner.Model.IO.Printing.PrinterHardware
{
    /// <summary>
    /// Label printer
    /// </summary>
    public interface ILabelPrinterHardware
    {
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

        /// <summary>
        /// Print a label image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="options"></param>
        void PrintLabelImage(Image<Rgba32> image, PrinterOptions options);
    }
}
