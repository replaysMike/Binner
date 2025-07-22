using System.Threading.Tasks;
using Binner.Model;
using Binner.Model.Responses;

namespace Binner.Services;

public interface IAdminService
{
    /// <summary>
    /// Get system information
    /// </summary>
    /// <returns></returns>
    Task<SystemInfoResponse> GetSystemInfoAsync();

    /// <summary>
    /// Get system information
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<PaginatedResponse<SystemLogsResponse>> GetSystemLogsAsync(PaginatedRequest request);
}