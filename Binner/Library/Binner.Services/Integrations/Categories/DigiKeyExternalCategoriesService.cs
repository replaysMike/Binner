using AutoMapper;
using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Integrations.DigiKey;
using Binner.Model.Requests;
using V3 = Binner.Model.Integrations.DigiKey.V3;
using V4 = Binner.Model.Integrations.DigiKey.V4;

namespace Binner.Services.Integrations.Categories
{
    public class DigiKeyExternalCategoriesService : ApiExternalCategoriesServiceBase, IDigiKeyExternalCategoriesService
    {
        protected readonly WebHostServiceConfiguration _configuration;
        protected readonly IIntegrationApiFactory _integrationApiFactory;
        protected readonly IMapper _mapper;
        protected readonly IUserConfigurationService _userConfigurationService;

        public DigiKeyExternalCategoriesService(WebHostServiceConfiguration configuration, IStorageProvider storageProvider, IIntegrationApiFactory integrationApiFactory, IRequestContextAccessor requestContextAccessor, IMapper mapper, IUserConfigurationService userConfigurationService)
            : base(storageProvider, requestContextAccessor)
        {
            _configuration = configuration;
            _integrationApiFactory = integrationApiFactory;
            _mapper = mapper;
            _userConfigurationService = userConfigurationService;
        }

        public async Task<ServiceResult<CategoriesResponse?>> GetExternalCategoriesAsync(ExternalCategoriesRequest request)
        {
            var user = _requestContext.GetUserContext() ?? throw new UserContextUnauthorizedException();
            var integrationConfiguration = _userConfigurationService.GetCachedIntegrationConfiguration(user.UserId);
            var digikeyApi = await _integrationApiFactory.CreateAsync<Integrations.DigikeyApi>(user.UserId, integrationConfiguration);
            if (!digikeyApi.IsEnabled)
                return ServiceResult<CategoriesResponse?>.Create("Api is not enabled.", nameof(Integrations.DigikeyApi));

            var apiResponse = await digikeyApi.GetCategoriesAsync();
            if (apiResponse.RequiresAuthentication)
                return ServiceResult<CategoriesResponse?>.Create(true, apiResponse.RedirectUrl ?? string.Empty, apiResponse.Errors, apiResponse.ApiName);
            else if (apiResponse.Errors?.Any() == true)
                return ServiceResult<CategoriesResponse?>.Create(apiResponse.Errors, apiResponse.ApiName);

            if (apiResponse.Response != null)
            {
                if (apiResponse.Response is V3.CategoriesResponse)
                {
                    var digikeyResponse = (V3.CategoriesResponse)apiResponse.Response;
                    return ServiceResult<CategoriesResponse?>.Create(_mapper.Map<CategoriesResponse>(digikeyResponse));
                }
                else if (apiResponse.Response is V4.CategoriesResponse)
                {
                    var digikeyResponse = (V4.CategoriesResponse)apiResponse.Response;
                    return ServiceResult<CategoriesResponse?>.Create(_mapper.Map<CategoriesResponse>(digikeyResponse));
                }
            }

            return ServiceResult<CategoriesResponse?>.Create("Invalid response received", apiResponse.ApiName);
        }
    }
}
