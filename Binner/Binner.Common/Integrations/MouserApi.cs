using Binner.Common.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Binner.Common.Integrations
{
    public class MouserApi
    {
        public const string Path = "https://sandbox-api.digikey.com/Search/v3/Products";
        private readonly HttpClient _client;
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            // ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            Converters = new List<JsonConverter> { new StringEnumConverter() }
        };

        public MouserApi()
        {
            _client = new HttpClient();
        }

        public async Task<ICollection<object>> GetProductInformationAsync(string partNumber)
        {
            return null;
        }

        public async Task<ICollection<object>> BuildCartAsync()
        {
            return null;
        }

        public async Task<ICollection<object>> GetOrderAsync()
        {
            return null;
        }

        public async Task<ICollection<object>> CreateOrderAsync()
        {
            return null;
        }
    }        
}
