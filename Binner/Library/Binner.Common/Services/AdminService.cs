using AutoMapper;
using Binner.Common.IO;
using Binner.Data;
using Binner.Global.Common;
using Binner.Model.Configuration;
using Binner.Model.Responses;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public class AdminService : IAdminService
    {
        private readonly IDbContextFactory<BinnerContext> _contextFactory;
        private readonly IMapper _mapper;
        private readonly RequestContextAccessor _requestContext;
        private readonly WebHostServiceConfiguration _configuration;
        private readonly StorageProviderConfiguration _storageProviderConfiguration;
        private readonly IVersionManagementService _versionManagementService;

        public AdminService(IDbContextFactory<BinnerContext> contextFactory, IMapper mapper, RequestContextAccessor requestContext, WebHostServiceConfiguration configuration, StorageProviderConfiguration storageProviderConfiguration, IVersionManagementService versionManagementService)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
            _requestContext = requestContext;
            _configuration = configuration;
            _storageProviderConfiguration = storageProviderConfiguration;
            _versionManagementService = versionManagementService;
        }

        public async Task<SystemInfoResponse> GetSystemInfoAsync()
        {
            var model = new SystemInfoResponse();
            var userContext = _requestContext.GetUserContext();
            var context = await _contextFactory.CreateDbContextAsync();
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
            if (!string.IsNullOrEmpty(_storageProviderConfiguration.UserUploadedFilesPath) && Path.Exists(_storageProviderConfiguration.UserUploadedFilesPath))
            {
                var files = Directory.GetFiles(_storageProviderConfiguration.UserUploadedFilesPath, "*", SearchOption.AllDirectories);
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
            model.UserFilesLocation = _storageProviderConfiguration.UserUploadedFilesPath;
            switch (model.StorageProvider.ToLower())
            {
                case "binner":
                    model.StorageProviderSettings = _storageProviderConfiguration.ProviderConfiguration["Filename"];
                    break;
                default:
                    model.StorageProviderSettings = _storageProviderConfiguration.ProviderConfiguration["ConnectionString"];
                    var builder = new DbConnectionStringBuilder();
                    builder.ConnectionString = model.StorageProviderSettings;
                    if (builder.ContainsKey("Password"))
                    {
                        builder["Password"] = "********";
                        model.StorageProviderSettings = builder.ToString();
                    }
                    break;
            }
            model.Port = _configuration.Port.ToString();
            model.IP = _configuration.IP.ToString();
            model.License = _configuration.Licensing?.LicenseKey;
            model.Language = _configuration.Locale?.Language.ToString();
            model.Currency = _configuration.Locale?.Currency.ToString();
            if (!string.IsNullOrEmpty(entryAssembly.Location))
                model.InstallPath = Path.GetDirectoryName(entryAssembly.Location);
            var fileTarget = (FileTarget) LogManager.Configuration.FindTargetByName("file");
            model.LogPath = Path.GetDirectoryName(fileTarget.FileName.ToString().Replace("'", ""));

            var enabledIntegrations = new List<string>();
            if (_configuration.Integrations.Swarm.Enabled)
                enabledIntegrations.Add("Swarm");
            if (_configuration.Integrations.Digikey.Enabled)
                enabledIntegrations.Add("DigiKey");
            if (_configuration.Integrations.Mouser.Enabled)
                enabledIntegrations.Add("Mouser");
            if (_configuration.Integrations.Arrow.Enabled)
                enabledIntegrations.Add("Arrow");
            if (_configuration.Integrations.Nexar.Enabled)
                enabledIntegrations.Add("Octopart/Nexar");
            if (_configuration.Integrations.AliExpress.Enabled)
                enabledIntegrations.Add("AliExpress");
            model.EnabledIntegrations = string.Join(", ", enabledIntegrations);

            return model;
        }
    }
}
