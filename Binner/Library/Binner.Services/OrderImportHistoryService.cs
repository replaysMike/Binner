using Binner.Common.IO;
using Binner.Global.Common;
using Binner.Model;
using System.Threading.Tasks;

namespace Binner.Services
{
    public class OrderImportHistoryService : IOrderImportHistoryService
    {
        private readonly IStorageProvider _storageProvider;
        private readonly IRequestContextAccessor _requestContext;

        public OrderImportHistoryService(IStorageProvider storageProvider, IRequestContextAccessor requestContextAccessor)
        {
            _storageProvider = storageProvider;
            _requestContext = requestContextAccessor;
        }

        public async Task<OrderImportHistory?> AddOrderImportHistoryAsync(OrderImportHistory orderImportHistory)
        {
            var user = _requestContext.GetUserContext();
            return await _storageProvider.AddOrderImportHistoryAsync(orderImportHistory, user);
        }

        public async Task<OrderImportHistoryLineItem?> AddOrderImportHistoryLineItemAsync(OrderImportHistoryLineItem orderImportHistoryLineItem)
        {
            var user = _requestContext.GetUserContext();
            return await _storageProvider.AddOrderImportHistoryLineItemAsync(orderImportHistoryLineItem, user);
        }

        public async Task<bool> DeleteOrderImportHistoryAsync(OrderImportHistory orderImportHistory)
        {
            var user = _requestContext.GetUserContext();
            return await _storageProvider.DeleteOrderImportHistoryAsync(orderImportHistory, _requestContext.GetUserContext());
        }

        public async Task<OrderImportHistory?> GetOrderImportHistoryAsync(string orderNumber, string supplier, bool includeChildren = false)
        {
            return await _storageProvider.GetOrderImportHistoryAsync(orderNumber, supplier, includeChildren, _requestContext.GetUserContext());
        }

        public async Task<OrderImportHistory?> GetOrderImportHistoryAsync(long orderImportHistoryId, bool includeChildren = false)
        {
            return await _storageProvider.GetOrderImportHistoryAsync(orderImportHistoryId, includeChildren, _requestContext.GetUserContext());
        }

        public async Task<OrderImportHistory?> GetOrderImportHistoryAsync(OrderImportHistory orderImportHistory, bool includeChildren = false)
        {
            return await _storageProvider.GetOrderImportHistoryAsync(orderImportHistory, includeChildren, _requestContext.GetUserContext());
        }

        public async Task<OrderImportHistory?> UpdateOrderImportHistoryAsync(OrderImportHistory orderImportHistory)
        {
            return await _storageProvider.UpdateOrderImportHistoryAsync(orderImportHistory, _requestContext.GetUserContext());
        }
    }
}
