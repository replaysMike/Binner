using AutoMapper;
using Binner.Common;
using Binner.Data;
using Binner.Global.Common;
using Binner.LicensedProvider.Services;
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
        private readonly ISettingsHandlerService _settingsHandlerService;

        public SettingsService() { }

        public SettingsService(ILogger<SettingsService> logger, IStorageProvider storageProvider, IDbContextFactory<BinnerContext> contextFactory, IRequestContextAccessor requestContextAccessor, ISettingsHandlerService settingsHandlerService)
        {
            _logger = logger;
            _storageProvider = storageProvider;
            _contextFactory = contextFactory;
            _requestContext = requestContextAccessor;
            _settingsHandlerService = settingsHandlerService;
        }

        public async Task SaveSettingsAsAsync(WebHostServiceConfiguration config, string sectionName, string filename, bool createBackup, string? backupFilename = null)
        {
            await _settingsHandlerService.SaveSettingsAsAsync(config, sectionName, filename, createBackup, backupFilename);
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
