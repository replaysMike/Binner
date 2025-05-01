using Binner.Common.Extensions;
using Binner.Common.Integrations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;

namespace Binner.Testing
{
    public class MockApiHttpClientFactory : IApiHttpClientFactory
    {
        public Dictionary<string, string> UriFileMapping { get; set; } = new();

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

    internal static class UrlFileMappingExtensions
    {
        internal static bool ContainsUrlKey(this Dictionary<string, string> collection, string uriKey, out string key)
        {
            var tryKey = uriKey.Split("/", StringSplitOptions.RemoveEmptyEntries);

            key = string.Join("/", tryKey);
            if (collection.ContainsKey(key)) return true;
            key = string.Join("/", tryKey) + "/";
            if (collection.ContainsKey(key)) return true;
            key = "/" + string.Join("/", tryKey);
            if (collection.ContainsKey(key)) return true;
            key = "/" + string.Join("/", tryKey) + "/";
            if (collection.ContainsKey(key)) return true;

            return false;
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
            var key = uriKey;
            if (!UriFileMapping.ContainsUrlKey(key, out key))
                key = request.RequestUri.PathAndQuery;
            if (!UriFileMapping.ContainsUrlKey(key, out key))
                key = request.RequestUri.AbsolutePath;
            if (!UriFileMapping.ContainsUrlKey(key, out key))
            {
                // iteratively try to find a match with one less path parameter. as some url's pass a search param using the path
                var parts = request.RequestUri.AbsolutePath.Split("/", StringSplitOptions.RemoveEmptyEntries);
                for(var i = parts.Length - 1; i > 1; i--)
                {
                    key = string.Join("/", parts.Take(i).ToList());
                    if (UriFileMapping.ContainsUrlKey(key, out key)) break;
                }
            }
            if (!UriFileMapping.ContainsUrlKey(key, out key)) throw new KeyNotFoundException($"No mapping specified for uri using any of the following paths '{request.RequestUri!.AbsoluteUri}', '{request.RequestUri!.PathAndQuery}', '{request.RequestUri!.AbsolutePath}'");

            var filename = UriFileMapping[key];
            var json = LoadResponseJson(filename);
            try
            {
                var obj = JObject.Parse(json);
                var response = new HttpResponseMessage((HttpStatusCode)(obj?["StatusCode"]?.Value<int>() ?? 500));
                var content = obj?["Content"];          // data specific to the request (request headers & message body)
                var description = obj?["Descripton"];   // test description, user provided
                var headers = content?["Headers"];      // request headers to send
                var body = content?["Body"];            // response body
                var contentType = new MediaTypeHeaderValue(headers?["Content-Type"]?.Value<string?>() ?? "application/json");
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
