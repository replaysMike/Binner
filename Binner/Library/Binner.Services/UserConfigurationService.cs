using AutoMapper;
using Binner.Common;
using Binner.Common.Cache;
using Binner.Common.Extensions;
using Binner.Common.Integrations;
using Binner.Data;
using Binner.Global.Common;
using Binner.Model.Configuration;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Requests;
using Binner.Model.Responses;
using Binner.Services.Integrations;
using Microsoft.EntityFrameworkCore;
using DataModel = Binner.Data.Model;

namespace Binner.Services
{
    public class UserConfigurationService : IUserConfigurationService
    {
        protected readonly WebHostServiceConfiguration _configuration;
        protected readonly IDbContextFactory<BinnerContext> _contextFactory;
        protected readonly IMapper _mapper;
        protected readonly IRequestContextAccessor _requestContext;
        protected readonly ICredentialService _credentialService;
        protected readonly IIntegrationCredentialsCacheProvider _credentialProvider;
        protected readonly IUserConfigurationCacheProvider _userConfigCache;
        protected readonly IOrganizationConfigurationCacheProvider _organizationConfigCache;

        public UserConfigurationService(WebHostServiceConfiguration configuration, IDbContextFactory<BinnerContext> contextFactory, IMapper mapper, IRequestContextAccessor requestContextAccessor, ICredentialService credentialService, IIntegrationCredentialsCacheProvider credentialProvider, IUserConfigurationCacheProvider configCache, IOrganizationConfigurationCacheProvider organizationConfigCache)
        {
            _configuration = configuration;
                        _contextFactory = contextFactory;
            _mapper = mapper;
            _requestContext = requestContextAccessor;
            _credentialService = credentialService;
            _credentialProvider = credentialProvider;
            _userConfigCache = configCache;
            _organizationConfigCache = organizationConfigCache;
        }

        public virtual async Task<TestApiResponse> ForgetCachedCredentialsAsync(ForgetCachedCredentialsRequest request)
        {
            var user = _requestContext.GetUserContext();
            switch (request.Name.ToLower())
            {
                case "digikey":
                    await _credentialService.RemoveOAuthCredentialAsync(nameof(DigikeyApi));
                    _credentialProvider.Cache.Clear(new ApiCredentialKey { UserId = user?.UserId ?? 0 });
                    return new TestApiResponse(nameof(Integrations.DigikeyApi), true);
            }

            return new TestApiResponse(request.Name, $"Unknown api name!");
        }

        protected static string WrapUrl(string url)
        {
            if (!url.Contains("http://") && !url.Contains("https://"))
                url = $"https://{url}";
            return url;
        }

        public virtual async Task<OrganizationIntegrationConfiguration> CreateOrUpdateOrganizationIntegrationConfigurationAsync(OrganizationIntegrationConfiguration integrationConfiguration, int? organizationId = null)
        {
            var oid = organizationId ?? _requestContext.GetUserContext()?.OrganizationId;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.OrganizationIntegrationConfigurations
                .WhereIf(oid != null, x => x.OrganizationId == oid)
                .OrderByDescending(x => x.OrganizationId).ThenByDescending(x => x.DateCreatedUtc) // a config of OrganizationId=NULL is shared across all organizations, unless a org config is specified
                .FirstOrDefaultAsync();
            if (entity == null)
            {
                entity = _mapper.Map<DataModel.OrganizationIntegrationConfiguration>(integrationConfiguration);
                entity.DateCreatedUtc = DateTime.UtcNow;
                context.OrganizationIntegrationConfigurations.Add(entity);
            }
            else
                entity = _mapper.Map(integrationConfiguration, entity);
            entity.OrganizationId = oid;
            entity.DateModifiedUtc = DateTime.UtcNow;

            await context.SaveChangesAsync();

            // reset the integrations api cache for this user
            _credentialProvider.Cache.Clear(ApiCredentialKey.Create(_requestContext.GetUserContext()?.UserId ?? 0));
            // reset the config cache for this user
            _organizationConfigCache.Cache.Clear(oid ?? 0);

            return _mapper.Map<OrganizationIntegrationConfiguration>(integrationConfiguration);
        }

