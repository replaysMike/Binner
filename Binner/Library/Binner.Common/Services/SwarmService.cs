using AutoMapper;
using Binner.Common.Models;
using Binner.Common.Models.Swarm.Requests;
using Binner.Common.Models.Swarm.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public class SwarmService : ISwarmService
    {
        private readonly IMapper _mapper;
        private readonly RequestContextAccessor _requestContext;

        public SwarmService(IMapper mapper, RequestContextAccessor requestContext)
        {
            _mapper = mapper;
            _requestContext = requestContext;
        }

        public Task<SearchPartsResponse> SearchPartsAsync(SearchPartsRequest request)
        {
            return Task.FromResult(new SearchPartsResponse
            {
                Parts = new List<PartNumber>()
            });
        }
    }
}
