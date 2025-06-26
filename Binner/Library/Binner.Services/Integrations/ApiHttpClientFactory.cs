namespace Binner.Services.Integrations
{
    public class ApiHttpClientFactory : IApiHttpClientFactory
    {
        /// <summary>
        /// Create an Http client
        /// </summary>
        /// <returns></returns>
        public IApiHttpClient Create()
        {
            return new ApiHttpClient();
        }
    }
}
