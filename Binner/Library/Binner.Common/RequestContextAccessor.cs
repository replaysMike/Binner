using System;
using Binner.Common.Models;
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
                    Name = context.User.Claims.Where(x => x.Type == "Name").Select(x => x.Value).FirstOrDefault(),
                    EmailAddress = context.User.Identity.Name,
                    PhoneNumber = context.User.Claims.Where(x => x.Type == "PhoneNumber").Select(x => x.Value).FirstOrDefault()
                };
            }
            return null;
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
    }
}
