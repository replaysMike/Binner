using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Binner.Common.Integrations
{
    public class ApiHttpClient : IApiHttpClient
    {
        private HttpClient _httpClient;

        public HttpRequestHeaders DefaultRequestHeaders => _httpClient.DefaultRequestHeaders;

        public ApiHttpClient()
        {
            _httpClient = new HttpClient();
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
    }
}
