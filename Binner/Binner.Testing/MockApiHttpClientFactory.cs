using Binner.Common.Integrations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;

namespace Binner.Testing
{
    public class MockApiHttpClientFactory : IApiHttpClientFactory
    {
        public Dictionary<string, string> UriFileMapping { get; set; }

        public MockApiHttpClientFactory() { }

        public MockApiHttpClientFactory(Dictionary<string, string> uriFileMapping)
        {
            UriFileMapping = uriFileMapping;
        }

        public IApiHttpClient Create()
        {
            if (!UriFileMapping.Any()) throw new ArgumentNullException("Filename was not set!");
            return new MockApiHttpClient(UriFileMapping);
        }
    }

    public class MockApiHttpClient : IApiHttpClient
    {
        public Dictionary<string, string> UriFileMapping { get; set; }

        public MockApiHttpClient(Dictionary<string, string> uriFileMapping)
        {
            UriFileMapping = uriFileMapping;
        }

        public void AddHeader(string name, string value) { }

        public void AddHeader(MediaTypeWithQualityHeaderValue value) { }

        public void ClearHeaders() { }

        public void Dispose() { }

        public void RemoveHeader(string name) { }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            var uriKey = request.RequestUri!.AbsoluteUri;
            if (!UriFileMapping.ContainsKey(uriKey))
                uriKey = request.RequestUri.PathAndQuery;
            if (!UriFileMapping.ContainsKey(uriKey))
                uriKey = request.RequestUri.AbsolutePath;
            if (!UriFileMapping.ContainsKey(uriKey)) throw new KeyNotFoundException($"No mapping specified for uri using any of the following paths '{request.RequestUri!.AbsoluteUri}', '{request.RequestUri!.PathAndQuery}', '{request.RequestUri!.AbsolutePath}'");

            var filename = UriFileMapping[uriKey];
            var json = LoadResponseJson(filename);
            try
            {
                var obj = JObject.Parse(json);
                var response = new HttpResponseMessage((HttpStatusCode)(obj?["StatusCode"]?.Value<int>() ?? 500));
                var content = obj?["Content"];
                var headers = content?["Headers"];
                var contentType = new MediaTypeHeaderValue(headers?["Content-Type"]?.Value<string?>() ?? "application/json");
                var body = content?["Body"];
                var bodyJson = JsonConvert.SerializeObject(body);
                response.Content = new StringContent(bodyJson, contentType);
                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse test response json!", ex);
            }
        }

        public void SetTimeout(TimeSpan timeout) { }

        private string LoadResponseJson(string filename)
        {
            var fullPath = Path.GetFullPath($"./IntegrationApiResponses/{filename}");
            if (!File.Exists(fullPath)) throw new FileNotFoundException(fullPath);
            return File.ReadAllText(fullPath);
        }
    }
}
