namespace Binner.Model
{
    public class ServiceResult<T> : IServiceResult<T>
    {
        /// <summary>
        /// Response from the api
        /// </summary>
        public T? Response { get; set; }

        /// <summary>
        /// Requires authentication to continue
        /// </summary>
        public bool RequiresAuthentication { get; set; }

        /// <summary>
        /// A redirect Url location if requires authentication
        /// </summary>
        public string? RedirectUrl { get; set; }

        /// <summary>
        /// Errors
        /// </summary>
        public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Name of api
        /// </summary>
        public string? ApiName { get; set; }

        public ServiceResult(T response)
        {
            Response = response;
        }

        public ServiceResult(IEnumerable<string> errors, string apiName)
        {
            Errors = errors;
            ApiName = apiName;
        }

        public ServiceResult(bool requiresAuthentication, string redirectUrl, IEnumerable<string> errors, string apiName)
        {
            RequiresAuthentication = requiresAuthentication;
            RedirectUrl = redirectUrl;
            Errors = errors;
            ApiName = apiName;
        }

        /// <summary>
        /// No results found, but not an error state.
        /// </summary>
        /// <returns></returns>
        public static ServiceResult<T> NotFound()
        {
            return new ServiceResult<T>(Activator.CreateInstance<T>());
        }

        public static ServiceResult<T> Create(T response)
        {
            return new ServiceResult<T>(response);
        }

        public static ServiceResult<T> Create(string error, string apiName)
        {
            return new ServiceResult<T>(Enumerable.Repeat(error, 1), apiName);
        }

        public static ServiceResult<T> Create(IEnumerable<string> errors, string apiName)
        {
            return new ServiceResult<T>(errors, apiName);
        }

        public static ServiceResult<T> Create(bool requiresAuthentication, string redirectUrl, string error, string apiName)
        {
            return new ServiceResult<T>(requiresAuthentication, redirectUrl, Enumerable.Repeat(error, 1), apiName);
        }

        public static ServiceResult<T> Create(bool requiresAuthentication, string redirectUrl, IEnumerable<string> errors, string apiName)
        {
            return new ServiceResult<T>(requiresAuthentication, redirectUrl, errors, apiName);
        }
    }
}
