using Binner.Model.Configuration;
using Binner.Model.Requests;
using Binner.Model.Responses;

namespace Binner.Services
{
    public interface IUserConfigurationService
    {
        /// <summary>
        /// Create or update integration configuration
        /// </summary>
        /// <param name="integrationConfiguration"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        Task<OrganizationIntegrationConfiguration> CreateOrUpdateOrganizationIntegrationConfigurationAsync(OrganizationIntegrationConfiguration integrationConfiguration, int? organizationId = null);

        /// <summary>
        /// Create or update user printer configuration
        /// </summary>
        /// <param name="printerConfiguration"></param>
        /// <param name="userId"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        Task<UserPrinterConfiguration> CreateOrUpdatePrinterConfigurationAsync(UserPrinterConfiguration printerConfiguration, int? userId = null, int? organizationId = null);

        /// <summary>
        /// Create or update user configuration
        /// </summary>
        /// <param name="userConfiguration"></param>
        /// <param name="userId"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        Task<UserConfiguration> CreateOrUpdateUserConfigurationAsync(UserConfiguration userConfiguration, int? userId = null, int? organizationId = null);

        /// <summary>
        /// Create or update organization configuration
        /// </summary>
        /// <param name="organizationConfiguration"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        Task<OrganizationConfiguration> CreateOrUpdateOrganizationConfigurationAsync(OrganizationConfiguration organizationConfiguration, int? organizationId = null);

        /// <summary>
        /// Get the organization's integrations configuration
        /// </summary>
        /// <returns></returns>
        Task<OrganizationIntegrationConfiguration> GetOrganizationIntegrationConfigurationAsync(int? organizationId = null);

        /// <summary>
        /// Get the organization's integrations configuration
        /// </summary>
        /// <returns></returns>
        OrganizationIntegrationConfiguration GetCachedOrganizationIntegrationConfiguration(int? organizationId = null);

        /// <summary>
        /// Get the organization's configuration
        /// </summary>
        /// <returns></returns>
        Task<OrganizationConfiguration> GetOrganizationConfigurationAsync(int? organizationId = null);

        /// <summary>
        /// Get the organization's configuration
        /// </summary>
        /// <returns></returns>
        OrganizationConfiguration GetCachedOrganizationConfiguration(int? organizationId = null);

        /// <summary>
        /// Get the user's printer configuration
        /// </summary>
        /// <returns></returns>
        Task<UserPrinterConfiguration> GetPrinterConfigurationAsync(int? userId = null);

        /// <summary>
        /// Get the user's printer configuration
        /// </summary>
        /// <returns></returns>
        UserPrinterConfiguration GetCachedPrinterConfiguration(int? userId = null);

        /// <summary>
        /// Get the user's configuration
        /// </summary>
        /// <returns></returns>
        Task<UserConfiguration> GetUserConfigurationAsync(int? userId = null);

        /// <summary>
        /// Get the user's configuration
        /// </summary>
        /// <returns></returns>
        UserConfiguration GetCachedUserConfiguration(int? userId = null);

        /// <summary>
        /// Clear the cached configurations for a user
        /// </summary>
        /// <param name="userId"></param>
        void ClearCachedUserConfigurations(int? userId = null);

        /// <summary>
        /// Clear the cached configurations for a organization
        /// </summary>
        /// <param name="userId"></param>
        void ClearCachedOrganizationConfigurations(int? organizationId = null);

        /// <summary>
        /// Forget cached oAuth credentials
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TestApiResponse> ForgetCachedCredentialsAsync(ForgetCachedCredentialsRequest request);
    }
}