        public virtual async Task<UserPrinterConfiguration> CreateOrUpdatePrinterConfigurationAsync(UserPrinterConfiguration printerConfiguration, int? userId = null, int? organizationId = null)
        {
            var uid = userId ?? _requestContext.GetUserContext()?.UserId;
            var oid = organizationId ?? _requestContext.GetUserContext()?.OrganizationId;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.UserPrinterConfigurations
                .Include(x => x.UserPrinterTemplateConfigurations)
                .WhereIf(uid != null, x => x.UserId == uid)
                .OrderByDescending(x => x.UserId).ThenByDescending(x => x.DateCreatedUtc) // a config of userId=NULL is shared across all users, unless a user config is specified
                .FirstOrDefaultAsync();
            if (entity == null)
            {
                entity = _mapper.Map<DataModel.UserPrinterConfiguration>(printerConfiguration);
                entity.DateCreatedUtc = DateTime.UtcNow;
                context.UserPrinterConfigurations.Add(entity);
            }
            else
            {
                entity = _mapper.Map(printerConfiguration, entity);
                // delete all associated template configurations
                context.UserPrinterTemplateConfigurations.RemoveRange(entity.UserPrinterTemplateConfigurations);
            }

            entity.UserId = uid;
            entity.OrganizationId = oid;
            entity.DateModifiedUtc = DateTime.UtcNow;
            // add all associated template configurations
            foreach (var templateConfig in printerConfiguration.UserPrinterTemplateConfigurations)
            {
                var templateEntity = _mapper.Map<DataModel.UserPrinterTemplateConfiguration>(templateConfig);
                templateEntity.UserPrinterConfiguration = entity;
                templateEntity.UserId = uid;
                templateEntity.OrganizationId = oid;
                templateEntity.DateCreatedUtc = DateTime.UtcNow;
                context.UserPrinterTemplateConfigurations.Add(templateEntity);
            }

            await context.SaveChangesAsync();

            // reset the config cache for this user
            _userConfigCache.Cache.Clear(uid ?? 0);

            return _mapper.Map<UserPrinterConfiguration>(printerConfiguration);
        }

        public virtual async Task<UserConfiguration> CreateOrUpdateUserConfigurationAsync(UserConfiguration userConfiguration, int? userId = null, int? organizationId = null)
        {
            var uid = userId ?? _requestContext.GetUserContext()?.UserId;
            var oid = organizationId ?? _requestContext.GetUserContext()?.OrganizationId;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.UserConfigurations
                .WhereIf(uid != null, x => x.UserId == uid)
                .OrderByDescending(x => x.UserId).ThenByDescending(x => x.DateCreatedUtc) // a config of userId=NULL is shared across all users, unless a user config is specified
                .FirstOrDefaultAsync();
            if (entity == null)
            {
                entity = _mapper.Map<DataModel.UserConfiguration>(userConfiguration);
                entity.DateCreatedUtc = DateTime.UtcNow;
                context.UserConfigurations.Add(entity);
            }
            else
                entity = _mapper.Map(userConfiguration, entity);
            entity.UserId = uid;
            entity.OrganizationId = oid;
            entity.DateModifiedUtc = DateTime.UtcNow;

            await context.SaveChangesAsync();

            // reset the config cache for this user
            _userConfigCache.Cache.Clear(uid ?? 0);

            return _mapper.Map<UserConfiguration>(userConfiguration);
        }

        public async Task<OrganizationConfiguration> CreateOrUpdateOrganizationConfigurationAsync(OrganizationConfiguration organizationConfiguration, int? organizationId = null)
        {
            var oid = organizationId ?? _requestContext.GetUserContext()?.OrganizationId;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.OrganizationConfigurations
                .WhereIf(oid != null, x => x.OrganizationId == oid)
                .OrderByDescending(x => x.OrganizationId).ThenByDescending(x => x.DateCreatedUtc) // a config of OrganizationId=NULL is shared across all organizations, unless a org config is specified
                .FirstOrDefaultAsync();
            if (entity == null)
            {
                entity = _mapper.Map<DataModel.OrganizationConfiguration>(organizationConfiguration);
                entity.DateCreatedUtc = DateTime.UtcNow;
                context.OrganizationConfigurations.Add(entity);
            }
            else
                entity = _mapper.Map(organizationConfiguration, entity);
            entity.OrganizationId = oid;
            entity.DateModifiedUtc = DateTime.UtcNow;

            await context.SaveChangesAsync();

            // reset the config cache for this user
            _organizationConfigCache.Cache.Clear(oid ?? 0);

            return _mapper.Map<OrganizationConfiguration>(organizationConfiguration);
        }

