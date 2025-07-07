using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Services;
using Binner.Web.Controllers;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Binner.Web.Database
{
    public class ConfigFileMigrator
    {
        private static readonly string _appSettingsFilename = EnvironmentVarConstants.GetEnvOrDefault(EnvironmentVarConstants.Config, AppConstants.AppSettings);

        private readonly ILogger<ConfigFileMigrator> _logger;
        private readonly AutoMapper.IMapper _mapper;
        private readonly ISettingsService _settingsService;
        private readonly IUserConfigurationService _userConfigurationService;
        private readonly WebHostServiceConfiguration _configuration;

        public ConfigFileMigrator(ILogger<ConfigFileMigrator> logger, AutoMapper.IMapper mapper, ISettingsService settingsService, IUserConfigurationService userConfigurationService, WebHostServiceConfiguration configuration)
        {
            _mapper = mapper;
            _logger = logger;
            _settingsService = settingsService;
            _userConfigurationService = userConfigurationService;
            _configuration = configuration;
        }

        public async Task<bool> MigrateConfigFileToDatabaseAsync()
        {
            try
            {
                // [ ] load config file, look for sections we are going to migrate to the database
                // [ ] save to database
                // [ ] remove sections from config file
                // todo: how do we handle this per user?
                if (!_configuration.IsMigrated)
                {
                    var integrationConfiguration = _mapper.Map<UserIntegrationConfiguration>(_configuration.Integrations);
                    var printerConfiguration = _mapper.Map<UserPrinterConfiguration>(_configuration.PrinterConfiguration);
                    var localeConfiguration = _mapper.Map<UserLocaleConfiguration>(_configuration.Locale);
                    var barcodeConfiguration = _mapper.Map<UserBarcodeConfiguration>(_configuration.Barcode);
                    integrationConfiguration = await _userConfigurationService.CreateOrUpdateIntegrationConfigurationAsync(integrationConfiguration);
                    printerConfiguration = await _userConfigurationService.CreateOrUpdatePrinterConfigurationAsync(printerConfiguration);
                    localeConfiguration = await _userConfigurationService.CreateOrUpdateLocaleConfigurationAsync(localeConfiguration);
                    barcodeConfiguration = await _userConfigurationService.CreateOrUpdateBarcodeConfigurationAsync(barcodeConfiguration);
                    _configuration.IsMigrated = true;
                    await _settingsService.SaveSettingsAsAsync(_configuration, nameof(WebHostServiceConfiguration), _appSettingsFilename, true, true);
                    return true;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to migrate configuration file to database!");
            }
            return false;
        }
    }
}
