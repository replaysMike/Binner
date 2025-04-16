using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Binner.Common.Integrations
{
    public interface IApiHttpClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
        void AddHeader(string name, string value);
        void AddHeader(MediaTypeWithQualityHeaderValue value);
    }
}