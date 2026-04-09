namespace Binner.Global.Common.Services
{
    public interface IPrintHubProxy
    {
        /// <summary>
        /// Notify of a print job
        /// </summary>
        /// <param name="printSpoolQueueId"></param>
        /// <returns></returns>
        Task NotifyPrintAsync(Guid printSpoolQueueId);
    }
}