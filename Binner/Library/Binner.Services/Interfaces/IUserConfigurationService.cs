using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Requests;
using Binner.Model.Responses;

namespace Binner.Services
{
    public interface IUserConfigurationService
    {
        /// <summary>
        /// Create or update user integration configuration
        /// </summary>
        /// <param name="integrationConfiguration"></param>
        /// <returns></returns>
        Task<UserIntegrationConfiguration> CreateOrUpdateIntegrationConfigurationAsync(UserIntegrationConfiguration integrationConfiguration);

        /// <summary>
        /// Create or update user printer configuration
        /// </summary>
        /// <param name="printerConfiguration"></param>
        /// <returns></returns>
        Task<UserPrinterConfiguration> CreateOrUpdatePrinterConfigurationAsync(UserPrinterConfiguration printerConfiguration);

        /// <summary>
        /// Create or update user locale configuration
        /// </summary>
        /// <param name="printerConfiguration"></param>
        /// <returns></returns>
        Task<UserLocaleConfiguration> CreateOrUpdateLocaleConfigurationAsync(UserLocaleConfiguration printerConfiguration);

        /// <summary>
        /// Create or update user barcode configuration
        /// </summary>
        /// <param name="barcodeConfiguration"></param>
        /// <returns></returns>
        Task<UserBarcodeConfiguration> CreateOrUpdateBarcodeConfigurationAsync(UserBarcodeConfiguration barcodeConfiguration);

        /// <summary>
        /// Get the user's integrations configuration
        /// </summary>
        /// <returns></returns>
        Task<UserIntegrationConfiguration> GetIntegrationConfigurationAsync(int? userId = null);

        /// <summary>
        /// Get the user's integrations configuration
        /// </summary>
        /// <returns></returns>
        UserIntegrationConfiguration GetCachedIntegrationConfiguration(int? userId = null);


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
        /// Get the user's locale configuration
        /// </summary>
        /// <returns></returns>
        Task<UserLocaleConfiguration> GetLocaleConfigurationAsync(int? userId = null);

        /// <summary>
        /// Get the user's locale configuration
        /// </summary>
        /// <returns></returns>
        UserLocaleConfiguration GetCachedLocaleConfiguration(int? userId = null);

        /// <summary>
        /// Get the user's barcode configuration
        /// </summary>
        /// <returns></returns>
        Task<UserBarcodeConfiguration> GetBarcodeConfigurationAsync(int? userId = null);

        /// <summary>
        /// Get the user's barcode configuration
        /// </summary>
        /// <returns></returns>
        UserBarcodeConfiguration GetCachedBarcodeConfiguration(int? userId = null);

        /// <summary>
        /// Clear the cached configurations for a user
        /// </summary>
        /// <param name="userId"></param>
        void ClearCachedConfigurations(int? userId = null);

        /// <summary>
        /// Test an api
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TestApiResponse> TestApiAsync(TestApiRequest request);

        /// <summary>
        /// Forget cached oAuth credentials
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<TestApiResponse> ForgetCachedCredentialsAsync(ForgetCachedCredentialsRequest request);
    }
}