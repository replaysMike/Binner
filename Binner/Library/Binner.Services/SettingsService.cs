using AutoMapper;
using Binner.Common;
using Binner.Data;
using Binner.Global.Common;
using Binner.Model;
using Binner.Model.Configuration;
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

        public async Task SaveSettingsAsAsync(WebHostServiceConfiguration config, string sectionName, string filename, bool createBackup, string? backupFilename = null)
        {
            if (createBackup)
            {
                var backupFile = $"{filename}_{DateTime.Now.Ticks}{BackupFilenameExtension}";
                if (!string.IsNullOrEmpty(backupFilename))
                {
                    backupFile = $"{backupFilename}{BackupFilenameExtension}";
                }
                File.Copy(filename, backupFile, true);
                _logger?.LogInformation($"Backed up '{filename}' to '{backupFile}'");
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

            // use the writable version of the configuration object so we don't serialize properties no longer stored in the file
            if (json != null && json.ContainsKey(sectionName) && config != null)
                json[sectionName] = JToken.FromObject(config);
            else
                throw new BinnerConfigurationException($"There is no section named '{sectionName}' in the configuration file '{filename}'!");

            if (json.ContainsKey(nameof(WebHostServiceConfiguration)))
            {
                // remove all sections that have been migrated to the database. These should not be present in the config file anymore.
                var sectionsToRemove = new List<string> { "Locale", "Barcode", "Integrations", "PrinterConfiguration", "Licensing", "MaxCacheItems", "CacheSlidingExpirationMinutes", "CacheAbsoluteExpirationMinutes" };
                foreach (var section in sectionsToRemove)
                {
                    json[nameof(WebHostServiceConfiguration)]?.SelectToken(section)?.Parent?.Remove();
                }
            }

            var jsonOutput = JsonConvert.SerializeObject(json, serializerSettings);
            using var file = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.Read);
            var buffer = Encoding.Default.GetBytes(jsonOutput);
            await file.WriteAsync(buffer, 0, buffer.Length);
            file.Close();
            _logger?.LogInformation($"Wrote new configuration '{filename}' ({buffer.Length} bytes).");
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
