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
        /// <param name="userId"></param>
        /// <returns></returns>
        T CreateGlobal<T>() where T : class;


        /// <summary>
        /// Create an integration Api for use by users
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<T> CreateAsync<T>(int userId) where T : class;
    }
}
