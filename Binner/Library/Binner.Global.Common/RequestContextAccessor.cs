using Binner.Global.Common.Extensions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security.Principal;

namespace Binner.Global.Common
{
    /// <summary>
    /// Access items from the request scoped http context
    /// </summary>
    public class RequestContextAccessor : IRequestContextAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// For backend services that don't require a user context, this mocks a basic user context to allow
        /// services to bypass user requirements.
        /// </summary>
        public static void MockUserContextForServices()
        {
            Thread.CurrentPrincipal = new ServiceMockPrincipal();
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
        public IUserContext? GetUserContext()
        {
            var context = _httpContextAccessor.HttpContext;
            ClaimsPrincipal? currentPrincipal = null;
            if (context != null && context.User != null && context.User.Identities.Any(x => x.IsAuthenticated) == true)
                currentPrincipal = context.User;
            // support using the current thread principal if user context is not available
            if (currentPrincipal == null)
                currentPrincipal = System.Threading.Thread.CurrentPrincipal as ClaimsPrincipal;

            if (currentPrincipal?.Identities.Any(x => x.IsAuthenticated) == true)
            {
                return CreateUserContext(currentPrincipal);
            }
            // not authenticated
            return null;
        }

        public UserContext CreateUserContext(ClaimsPrincipal currentPrincipal)
        {
            var identity = currentPrincipal.Identities.FirstOrDefault(x => x.IsAuthenticated);
            var userId = int.Parse(currentPrincipal.Claims.Where(x => x.Type == "UserId").Select(x => x.Value).FirstOrDefault() ?? "0");
            var organizationId = int.Parse(currentPrincipal.Claims.Where(x => x.Type == "OrganizationId").Select(x => x.Value).FirstOrDefault() ?? "0");
            if (userId <= 0 || organizationId <= 0) return null; // invalid claims
            var properties = new Dictionary<string, object?>();
            var subscriptionLevel = SubscriptionLevel.Free;
            if (currentPrincipal.Claims.Any(x => x.Type == JwtClaimTypes.SubscriptionLevel))
            {
                properties.Add(JwtClaimTypes.SubscriptionLevel, currentPrincipal.Claims.Where(x => x.Type == JwtClaimTypes.SubscriptionLevel).Select(x => x.Value).FirstOrDefault() ?? string.Empty);
                Enum.TryParse<SubscriptionLevel>(currentPrincipal.Claims.Where(x => x.Type == JwtClaimTypes.SubscriptionLevel).Select(x => x.Value).FirstOrDefault(), out subscriptionLevel);
            }
            if (currentPrincipal.Claims.Any(x => x.Type == JwtClaimTypes.SuperAdmin))
                properties.Add(JwtClaimTypes.SuperAdmin, currentPrincipal.Claims.Where(x => x.Type == JwtClaimTypes.SuperAdmin).Select(x => x.Value).FirstOrDefault() ?? string.Empty);
            return new UserContext
            {
                UserId = userId,
                OrganizationId = organizationId,
                Name = currentPrincipal.Claims.Where(x => x.Type == "FullName").Select(x => x.Value).FirstOrDefault(),
                EmailAddress = identity?.Name ?? string.Empty,
                PhoneNumber = currentPrincipal.Claims.Where(x => x.Type == "PhoneNumber").Select(x => x.Value).FirstOrDefault(),
                Properties = properties,
                IsAdmin = bool.Parse(currentPrincipal.Claims.Where(x => x.Type == JwtClaimTypes.Admin).Select(x => x.Value).FirstOrDefault() ?? "false"),
                SubscriptionLevel = subscriptionLevel
            };
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
            => _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4().ToLong() ?? 0;

        /// <summary>
        /// Get the remote user's IP address as a string
        /// </summary>
        /// <returns></returns>
        public string? GetIpAddress()
            => _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4().ToString();

        /// <summary>
        /// Get the current http request
        /// </summary>
        /// <returns></returns>
        public HttpContext? GetHttpContext()
            => _httpContextAccessor?.HttpContext;

        /// <summary>
        /// Get the current http request
        /// </summary>
        /// <returns></returns>
        public HttpRequest? GetRequest()
            => _httpContextAccessor?.HttpContext?.Request;

        /// <summary>
        /// Get the current http connection
        /// </summary>
        /// <returns></returns>
        public ConnectionInfo? GetConnection()
            => _httpContextAccessor?.HttpContext?.Connection;

        /// <summary>
        /// Get a header from the current request
        /// </summary>
        /// <param name="headerName">Name of header</param>
        /// <returns></returns>
        public string? GetHeader(string headerName)
        {
            if (_httpContextAccessor?.HttpContext?.Request?.Headers?.TryGetValue(headerName, out var value) == true)
                return value;
            return null;
        }
    }
}
