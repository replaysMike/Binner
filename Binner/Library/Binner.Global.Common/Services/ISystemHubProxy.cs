namespace Binner.Global.Common.Services
{
    public interface ISystemHubProxy
    {
        Task NotifySubscriptionChangeAsync(SubscriptionLevel subscriptionLevel, int organizationId);
    }
}