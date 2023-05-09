using Binner.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binner.Common.Services
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
        /// <returns></returns>
        Task<Label> GetPartLabelTemplateAsync();

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
        Task<LabelTemplate> UpdateLabelTemplateAsync(LabelTemplate model);

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
    }
}
