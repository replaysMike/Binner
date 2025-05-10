using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Binner.Global.Common
{
    public interface IRequestContextAccessor
    {
        /// <summary>
        /// Get an item from the request context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T? Get<T>(string key);

        /// <summary>
        /// Get the current user context
        /// </summary>
        /// <returns></returns>
        IUserContext? GetUserContext();

        /// <summary>
        /// Get the current user
        /// </summary>
        /// <returns></returns>
        ClaimsPrincipal? GetUser();

        /// <summary>
        /// Set the User for the current request
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        void SetUser(ClaimsPrincipal claimsPrincipal);

        /// <summary>
        /// Set an item in the request context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        void Set<T>(string key, T obj);

        /// <summary>
        /// Get the remote user's IP address as a 64-bit integer
        /// </summary>
        /// <returns></returns>
        long GetIp();

        /// <summary>
        /// Get the remote user's IP address as a string
        /// </summary>
        /// <returns></returns>
        string? GetIpAddress();

        /// <summary>
        /// Get the current http request
        /// </summary>
        /// <returns></returns>
        HttpRequest? GetRequest();

        /// <summary>
        /// Get the current http connection
        /// </summary>
        /// <returns></returns>
        ConnectionInfo? GetConnection();

        /// <summary>
        /// Get a header from the current request
        /// </summary>
        /// <param name="headerName">Name of header</param>
        /// <returns></returns>
        string? GetHeader(string headerName);
    }
}
