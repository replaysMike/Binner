using Binner.Model;

namespace Binner.Services;

public interface IVersionManagementService
{
    /// <summary>
    /// Get the latest version of Binner
    /// </summary>
    /// <returns></returns>
    Task<VersionManagementService.BinnerVersion> GetLatestVersionAsync();

    /// <summary>
    /// Get the latest system messages
    /// </summary>
    /// <returns></returns>
    Task<ICollection<MessageState>> GetSystemMessagesAsync();

    /// <summary>
    /// Update the system messages as read
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task UpdateSystemMessagesReadAsync(UpdateSystemMessagesRequest request);
}