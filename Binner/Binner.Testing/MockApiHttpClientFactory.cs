using Binner.Common.Integrations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;

namespace Binner.Testing
{
    public class MockApiHttpClientFactory : IApiHttpClientFactory
    {
        public string Filename { get; set; } = string.Empty;

        public MockApiHttpClientFactory() { }

        public MockApiHttpClientFactory(string filename)
        {
            Filename = filename;
        }

        public IApiHttpClient Create()
        {
            if (string.IsNullOrEmpty(Filename)) throw new ArgumentNullException("Filename was not set!");
            return new MockApiHttpClient(Filename);
        }
    }

    public class MockApiHttpClient : IApiHttpClient
    {
        private string _filename;
        public MockApiHttpClient(string filename)
        {
            _filename = filename;
        }

        public void AddHeader(string name, string value) { }

        public void AddHeader(MediaTypeWithQualityHeaderValue value) { }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            var json = LoadResponseJson(_filename);
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

        private string LoadResponseJson(string filename)
        {
            var fullPath = Path.GetFullPath($"./IntegrationApiResponses/{filename}");
            if (!File.Exists(fullPath)) throw new FileNotFoundException(fullPath);
            return File.ReadAllText(fullPath);
        }
    }
}
