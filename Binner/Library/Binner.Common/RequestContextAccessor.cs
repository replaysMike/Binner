using Binner.Common.Extensions;
using Binner.Model;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;

namespace Binner.Common
{
    /// <summary>
    /// Access items from the request scoped http context
    /// </summary>
    public class RequestContextAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Get an item from the request context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T? Get<T>(string key)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                if (context.Items.ContainsKey(key) && context.Items[key].GetType() == typeof(T))
                {
                    return (T)context.Items[key];
                }
            }
            return default(T);
        }

        /// <summary>
        /// Get the current user context
        /// </summary>
        /// <returns></returns>
        public UserContext? GetUserContext()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null && context.User != null && context.User.Identity?.IsAuthenticated == true)
            {
                return new UserContext
                {
                    UserId = int.Parse(context.User.Claims.Where(x => x.Type == "UserId").Select(x => x.Value).FirstOrDefault() ?? "0"),
                    OrganizationId = int.Parse(context.User.Claims.Where(x => x.Type == "OrganizationId").Select(x => x.Value).FirstOrDefault() ?? "0"),
                    Name = context.User.Claims.Where(x => x.Type == "Name").Select(x => x.Value).FirstOrDefault(),
                    EmailAddress = context.User.Identity.Name,
                    PhoneNumber = context.User.Claims.Where(x => x.Type == "PhoneNumber").Select(x => x.Value).FirstOrDefault()
                };
            }
            // todo: migrate
            return new UserContext
            {
                UserId = 1,
                OrganizationId = 1,
                Name = "Admin",
                EmailAddress = "admin"
            };
            //return null;
        }

        /// <summary>
        /// Get the current user
        /// </summary>
        /// <returns></returns>
        public ClaimsPrincipal? GetUser()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                return context.User;
            }
            return null;
        }

        /// <summary>
        /// Set the User for the current request
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        public void SetUser(ClaimsPrincipal claimsPrincipal)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                context.User = claimsPrincipal;
            }
        }

        /// <summary>
        /// Set an item in the request context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public void Set<T>(string key, T obj)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                if (!context.Items.ContainsKey(key))
                {
                    context.Items.Add(key, obj);
                }
            }
        }

        /// <summary>
        /// Get the remote user's IP address as a 64-bit integer
        /// </summary>
        /// <returns></returns>
        public long GetIp() 
            => _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToLong() ?? 0;

        /// <summary>
        /// Get the remote user's IP address as a string
        /// </summary>
        /// <returns></returns>
        public string? GetIpAddress()
            => _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
    }
}
