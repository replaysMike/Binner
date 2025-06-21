using Binner.Model;
using Binner.Model.Integrations.DigiKey;
using Binner.Model.Requests;

namespace Binner.Services.Integrations.Categories
{
    public interface IApiExternalCategoriesService
    {
        Task<ServiceResult<CategoriesResponse?>> GetExternalCategoriesAsync(ExternalCategoriesRequest request);
    }
}
