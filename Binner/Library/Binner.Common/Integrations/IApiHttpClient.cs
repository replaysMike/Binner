using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Binner.Common.Integrations
{
    public interface IApiHttpClient : IDisposable
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
        void ClearHeaders();
        void AddHeader(string name, string value);
        void AddHeader(MediaTypeWithQualityHeaderValue value);
        void RemoveHeader(string name);
        void SetTimeout(TimeSpan timeout);
    }
}