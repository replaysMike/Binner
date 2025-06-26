using Binner.Model;
using Binner.Model.Integrations.DigiKey;
using Binner.Model.Requests;

namespace Binner.Services
{
    public interface IExternalCategoriesService
    {
        Task<ServiceResult<CategoriesResponse?>> GetExternalCategoriesAsync(ExternalCategoriesRequest request);
    }
}
