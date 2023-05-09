using Binner.Model;
using Binner.Model.IO.Printing;
using Binner.Model.Requests;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Binner.Common.IO.Printing;

public interface ILabelGenerator
{
    /// <summary>
    /// Printer settings
    /// </summary>
    IPrinterSettings PrinterSettings { get; set; }

    /// <summary>
    /// Create a label image
    /// </summary>
    /// <param name="request"></param>
    /// <param name="part"></param>
    /// <returns></returns>
    Image<Rgba32> CreateLabelImage(CustomLabelRequest request, Part part);
}