using System.Threading.Tasks;
using Binner.Model.Responses;

namespace Binner.Common.Services;

public interface IAdminService
{
    /// <summary>
    /// Get system information
    /// </summary>
    /// <returns></returns>
    Task<SystemInfoResponse> GetSystemInfoAsync();
}