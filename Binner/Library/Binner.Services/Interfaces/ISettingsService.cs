using Binner.Global.Common;

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
        /// <param name="stripConfiguration">True to strip sections that have been moved to the database</param>
        /// <typeparam name="T"></typeparam>
        Task SaveSettingsAsAsync<T>(T instance, string sectionName, string filename, bool createBackup, bool stripConfiguration);

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

        /// <summary>
        /// Ping the database to ensure it is reachable and operational.
        /// </summary>
        /// <returns></returns>
        Task<bool> PingDatabaseAsync();
    }
}
