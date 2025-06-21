using Binner.Model;
using Binner.Model.Integrations.DigiKey;
using Binner.Model.Requests;
using Binner.Services.Integrations.Categories;

namespace Binner.Services
{
    public class ExternalCategoriesService : IExternalCategoriesService
    {
        private readonly IDigiKeyExternalCategoriesService _digiKeyExternalCategoriesService;

        public ExternalCategoriesService(IDigiKeyExternalCategoriesService digiKeyExternalCategoriesService)
        {
            _digiKeyExternalCategoriesService = digiKeyExternalCategoriesService;
        }

        public async Task<ServiceResult<CategoriesResponse?>> GetExternalCategoriesAsync(ExternalCategoriesRequest request)
        {
            if (string.IsNullOrEmpty(request.Supplier)) throw new InvalidOperationException($"OrderId must be provided");

            switch (request.Supplier?.ToLower())
            {
                case "digikey":
                    return await _digiKeyExternalCategoriesService.GetExternalCategoriesAsync(request);
                case "mouser":
                    throw new NotSupportedException();
                case "arrow":
                    throw new NotSupportedException();
                case "tme":
                    throw new NotSupportedException();
                default:
                    return await _digiKeyExternalCategoriesService.GetExternalCategoriesAsync(request);
            }
        }
    }
}
