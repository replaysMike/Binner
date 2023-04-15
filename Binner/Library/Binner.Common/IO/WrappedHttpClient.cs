using Binner.Model.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Binner.Common.IO
{
    public class WrappedHttpClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly HeaderConfiguration _headerConfiguration;
        public HttpRequestHeaders DefaultRequestHeaders => _httpClient.DefaultRequestHeaders;
        public TimeSpan Timeout
        {
            get
            {
                return _httpClient.Timeout;
            }
            set
            {
                _httpClient.Timeout = value;
            }
        }

        public WrappedHttpClient(HeaderConfiguration headerConfiguration, HttpClientHandler httpClientHandler)
        {
            _httpClient = new HttpClient(httpClientHandler);
            _headerConfiguration = headerConfiguration;
        }

        public Task<HttpResponseMessage> GetAsync(string? requestUri)
            => GetAsync(new Uri(requestUri));

        public Task<HttpResponseMessage> GetAsync(string? requestUri, HttpCompletionOption completionOption)
            => GetAsync(new Uri(requestUri), completionOption);

        public Task<HttpResponseMessage> GetAsync(string? requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken)
            => GetAsync(new Uri(requestUri), completionOption, cancellationToken);

        public Task<HttpResponseMessage> GetAsync(string? requestUri, CancellationToken cancellationToken)
            => GetAsync(new Uri(requestUri), cancellationToken);

        public Task<HttpResponseMessage> GetAsync(Uri? requestUri)
        {
            ReorderHeaders(_httpClient.DefaultRequestHeaders, requestUri);
            return _httpClient.GetAsync(requestUri);
        }

        public Task<HttpResponseMessage> GetAsync(Uri? requestUri, HttpCompletionOption completionOption)
        {
            ReorderHeaders(_httpClient.DefaultRequestHeaders, requestUri);
            return _httpClient.GetAsync(requestUri, completionOption);
        }

        public Task<HttpResponseMessage> GetAsync(Uri? requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            ReorderHeaders(_httpClient.DefaultRequestHeaders, requestUri);
            return _httpClient.GetAsync(requestUri, completionOption, cancellationToken);
        }

        public Task<HttpResponseMessage> GetAsync(Uri? requestUri, CancellationToken cancellationToken)
        {
            ReorderHeaders(_httpClient.DefaultRequestHeaders, requestUri);
            return _httpClient.GetAsync(requestUri, cancellationToken);
        }

        public Task<HttpResponseMessage> PostAsync(string? requestUri, HttpContent? content)
        {
            return _httpClient.PostAsync(requestUri, content);
        }

        public Task<HttpResponseMessage> PostAsync(Uri? requestUri, HttpContent? content)
        {
            return _httpClient.PostAsync(requestUri, content);
        }


        /// <summary>
        /// Reorder headers in the order they must be defined in according to configuration
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private void ReorderHeaders(HttpRequestHeaders requestHeaders, Uri requestUri)
        {
            // override header ordering
            if (_headerConfiguration == null)
                return;
            IList<string> headerOrdering = new List<string>();
            if (_headerConfiguration.HeaderFingerprintConfiguration.ContainsKey(requestUri.Host))
                headerOrdering = _headerConfiguration.HeaderFingerprintConfiguration[requestUri.Host];
            else if (_headerConfiguration.HeaderFingerprintConfiguration.ContainsKey("Default"))
                headerOrdering = _headerConfiguration.HeaderFingerprintConfiguration["Default"];
            if (headerOrdering.Any())
            {
                var headers = requestHeaders.ToDictionary(x => x.Key, x => x.Value);
                requestHeaders.Clear();
                // add headers in the order they must be output in
                foreach (var header in headerOrdering)
                {
                    if (headers.ContainsKey(header))
                        requestHeaders.Add(header, headers[header]);
                }
                // add any remaining headers at the end
                foreach (var header in headers)
                {
                    if (!requestHeaders.Contains(header.Key))
                        requestHeaders.Add(header.Key, header.Value);
                }
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
