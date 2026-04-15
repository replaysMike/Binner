using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.IO.Printing;
using Binner.Model.Requests;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binner.Services
{
    public interface IPrintService
    {
        /// <summary>
        /// Check if a part label template exists
        /// </summary>
        /// <returns></returns>
        Task<bool> HasPartLabelTemplateAsync();

        /// <summary>
        /// Get the part label template
        /// </summary>
        /// <param name="labelId">Optional labelId to use. If unspecified the default will be chosen.</param>
        /// <returns></returns>
        Task<Label> GetPartLabelTemplateAsync(int? labelId = null);

        /// <summary>
        /// Set the label as the default label template
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        Task<Label?> SetDefaultLabelAsync(UpdateLabelRequest label);

        /// <summary>
        /// Add a new label template
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<LabelTemplate> AddLabelTemplateAsync(LabelTemplate model);

        /// <summary>
        /// Update an existing label template
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<LabelTemplate?> UpdateLabelTemplateAsync(LabelTemplate model);

        /// <summary>
        /// Delete an existing label template
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> DeleteLabelTemplateAsync(LabelTemplate model);

        /// <summary>
        /// Get a label template
        /// </summary>
        /// <returns></returns>
        Task<LabelTemplate?> GetLabelTemplateAsync(int labelTemplateId);

        /// <summary>
        /// Get all label templates
        /// </summary>
        /// <returns></returns>
        Task<ICollection<LabelTemplate>> GetLabelTemplatesAsync();

        /// <summary>
        /// Add a new label
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<Label> AddOrUpdateLabelAsync(Label model);

        /// <summary>
        /// Update an existing label
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<Label> UpdateLabelAsync(Label model);

        /// <summary>
        /// Delete an existing label
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<bool> DeleteLabelAsync(Label model);

        /// <summary>
        /// Get all label
        /// </summary>
        /// <returns></returns>
        Task<ICollection<Label>> GetLabelsAsync();

        /// <summary>
        /// Print a label
        /// </summary>
        /// <param name="part"></param>
        /// <param name="labelId">Optional label id. If not specified the default will be used.</param>
        /// <param name="generateImageOnly"></param>
        /// <returns></returns>
        Task<Stream> PrintAsync(Part part, int? labelId = null, bool generateImageOnly = false);

        /// <summary>
        /// Print a legacy label
        /// </summary>
        /// <param name="part"></param>
        /// <param name="generateImageOnly"></param>
        /// <returns></returns>
        Task<(Stream, Image<Rgba32>)> PrintLegacyAsync(Part part, bool generateImageOnly);

        /// <summary>
        /// Generate a Code 128 Barcode
        /// </summary>
        /// <param name="partNumber">Part number to identify</param>
        /// <param name="foregroundColor">Foreground color to use</param>
        /// <param name="backgroundColor">Background color to use</param>
        /// <param name="width">Barcode image width</param>
        /// <param name="height">Barcode image height</param>
        /// <returns></returns>
        Image<Rgba32> GenerateBarcode(string partNumber, Color foregroundColor, Color backgroundColor, int width, int height);

        /// <summary>
        /// Print a label image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="template"></param>
        /// <param name="generateImageOnly"></param>
        void PrintLabelImage(Image<Rgba32> image, LabelTemplate template, bool generateImageOnly);

        /// <summary>
        /// Print a label
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="printerOptions"></param>
        /// <returns></returns>
        Image<Rgba32> PrintLabel(ICollection<LineConfiguration> lines, PrinterOptions printerOptions);
    }
}
