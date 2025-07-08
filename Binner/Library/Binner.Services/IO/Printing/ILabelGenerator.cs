using Binner.Model;
using Binner.Model.Requests;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Binner.Services.IO.Printing;

public interface ILabelGenerator
{
    /// <summary>
    /// Create a label image
    /// </summary>
    /// <param name="label"></param>
    /// <param name="part"></param>
    /// <returns></returns>
    Image<Rgba32> CreateLabelImage(Label label, Part part);

    /// <summary>
    /// Create a label image
    /// </summary>
    /// <param name="request"></param>
    /// <param name="part"></param>
    /// <returns></returns>
    Image<Rgba32> CreateLabelImage(CustomLabelDefinition request, Part part);
}