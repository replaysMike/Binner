using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Binner.Common.Integrations
{
    public class ApiHttpClient : IApiHttpClient
    {
        private HttpClientHandler _handler;
        private HttpClient _httpClient;

        public HttpRequestHeaders DefaultRequestHeaders => _httpClient.DefaultRequestHeaders;

        public ApiHttpClient()
        {
            _handler = new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 5,
                //SslProtocols = System.Security.Authentication.SslProtocols.Tls13 | System.Security.Authentication.SslProtocols.Tls12,
                SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer(),
                AutomaticDecompression = (DecompressionMethods.GZip | DecompressionMethods.Deflate),
                // for SocketsHttpHandler
                //PooledConnectionLifetime = configuration.PooledConnectionLifetime,
                //ConnectTimeout = configuration.ConnectTimeout,
                //ResponseDrainTimeout = configuration.ResponseDrainTimeout
            };
            _httpClient = new HttpClient(_handler);
            //_httpClient = new HttpClient();
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return _httpClient.SendAsync(request);
        }

        public void AddHeader(string name, string value)
        {
            _httpClient.DefaultRequestHeaders.Add(name, value);
        }

        public void AddHeader(MediaTypeWithQualityHeaderValue value)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(value);
        }

        public void RemoveHeader(string name)
        {
            _httpClient.DefaultRequestHeaders.Remove(name);
        }

        public void ClearHeaders()
        {
            _httpClient.DefaultRequestHeaders.Clear();
        }

        public void SetTimeout(TimeSpan timeout)
        {
            _httpClient.Timeout = timeout;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            _handler.Dispose();
        }
    }
}
