using Binner.Common.Integrations;
using System;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public interface IIntegrationApiFactory
    {
        /// <summary>
        /// Create an integration Api for system use
        /// Only for global system usage as no user api keys will be available
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T CreateGlobal<T>() where T : class;


        /// <summary>
        /// Create an integration Api for use by users
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<T> CreateAsync<T>(int userId) where T : class;

        /// <summary>
        /// Create an integration Api for use by users
        /// </summary>
        /// <param name="apiType">Api type to create</param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IIntegrationApi> CreateAsync(Type apiType, int userId);

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
