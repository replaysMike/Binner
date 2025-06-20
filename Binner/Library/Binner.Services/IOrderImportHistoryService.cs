using Binner.Model;

namespace Binner.Services
{
    public interface IOrderImportHistoryService
    {
        Task<OrderImportHistory?> AddOrderImportHistoryAsync(OrderImportHistory orderImportHistory);
        Task<OrderImportHistoryLineItem?> AddOrderImportHistoryLineItemAsync(OrderImportHistoryLineItem orderImportHistoryLineItem);
        Task<bool> DeleteOrderImportHistoryAsync(OrderImportHistory orderImportHistory);
        Task<OrderImportHistory?> GetOrderImportHistoryAsync(OrderImportHistory orderImportHistory, bool includeChildren = false);
        Task<OrderImportHistory?> GetOrderImportHistoryAsync(long orderImportHistoryId, bool includeChildren = false);
        Task<OrderImportHistory?> GetOrderImportHistoryAsync(string orderNumber, string supplier, bool includeChildren = false);
        Task<OrderImportHistory?> UpdateOrderImportHistoryAsync(OrderImportHistory orderImportHistory);
    }
}