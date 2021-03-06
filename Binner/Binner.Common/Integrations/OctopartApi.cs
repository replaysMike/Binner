﻿using Binner.Common.Extensions;
using Binner.Common.Integrations.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Binner.Common.Integrations
{
    public class OctopartApi : IIntegrationApi
    {
        public const string BasePath = "/api/v3/parts";
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public bool IsConfigured => !string.IsNullOrEmpty(_apiKey) && !string.IsNullOrEmpty(_apiUrl);

        public OctopartApi(string apiKey, string apiUrl, IHttpContextAccessor httpContextAccessor)
        {
            _apiKey = apiKey;
            _apiUrl = apiUrl;
            _httpContextAccessor = httpContextAccessor;
            _client = new HttpClient();
        }

        public async Task<IApiResponse> GetDatasheetsAsync(string partNumber)
        {
            var datasheets = new List<string>();
            partNumber = "SN74S74N";
            var values = new Dictionary<string, string>
            {
                // { "apikey", _apiKey },
                { "apikey", "EXAMPLE_KEY" },
                { "queries" , $@"[{{""mpn"":""{partNumber}""}}]"},
                { "pretty_print", "true" },
                { "include[]", "datasheets" },
            };
            System.Environment.SetEnvironmentVariable("MONO_URI_IRIPARSING", "true");
            var uri = Url.Combine(false, _apiUrl, BasePath, $"/match?" + string.Join("&", values.Select(x => $"{x.Key}={x.Value}")));

            var k = uri.ToString();
            var req = new HttpRequestMessage(HttpMethod.Get, uri);
            // req.Content = new FormUrlEncodedContent(values);
            var response = await _client.SendAsync(req);
            if (response.IsSuccessStatusCode)
            {
                var resultString = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<dynamic>(resultString);
                // dynamic requires Microsoft.CSharp nuget package
                foreach(var r in result["results"])
                {
                    foreach(var item in r["items"])
                    {
                        foreach(var datasheet in item["datasheets"])
                        {
                            var url = datasheet["url"];
                            datasheets.Add(url.Value);
                        }
                    }
                }
                // parse into readable format
            }
            return new ApiResponse(datasheets, nameof(OctopartApi));
        }
    }
}