        public virtual void ClearCachedUserConfigurations(int? userId = null)
        {
            var uid = userId ?? _requestContext.GetUserContext()?.UserId;
            // reset the config cache for this user
            _userConfigCache.Cache.Clear(uid ?? 0);
        }

        public virtual void ClearCachedOrganizationConfigurations(int? organizationId = null)
        {
            var oid = organizationId ?? _requestContext.GetUserContext()?.OrganizationId;
            // reset the config cache for this organization
            _organizationConfigCache.Cache.Clear(oid ?? 0);
            _organizationConfigCache.Cache.Clear(0);
        }

        public virtual async Task<OrganizationIntegrationConfiguration> GetOrganizationIntegrationConfigurationAsync(int? organizationId = null)
        {
            var oid = organizationId ?? _requestContext.GetUserContext()?.OrganizationId;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.OrganizationIntegrationConfigurations
                .Where(x => x.OrganizationId == oid || x.Organization == null)
                .OrderByDescending(x => x.OrganizationId).ThenByDescending(x => x.DateCreatedUtc) // a config of organizationId=NULL is shared across all organizations, unless a user config is specified
                .FirstOrDefaultAsync();
            if (entity == null)
                return GetDefaultOrganizationIntegrationConfiguration();

            return _mapper.Map<OrganizationIntegrationConfiguration>(entity);
        }

        public virtual async Task<UserPrinterConfiguration> GetPrinterConfigurationAsync(int? userId = null)
        {
            var uid = userId ?? _requestContext.GetUserContext()?.UserId;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.UserPrinterConfigurations
                .Include(x => x.UserPrinterTemplateConfigurations)
                .Where(x => x.UserId == uid || x.UserId == null)
                .OrderByDescending(x => x.UserId).ThenByDescending(x => x.DateCreatedUtc) // a config of userId=NULL is shared across all users, unless a user config is specified
                .FirstOrDefaultAsync();
            if (entity == null)
                return GetDefaultPrinterConfiguration();
            return _mapper.Map<UserPrinterConfiguration>(entity);
        }

        public virtual async Task<UserConfiguration> GetUserConfigurationAsync(int? userId = null)
        {
            var uid = userId ?? _requestContext.GetUserContext()?.UserId;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.UserConfigurations
                .Where(x => x.UserId == uid || x.UserId == null)
                .OrderByDescending(x => x.UserId).ThenByDescending(x => x.DateCreatedUtc) // a config of userId=NULL is shared across all users, unless a user config is specified
                .FirstOrDefaultAsync();
            if (entity == null)
                return GetDefaultUserConfiguration();

            return _mapper.Map<UserConfiguration>(entity);
        }

        public virtual async Task<OrganizationConfiguration> GetOrganizationConfigurationAsync(int? organizationId = null)
        {
            var oid = organizationId ?? _requestContext.GetUserContext()?.OrganizationId;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.OrganizationConfigurations
                .Where(x => x.OrganizationId == oid || x.OrganizationId == null)
                .OrderByDescending(x => x.OrganizationId).ThenByDescending(x => x.DateCreatedUtc) // a config of organizationId=NULL is shared across all organizations, unless a user config is specified
                .FirstOrDefaultAsync();
            if (entity == null)
                return GetDefaultOrganizationConfiguration();

            return _mapper.Map<OrganizationConfiguration>(entity);
        }

        public virtual OrganizationConfiguration GetCachedOrganizationConfiguration(int? organizationId = null)
        {
            var oid = organizationId ?? _requestContext.GetUserContext()?.OrganizationId;
            return AsyncHelper.RunSync(async () => await _organizationConfigCache.Cache.GetOrAddConfigAsync(oid ?? 0, async () => await GetOrganizationConfigurationAsync(oid)));
        }

        public virtual UserConfiguration GetCachedUserConfiguration(int? userId = null)
        {
            var uid = userId ?? _requestContext.GetUserContext()?.UserId;
            return AsyncHelper.RunSync(async () => await _userConfigCache.Cache.GetOrAddConfigAsync(uid ?? 0, async () => await GetUserConfigurationAsync(uid)));
        }

