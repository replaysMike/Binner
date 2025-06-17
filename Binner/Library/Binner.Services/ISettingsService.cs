using Binner.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binner.Services
{
    /// <summary>
    /// System settings service
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Save system settings
        /// </summary>
        /// <param name="instance">The configuration object to save</param>
        /// <param name="filename">The filename to save settings to</param>
        /// <param name="sectionName">The section name to replace</param>
        /// <param name="createBackup">True to create a backup before saving</param>
        /// <typeparam name="T"></typeparam>
        Task SaveSettingsAsAsync<T>(T instance, string sectionName, string filename, bool createBackup);

        /// <summary>
        /// Get list of custom fields
        /// </summary>
        /// <returns></returns>
        Task<ICollection<CustomField>> GetCustomFieldsAsync();

        /// <summary>
        /// Save the list of custom fields. New fields will be added, existing fields will be updated.
        /// </summary>
        /// <param name="customFields"></param>
        /// <returns></returns>
        Task<ICollection<CustomField>> SaveCustomFieldsAsync(ICollection<CustomField> customFields);
    }
}
