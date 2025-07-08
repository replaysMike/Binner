using AutoMapper;
using Binner.Common;
using Binner.Common.IO;
using Binner.Data;
using Binner.Global.Common;
using Binner.Model.Configuration;
using Binner.Model.Responses;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Targets;
using System.Data.Common;
using System.Reflection;

namespace Binner.Services
{
    public class AdminService : IAdminService
    {
        private readonly IDbContextFactory<BinnerContext> _contextFactory;
        private readonly IMapper _mapper;
        private readonly IRequestContextAccessor _requestContext;
        private readonly WebHostServiceConfiguration _configuration;
        private readonly StorageProviderConfiguration _storageProviderConfiguration;
        private readonly IVersionManagementService _versionManagementService;
        private readonly IUserConfigurationService _userConfigurationService;

        public AdminService(IDbContextFactory<BinnerContext> contextFactory, IMapper mapper, IRequestContextAccessor requestContext, WebHostServiceConfiguration configuration, StorageProviderConfiguration storageProviderConfiguration, IVersionManagementService versionManagementService, IUserConfigurationService userConfigurationService)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
            _requestContext = requestContext;
            _configuration = configuration;
            _storageProviderConfiguration = storageProviderConfiguration;
            _versionManagementService = versionManagementService;
            _userConfigurationService = userConfigurationService;
        }

        public virtual async Task<SystemInfoResponse> GetSystemInfoAsync()
        {
            var model = new SystemInfoResponse();
            var userContext = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            Assembly? entryAssembly = null;
            try
            {
                entryAssembly = Assembly.GetEntryAssembly();
            }
            catch (Exception)
            {
            }

            VersionManagementService.BinnerVersion latestVersion;
            try
            {
                latestVersion = await _versionManagementService.GetLatestVersionAsync();
            }
            catch (Exception ex)
            {
                latestVersion = new VersionManagementService.BinnerVersion("Unable to fetch", ex.Message, "", false);
            }

            Version version;
            try
            {
                version = entryAssembly?.GetName()?.Version ?? new Version();
            }
            catch (Exception)
            {
                version = new Version(1, 0, 0, 0);
            }

            model.LatestVersion = latestVersion.Version;
            model.Version = version.ToString(3);

            model.TotalUsers = await context.Users.CountAsync();
            model.TotalParts = await context.Parts.CountAsync();
            model.TotalPartTypes = await context.PartTypes.CountAsync();
            var userFilesPath = SystemPaths.GetUserFilesPath(_storageProviderConfiguration);
            if (!string.IsNullOrEmpty(userFilesPath) && Path.Exists(userFilesPath))
            {
                var files = Directory.GetFiles(userFilesPath, "*", SearchOption.AllDirectories);
                model.TotalUserFiles = files.Length;
                var fileSize = 0L;
                try
                {
                    foreach (var file in files)
                        fileSize += new FileInfo(file).Length;
                    model.UserFilesSize = FileUtils.GetFriendlyFileSize(fileSize);
                }
                catch (Exception)
                {
                }
            }


            model.StorageProvider = _storageProviderConfiguration.Provider;
            model.UserFilesLocation = SystemPaths.GetUserFilesPath(_storageProviderConfiguration);
            switch (model.StorageProvider.ToLower())
            {
                case "binner":
                    model.StorageProviderSettings = _storageProviderConfiguration.ProviderConfiguration["Filename"];
                    break;
                default:
                    if (_storageProviderConfiguration.ProviderConfiguration.ContainsKey("ConnectionString") && !string.IsNullOrEmpty(_storageProviderConfiguration.ProviderConfiguration["ConnectionString"]))
                    {
                        model.StorageProviderSettings = _storageProviderConfiguration.ProviderConfiguration["ConnectionString"];
                        var builder = new DbConnectionStringBuilder();
                        builder.ConnectionString = model.StorageProviderSettings;
                        if (builder.ContainsKey("Password"))
                        {
                            builder["Password"] = "********";
                            model.StorageProviderSettings = builder.ToString();
                        }
                    }
                    else
                    {
                        model.StorageProviderSettings = "";
                        var builder = new DbConnectionStringBuilder();
                        if (_storageProviderConfiguration.ProviderConfiguration.ContainsKey("Host") && !string.IsNullOrEmpty(_storageProviderConfiguration.ProviderConfiguration["Host"]))
                            builder["Host"] = _storageProviderConfiguration.ProviderConfiguration["Host"];
                        if (_storageProviderConfiguration.ProviderConfiguration.ContainsKey("Port") && !string.IsNullOrEmpty(_storageProviderConfiguration.ProviderConfiguration["Port"]))
                            builder["Port"] = _storageProviderConfiguration.ProviderConfiguration["Port"];
                        if (_storageProviderConfiguration.ProviderConfiguration.ContainsKey("Database") && !string.IsNullOrEmpty(_storageProviderConfiguration.ProviderConfiguration["Database"]))
                            builder["Database"] = _storageProviderConfiguration.ProviderConfiguration["Database"];
                        if (_storageProviderConfiguration.ProviderConfiguration.ContainsKey("Username") && !string.IsNullOrEmpty(_storageProviderConfiguration.ProviderConfiguration["Username"]))
                            builder["Username"] = _storageProviderConfiguration.ProviderConfiguration["Username"];
                        if (_storageProviderConfiguration.ProviderConfiguration.ContainsKey("Password") && !string.IsNullOrEmpty(_storageProviderConfiguration.ProviderConfiguration["Password"]))
                            builder["Password"] = "********";
                        model.StorageProviderSettings = builder.ToString();
                    }
                    break;
            }
            model.Port = _configuration.Port.ToString();
            model.IP = _configuration.IP.ToString();

            var localeConfiguration = _userConfigurationService.GetCachedUserConfiguration();
            var integrationConfiguration = _mapper.Map<IntegrationConfiguration>(_userConfigurationService.GetCachedOrganizationIntegrationConfiguration());
            model.License = _configuration.Licensing?.LicenseKey;
            model.Language = localeConfiguration.Language.ToString();
            model.Currency = localeConfiguration.Currency.ToString();
            if (!string.IsNullOrEmpty(entryAssembly.Location))
                model.InstallPath = Path.GetDirectoryName(entryAssembly.Location);
            var fileTarget = (FileTarget) LogManager.Configuration.FindTargetByName("file");
            model.LogPath = Path.GetDirectoryName(fileTarget.FileName.ToString().Replace("'", ""));

            var enabledIntegrations = new List<string>();
            if (integrationConfiguration.Swarm.Enabled)
                enabledIntegrations.Add("Swarm");
            if (integrationConfiguration.Digikey.Enabled)
                enabledIntegrations.Add("DigiKey");
            if (integrationConfiguration.Mouser.Enabled)
                enabledIntegrations.Add("Mouser");
            if (integrationConfiguration.Arrow.Enabled)
                enabledIntegrations.Add("Arrow");
            if (integrationConfiguration.Nexar.Enabled)
                enabledIntegrations.Add("Octopart/Nexar");
            if (integrationConfiguration.AliExpress.Enabled)
                enabledIntegrations.Add("AliExpress");
            if (integrationConfiguration.Tme.Enabled)
                enabledIntegrations.Add("TME");
            model.EnabledIntegrations = string.Join(", ", enabledIntegrations);

            return model;
        }
    }
}