        public virtual OrganizationIntegrationConfiguration GetCachedOrganizationIntegrationConfiguration(int? organizationId = null)
        {
            var oid = organizationId ?? _requestContext.GetUserContext()?.OrganizationId;
            return AsyncHelper.RunSync(async () => await _organizationConfigCache.Cache.GetOrAddConfigAsync(oid ?? 0, async () => await GetOrganizationIntegrationConfigurationAsync(oid)));
        }

        public virtual UserPrinterConfiguration GetCachedPrinterConfiguration(int? userId = null)
        {
            var uid = userId ?? _requestContext.GetUserContext()?.UserId;
            return AsyncHelper.RunSync(async () => await _userConfigCache.Cache.GetOrAddConfigAsync(uid ?? 0, async () => await GetPrinterConfigurationAsync(uid)));
        }

        protected virtual OrganizationConfiguration GetDefaultOrganizationConfiguration()
        {
            return new OrganizationConfiguration
            {
                LicenseKey = string.Empty
            };
        }

        protected virtual OrganizationIntegrationConfiguration GetDefaultOrganizationIntegrationConfiguration()
        {
            return new OrganizationIntegrationConfiguration
            {
                // accept model defaults
                // override default public url, as the port number is different
                DigiKeyOAuthPostbackUrl = $"{_configuration.PublicUrl}/Authorization/Authorize"
            };
        }

        protected virtual UserPrinterConfiguration GetDefaultPrinterConfiguration()
        {
            return new UserPrinterConfiguration
            {
                PrinterName = "DYMO LabelWriter 450 Twin Turbo",
                PartLabelName = "30346",
                PartLabelSource = Model.IO.Printing.LabelSource.Auto,
                UserPrinterTemplateConfigurations = new List<UserPrinterTemplateConfiguration>
                {
                    { 
                        // line 1
                        new UserPrinterTemplateConfiguration() {
                            Label = 1,
                            Content = "{partNumber}",
                            FontName = "Segoe UI",
                            FontSize = 16,
                            AutoSize = true,
                            UpperCase = true
                        }
                    },
                    { 
                        // line 2
                        new UserPrinterTemplateConfiguration() {
                            Label = 1,
                            Content = "{description}",
                            FontName = "Roboto Mono",
                            FontSize = 6
                        }
                    },
                    { 
                        // line 3
                        new UserPrinterTemplateConfiguration() {
                            Label = 1,
                            Content = "{description}",
                            FontName = "Roboto Mono",
                            FontSize = 6
                        }
                    },
                    { 
                        // line 4
                        new UserPrinterTemplateConfiguration() {
                            Label = 1,
                            Content = "{partNumber}",
                            Barcode = true
                        }
                    },
                    { 
                        // Identifier 1
                        new UserPrinterTemplateConfiguration() {
                            Label = 1,
                            Content = "{binNumber}",
                            FontName = "Segoe UI",
                            FontSize = 10,
                            Color = "ee0000",
                            Rotate = 90,
                            Position = Model.IO.Printing.LabelPosition.Left,
                            UpperCase = true,
                            MarginTop = 25
                        }
                    },
                    { 
                        // Identifier 2
                        new UserPrinterTemplateConfiguration() {
                            Label = 1,
                            Content = "{binNumber2}",
                            FontName = "Segoe UI",
                            FontSize = 10,
                            Color = "ee0000",
                            Rotate = 90,
                            Position = Model.IO.Printing.LabelPosition.Right,
                            UpperCase = true,
                            MarginTop = 25,
                            MarginLeft = 20
                        }
                    }
                }
            };
        }

        protected virtual UserConfiguration GetDefaultUserConfiguration()
        {
            return new UserConfiguration
            {
                Currency = Model.Currencies.USD,
                Language = Model.Languages.En,
                BarcodeEnabled = true,
                BarcodeBufferTime = 80,
                BarcodeIsDebug = false,
                BarcodeMaxKeystrokeThresholdMs = 300,
                BarcodeProfile = BarcodeProfiles.Default,
                BarcodePrefix2D = @"[)>",
                EnableAutoPartSearch = true,
                EnableDarkMode = false,
            };
        }
    }
}
