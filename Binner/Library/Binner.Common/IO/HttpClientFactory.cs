using Binner.Model.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Binner.Common.IO
{
    /// <summary>
    /// Create an HttpClient
    /// </summary>
    public class HttpClientFactory : IDisposable
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);
        private readonly TimeSpan _timeout;
        private readonly HeaderConfiguration _headerConfiguration;
        private readonly Dictionary<string, string> _defaultCustomHeaders;
        private WrappedHttpClient? _httpClient;
        private HttpClientHandler? _httpClientHandler;
        private bool _isDisposed;

        public Dictionary<string, string> DefaultCustomHeaders => _defaultCustomHeaders;

        public HttpClientFactory() : this(new HeaderConfiguration())
        {
        }

        public HttpClientFactory(HeaderConfiguration headerConfiguration) : this(headerConfiguration, DefaultTimeout)
        {
        }

        public HttpClientFactory(HeaderConfiguration headerConfiguration, TimeSpan timeout)
        {
            _headerConfiguration = headerConfiguration;
            _defaultCustomHeaders = headerConfiguration.AddHeaders.ToDictionary(x => x.Name, x => x.Value);
            _timeout = timeout;
        }

        /// <summary>
        /// Create a cached HttpClient
        /// </summary>
        /// <returns></returns>
        public WrappedHttpClient Create()
        {
            if (_httpClientHandler == null)
                _httpClientHandler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    AllowAutoRedirect = true,
                    MaxAutomaticRedirections = 10,
                    MaxConnectionsPerServer = 2,
                    UseCookies = true,
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                };
            if (_httpClient == null)
                _httpClient = BuildHttpClient(_httpClientHandler);
            return _httpClient;
        }

        private WrappedHttpClient BuildHttpClient(HttpClientHandler clientHandler)
        {
            var httpClient = new WrappedHttpClient(_headerConfiguration, clientHandler);
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            httpClient.DefaultRequestHeaders.ConnectionClose = false;
            if (_defaultCustomHeaders.Any())
            {
                foreach (var customHeader in _defaultCustomHeaders)
                {
                    if (httpClient.DefaultRequestHeaders.Contains(customHeader.Key))
                        httpClient.DefaultRequestHeaders.Remove(customHeader.Key);
                    httpClient.DefaultRequestHeaders.Add(customHeader.Key, customHeader.Value);
                }
            }

            httpClient.Timeout = _timeout;
            return httpClient;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _httpClient?.Dispose();
                    _httpClientHandler?.Dispose();
                }
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
