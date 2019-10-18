using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Binner.Common.Integrations
{
    public class OctopartApi : IIntegrationApi
    {
        public const string Path = "/api/v3/parts";
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly HttpClient _client;
        public bool IsConfigured => !string.IsNullOrEmpty(_apiKey) && !string.IsNullOrEmpty(_apiUrl);

        public OctopartApi(string apiKey, string apiUrl)
        {
            _apiKey = apiKey;
            _apiUrl = apiUrl;
            _client = new HttpClient();
        }

        public async Task<ICollection<string>> GetDatasheetsAsync(string partNumber)
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
            var uri = new Uri($"{Path}/match?" + string.Join("&", values.Select(x => $"{x.Key}={x.Value}")));
            ForceCanonicalPathAndQuery(uri);
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
            return datasheets;
        }

        void ForceCanonicalPathAndQuery(Uri uri)
        {
            string paq = uri.PathAndQuery; // need to access PathAndQuery
            var flagsFieldInfo = typeof(Uri).GetField("_flags", BindingFlags.Instance | BindingFlags.NonPublic);
            var flags = (ulong)flagsFieldInfo.GetValue(uri);
            var flagsBefore = flags;
            // System.Uri.Flags.E_QueryNotCanonical | System.Uri.Flags.DnsHostType | System.Uri.Flags.AuthorityFound | System.Uri.Flags.MinimalUriInfoSet | System.Uri.Flags.AllUriInfoSet | System.Uri.Flags.HostUnicodeNormalized | System.Uri.Flags.RestUnicodeNormalized
            flags &= ~((ulong)0xC30); // Flags.PathNotCanonical|Flags.QueryNotCanonical|Flags.E_QueryNotCanonical|Flags.E_PathNotCanonical
            flagsFieldInfo.SetValue(uri, flags);
        }

    }
}
