namespace Binner.Global.Common.Services
{
    public interface ISystemHubProxy
    {
        /// <summary>
        /// Notify of a subscription level change
        /// </summary>
        /// <param name="subscriptionLevel"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        Task NotifySubscriptionChangeAsync(SubscriptionLevel subscriptionLevel, int organizationId);

        /// <summary>
        /// Notify of a print completed
        /// </summary>
        /// <param name="partName"></param>
        /// <param name="userId"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        Task NotifyPrintCompleteAsync(string partName, int userId, int organizationId);
    }
}