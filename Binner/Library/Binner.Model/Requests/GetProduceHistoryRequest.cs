namespace Binner.Model.Requests
{
    public class GetProduceHistoryRequest : PaginatedRequest
    {
        public long ProjectId { get; set; }
    }
}
