using Binner.Model;
using Binner.Model.Requests;
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
    }
}
