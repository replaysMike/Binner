using Binner.Model.Common;

namespace Binner.Common.Models.Requests
{
    public class GetProduceHistoryRequest : PaginatedRequest
    {
        public long ProjectId { get; set; }
    }
}
