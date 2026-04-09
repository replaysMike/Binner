using Binner.Model;
using Binner.Model.Responses;

namespace Binner.Services
{
    public interface IPrintSpoolQueueService
    {
        /// <summary>
        /// Get the printer configuration
        /// </summary>
        /// <param name="printSpoolQueueId"></param>
        /// <returns></returns>
        Task<PrinterConfigurationResponse?> GetConfigurationAsync(Guid printSpoolQueueId);

        /// <summary>
        /// Get the pending print queue
        /// </summary>
        /// <param name="printSpoolQueueId"></param>
        /// <returns></returns>
        Task<PrintSpoolQueueResponse?> GetPendingAsync(Guid printSpoolQueueId);

        /// <summary>
        /// Delete a queued print item (mark it as printed)
        /// </summary>
        /// <param name="printSpoolQueueId"></param>
        /// <param name="globalId"></param>
        /// <returns></returns>
        Task<bool> DeletePrintSpoolQueueAsync(Guid printSpoolQueueId, Guid globalId);

        /// <summary>
        /// Queue a print job
        /// </summary>
        /// <param name="part"></param>
        /// <param name="label"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        Task<bool> QueuePrintAsync(Part part, Label? label = null, LabelTemplate? template = null);
    }
}
