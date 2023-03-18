namespace Binner.Common.Models.Requests
{
    public class RemoveBomPcbRequest
    {
        public long ProjectId { get; set; }
        public long PcbId { get; set; }
    }
}
