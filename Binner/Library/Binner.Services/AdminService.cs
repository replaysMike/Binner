using AutoMapper;
using Binner.Common;
using Binner.Common.IO;
using Binner.Data;
using Binner.Global.Common;
using Binner.Model;
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
            var integrationConfiguration = _userConfigurationService.GetCachedOrganizationIntegrationConfiguration();
            model.License = userContext?.SubscriptionLevel.ToString();
            model.Language = localeConfiguration.Language.ToString();
            model.Currency = localeConfiguration.Currency.ToString();
            if (!string.IsNullOrEmpty(entryAssembly.Location))
                model.InstallPath = Path.GetDirectoryName(entryAssembly.Location);
            var fileTarget = (FileTarget) LogManager.Configuration.FindTargetByName("file");
            model.LogPath = Path.GetDirectoryName(fileTarget.FileName.ToString().Replace("'", ""));

            var enabledIntegrations = new List<string>();
            if (integrationConfiguration.SwarmEnabled)
                enabledIntegrations.Add("Swarm");
            if (integrationConfiguration.DigiKeyEnabled)
                enabledIntegrations.Add("DigiKey");
            if (integrationConfiguration.MouserEnabled)
                enabledIntegrations.Add("Mouser");
            if (integrationConfiguration.ArrowEnabled)
                enabledIntegrations.Add("Arrow");
            if (integrationConfiguration.NexarEnabled)
                enabledIntegrations.Add("Octopart/Nexar");
            //if (integrationConfiguration.AliExpress.Enabled)
            //    enabledIntegrations.Add("AliExpress");
            if (integrationConfiguration.TmeEnabled)
                enabledIntegrations.Add("TME");
            if (integrationConfiguration.Element14Enabled)
                enabledIntegrations.Add("Element14");
            model.EnabledIntegrations = string.Join(", ", enabledIntegrations);

            return model;
        }

        public async Task<PaginatedResponse<SystemLogsResponse>> GetSystemLogsAsync(PaginatedRequest request)
        {
            var userContext = _requestContext.GetUserContext();
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            
            var lines = new List<SystemLogsResponse>();
            
            string? filename = string.Empty;
            switch(request.By?.ToLower())
            {
                case "binner":
                    filename = LogManager.Configuration?.FindTargetByName<NLog.Targets.FileTarget>("file")?.FileName.ToString();
                    break;
                case "microsoft":
                    filename = LogManager.Configuration?.FindTargetByName<NLog.Targets.FileTarget>("microsoftfile")?.FileName.ToString();
                    break;
                case "missinglocalekeys":
                    filename = LogManager.Configuration?.FindTargetByName<NLog.Targets.FileTarget>("locale")?.FileName.ToString();
                    break;
                case "internal":
                    filename = Path.Combine(Path.GetDirectoryName(LogManager.Configuration?.FindTargetByName<NLog.Targets.FileTarget>("file")?.FileName.ToString()) ?? string.Empty, "Binner-internal.log");
                    break;
                default:
                    return new PaginatedResponse<SystemLogsResponse>(0, request.Results, request.Page, new List<SystemLogsResponse>());
            }

            if (string.IsNullOrEmpty(filename) || !File.Exists(filename)) return new PaginatedResponse<SystemLogsResponse>(0, request.Results, request.Page, new List<SystemLogsResponse>());

            var logReader = new LogReader(filename);
            var lineNum = 1;
            var currentPage = 1;
            foreach(var line in logReader)
            {
                currentPage = (int)Math.Ceiling((double)lineNum / request.Results);
                if (currentPage == request.Page) 
                    lines.Add(new SystemLogsResponse(line));
                lineNum++;
            }
            
            return new PaginatedResponse<SystemLogsResponse>(-1, request.Results, request.Page, lines);
        }
    }
}
