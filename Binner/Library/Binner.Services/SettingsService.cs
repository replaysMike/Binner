using Binner.Common;
using Binner.Data;
using Binner.Global.Common;
using Binner.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Binner.Services
{
    public class SettingsService : ISettingsService
    {
        private const string BackupFilenameExtension = ".bak";

        private readonly ILogger<SettingsService>? _logger;
        private readonly IStorageProvider? _storageProvider;
        private readonly IDbContextFactory<BinnerContext>? _contextFactory;
        private readonly IRequestContextAccessor? _requestContext;

        public SettingsService() { }

        public SettingsService(ILogger<SettingsService> logger, IStorageProvider storageProvider, IDbContextFactory<BinnerContext> contextFactory, IRequestContextAccessor requestContextAccessor)
        {
            _logger = logger;
            _storageProvider = storageProvider;
            _contextFactory = contextFactory;
            _requestContext = requestContextAccessor;
        }

        public async Task SaveSettingsAsAsync<T>(T instance, string sectionName, string filename, bool createBackup)
        {
            if (createBackup)
            {
                File.Copy(filename, $"{filename}_{DateTime.Now.Ticks}{BackupFilenameExtension}", true);
            }

            // serialize settings to disk
            var serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
            };
            var serializer = new JsonSerializer();

            JObject? json;
            using (var textReader = new StreamReader(filename))
            {
                using var reader = new JsonTextReader(textReader);
                json = (JObject?)serializer.Deserialize(reader);
                textReader.Close();
            }

            if (json != null && json.ContainsKey(sectionName) && instance != null)
                json[sectionName] = JToken.FromObject(instance);
            else
                throw new BinnerConfigurationException($"There is no section named '{sectionName}' in the configuration file '{filename}'!");

            var jsonOutput = JsonConvert.SerializeObject(json, serializerSettings);
            using var file = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.Read);
            var buffer = Encoding.Default.GetBytes(jsonOutput);
            file.Write(buffer, 0, buffer.Length);
            file.Close();
        }

        public async Task<ICollection<CustomField>> GetCustomFieldsAsync()
        {
            if (_storageProvider == null) throw new ArgumentNullException("StorageProvider not set!");
            if (_requestContext == null) throw new ArgumentNullException("RequestContextAccessor not set!");

            return await _storageProvider.GetCustomFieldsAsync(_requestContext.GetUserContext());
        }

        public async Task<ICollection<CustomField>> SaveCustomFieldsAsync(ICollection<CustomField> customFields)
        {
            if (_storageProvider == null) throw new ArgumentNullException("StorageProvider not set!");
            if (_requestContext == null) throw new ArgumentNullException("RequestContextAccessor not set!");

            return await _storageProvider.SaveCustomFieldsAsync(customFields, _requestContext.GetUserContext());
        }

        public async Task<bool> PingDatabaseAsync()
        {
            if (_storageProvider == null) return false;
            return await _storageProvider.PingDatabaseAsync();
        }
    }
}
