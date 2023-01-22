using Binner.Common.Models.Swarm.Requests;
using Binner.Common.Models.Swarm.Responses;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    /// <summary>
    /// Swarm manages open collection of metadata that can be submitted by the public
    /// </summary>
    public interface ISwarmService
    {
        /// <summary>
        /// Search for parts
        /// </summary>
        /// <param name="request">The request filtering parameters</param>
        /// <returns></returns>
        Task<SearchPartsResponse> SearchPartsAsync(SearchPartsRequest request);
    }
}
