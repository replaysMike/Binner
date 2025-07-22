using Binner.Model;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Services;
using Binner.Web.Controllers;
using Microsoft.Extensions.Logging;
using NPOI.HPSF;
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
                if (!_configuration.IsMigrated)
                {
                    const int defaultOrganizationId = 1;
                    const int defaultUserId = 1;
                    var integrationConfiguration = _mapper.Map<OrganizationIntegrationConfiguration>(_configuration.Integrations ?? new IntegrationConfiguration());
                    var printerConfiguration = _mapper.Map<UserPrinterConfiguration>(_configuration.PrinterConfiguration ?? new PrinterConfiguration());
                    var userConfiguration = _mapper.Map<UserConfiguration>(_configuration.Locale ?? new LocaleConfiguration());
                    userConfiguration =  _mapper.Map<BarcodeConfiguration, UserConfiguration>(_configuration.Barcode ?? new BarcodeConfiguration(), userConfiguration);
                    
                    var organizationConfiguration = await _userConfigurationService.CreateOrUpdateOrganizationConfigurationAsync(new OrganizationConfiguration
                    {
                        LicenseKey = _configuration.Licensing.LicenseKey,
                    }, defaultOrganizationId);
                    integrationConfiguration = await _userConfigurationService.CreateOrUpdateOrganizationIntegrationConfigurationAsync(integrationConfiguration, defaultOrganizationId);
                    userConfiguration = await _userConfigurationService.CreateOrUpdateUserConfigurationAsync(userConfiguration, defaultUserId, defaultOrganizationId);
                    printerConfiguration = await _userConfigurationService.CreateOrUpdatePrinterConfigurationAsync(printerConfiguration, defaultUserId, defaultOrganizationId);
                    
                    // mark it as migrated so we don't run it again
                    _configuration.IsMigrated = true;
                    var backupFilename = $"{_appSettingsFilename}_{DateTime.Now.Ticks}.preMigrate";
                    await _settingsService.SaveSettingsAsAsync(_configuration, nameof(WebHostServiceConfiguration), _appSettingsFilename, true, backupFilename);
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
