using Binner.Common.Integrations;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Services.Integrations;

namespace Binner.Services
{
    public interface IIntegrationApiFactory
    {
        /// <summary>
        /// Create an integration Api for system use
        /// Only for global system usage as no user api keys will be available
        /// </summary>
        /// <param name="integrationConfiguration">Integration configuration to use for the apis</param>
        /// <typeparam name="T">Type of api class</typeparam>
        /// <returns></returns>
        T CreateGlobal<T>(IntegrationConfiguration integrationConfiguration) where T : class;

        /// <summary>
        /// Create an integration Api for system use
        /// Only for global system usage as no user api keys will be available
        /// </summary>
        /// <param name="apiType">Type of api class</param>
        /// <param name="integrationConfiguration">Integration configuration to use for the apis</param>
        /// <returns></returns>
        IIntegrationApi CreateGlobal(Type apiType, IntegrationConfiguration integrationConfiguration);

        /// <summary>
        /// Create an integration Api for use by users
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<T> CreateAsync<T>(int userId, OrganizationIntegrationConfiguration integrationConfiguration) where T : class;

        /// <summary>
        /// Create an integration Api for use by users
        /// </summary>
        /// <param name="apiType">Api type to create</param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IIntegrationApi> CreateAsync(Type apiType, int userId, OrganizationIntegrationConfiguration integrationConfiguration);

        /// <summary>
        /// Create an integration Api for use by users
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userId"></param>
        /// <param name="getCredentialsMethod"></param>
        /// <param name="cache">True to enable caching of credentials</param>
        /// <returns></returns>
        Task<T> CreateAsync<T>(int userId, Func<Task<ApiCredentialConfiguration>> getCredentialsMethod, bool cache = true) where T : class;

        /// <summary>
        /// Create an integration Api for use by users
        /// </summary>
        /// <param name="apiType"></param>
        /// <param name="userId"></param>
        /// <param name="getCredentialsMethod"></param>
        /// <param name="cache">True to enable caching of credentials</param>
        /// <returns></returns>
        Task<IIntegrationApi> CreateAsync(Type apiType, int userId, Func<Task<ApiCredentialConfiguration>> getCredentialsMethod, bool cache = true);
    }
}
