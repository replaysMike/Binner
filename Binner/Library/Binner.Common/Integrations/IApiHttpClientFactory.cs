namespace Binner.Common.Integrations
{
    public interface IApiHttpClientFactory
    {
        /// <summary>
        /// Create an Api HttpClient
        /// </summary>
        /// <returns></returns>
        IApiHttpClient Create();
    }
}