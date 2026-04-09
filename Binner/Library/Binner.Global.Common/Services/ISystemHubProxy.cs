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
    }
}