using System.Threading.Tasks;

namespace Binner.Services;

public interface IVersionManagementService
{
    /// <summary>
    /// Get the latest version of Binner
    /// </summary>
    /// <returns></returns>
    Task<VersionManagementService.BinnerVersion> GetLatestVersionAsync();
}