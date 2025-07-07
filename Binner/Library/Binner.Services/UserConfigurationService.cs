using AutoMapper;
using Binner.Common.Cache;
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
        protected readonly IIntegrationApiFactory _integrationApiFactory;
        protected readonly IDbContextFactory<BinnerContext> _contextFactory;
        protected readonly IMapper _mapper;
        protected readonly IRequestContextAccessor _requestContext;
        protected readonly ICredentialService _credentialService;
        protected readonly IIntegrationCredentialsCacheProvider _credentialProvider;
        protected readonly IUserConfigurationCacheProvider _configCache;

        public UserConfigurationService(WebHostServiceConfiguration configuration, IIntegrationApiFactory integrationApiFactory, IDbContextFactory<BinnerContext> contextFactory, IMapper mapper, IRequestContextAccessor requestContextAccessor, ICredentialService credentialService, IIntegrationCredentialsCacheProvider credentialProvider, IUserConfigurationCacheProvider configCache)
        {
            _configuration = configuration;
            _integrationApiFactory = integrationApiFactory;
            _contextFactory = contextFactory;
            _mapper = mapper;
            _requestContext = requestContextAccessor;
            _credentialService = credentialService;
            _credentialProvider = credentialProvider;
            _configCache = configCache;
        }

        public virtual async Task<TestApiResponse> TestApiAsync(TestApiRequest request)
        {
            var user = _requestContext.GetUserContext();

            var getCredentialsMethod = async () =>
            {
                var userId = user.UserId;
                // create a db context
                using var context = await _contextFactory.CreateDbContextAsync();
                var userIntegrationConfiguration = await context.UserIntegrationConfigurations
                    .Where(x => x.UserId.Equals(userId))
                    .FirstOrDefaultAsync()
                    ?? new Data.Model.UserIntegrationConfiguration();
                var comparisonType = StringComparison.InvariantCultureIgnoreCase;

                // build the credentials list
                var credentials = new List<ApiCredential>();

                // add user defined credentials
                var swarmConfiguration = new Dictionary<string, object?>
                {
                    { "Enabled", request.Configuration.Where(x => x.Key.Equals("Enabled", comparisonType) && x.Value != null).Select(x => bool.Parse(x.Value ?? "false")).FirstOrDefault() },
                    { "ApiKey", request.Configuration.Where(x => x.Key.Equals("ApiKey", comparisonType) && x.Value != null).Select(x =>x.Value).FirstOrDefault() ?? string.Empty },
                    { "ApiUrl", request.Configuration.Where(x => x.Key.Equals("ApiUrl", comparisonType) && x.Value != null).Select(x => WrapUrl(x.Value)).FirstOrDefault() ?? string.Empty },
                    { "Timeout", request.Configuration.Where(x => x.Key.Equals("Timeout", comparisonType) && x.Value != null).Select(x => TimeSpan.Parse(x.Value ?? "00:00:05")).FirstOrDefault() },
                };
                credentials.Add(new ApiCredential(userId, swarmConfiguration, nameof(SwarmApi)));

                var digikeyConfiguration = new Dictionary<string, object?>
                {
                    {"Enabled", request.Configuration.Where(x => x.Key.Equals("Enabled", comparisonType) && x.Value != null).Select(x => bool.Parse(x.Value ?? "false")).FirstOrDefault()},
                    { "Site", request.Configuration.Where(x => x.Key.Equals("Site", comparisonType) && x.Value != null).Select(x => int.Parse(x.Value ?? "0")).FirstOrDefault() },
                    { "ClientId", request.Configuration.Where(x => x.Key.Equals("ClientId", comparisonType) && x.Value != null).Select(x =>x.Value).FirstOrDefault() ?? string.Empty },
                    { "ClientSecret", request.Configuration.Where(x => x.Key.Equals("ClientSecret", comparisonType) && x.Value != null).Select(x =>x.Value).FirstOrDefault() ?? string.Empty },
                    { "oAuthPostbackUrl", request.Configuration.Where(x => x.Key.Equals("oAuthPostbackUrl", comparisonType) && x.Value != null).Select(x =>x.Value).FirstOrDefault() ?? string.Empty },
                    { "ApiUrl", request.Configuration.Where(x => x.Key.Equals("ApiUrl", comparisonType) && x.Value != null).Select(x =>  WrapUrl(x.Value)).FirstOrDefault() ?? string.Empty }
                };
                credentials.Add(new ApiCredential(userId, digikeyConfiguration, nameof(DigikeyApi)));

                var mouserConfiguration = new Dictionary<string, object?>
                {
                    { "Enabled", request.Configuration.Where(x => x.Key.Equals("Enabled", comparisonType) && x.Value != null).Select(x => bool.Parse(x.Value ?? "false")).FirstOrDefault()},
                    { "CartApiKey", request.Configuration.Where(x => x.Key.Equals("CartApiKey", comparisonType) && x.Value != null).Select(x =>x.Value).FirstOrDefault() ?? string.Empty},
                    { "OrderApiKey", request.Configuration.Where(x => x.Key.Equals("OrderApiKey", comparisonType) && x.Value != null).Select(x => x.Value).FirstOrDefault() ?? string.Empty},
                    { "SearchApiKey", request.Configuration.Where(x => x.Key.Equals("SearchApiKey", comparisonType) && x.Value != null).Select(x => x.Value).FirstOrDefault() ?? string.Empty },
                    { "ApiUrl", request.Configuration.Where(x => x.Key.Equals("ApiUrl", comparisonType) && x.Value != null).Select(x => WrapUrl(x.Value)).FirstOrDefault() ?? string.Empty },
                };
                credentials.Add(new ApiCredential(userId, mouserConfiguration, nameof(MouserApi)));

                var arrowConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", request.Configuration.Where(x => x.Key.Equals("Enabled", comparisonType) && x.Value != null).Select(x => bool.Parse(x.Value ?? "false")).FirstOrDefault() },
                    { "Username", request.Configuration.Where(x => x.Key.Equals("Username", comparisonType) && x.Value != null).Select(x => x.Value).FirstOrDefault() ?? string.Empty },
                    { "ApiKey", request.Configuration.Where(x => x.Key.Equals("ApiKey", comparisonType) && x.Value != null).Select(x =>x.Value).FirstOrDefault() ?? string.Empty },
                    { "ApiUrl", request.Configuration.Where(x => x.Key.Equals("ApiUrl", comparisonType) && x.Value != null).Select(x => WrapUrl(x.Value)).FirstOrDefault() ?? string.Empty },
                };
                credentials.Add(new ApiCredential(userId, arrowConfiguration, nameof(ArrowApi)));

                var octopartConfiguration = new Dictionary<string, object?>
                {
                    { "Enabled", request.Configuration.Where(x => x.Key.Equals("Enabled", comparisonType) && x.Value != null).Select(x => bool.Parse(x.Value ?? "false")).FirstOrDefault() },
                    { "ClientId", request.Configuration.Where(x => x.Key.Equals("ClientId", comparisonType) && x.Value != null).Select(x =>x.Value).FirstOrDefault() ?? string.Empty },
                    { "ClientSecret", request.Configuration.Where(x => x.Key.Equals("ClientSecret", comparisonType) && x.Value != null).Select(x => x.Value).FirstOrDefault() ?? string.Empty }
                };
                credentials.Add(new ApiCredential(userId, octopartConfiguration, nameof(NexarApi)));

                var tmeConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", request.Configuration.Where(x => x.Key.Equals("Enabled", comparisonType) && x.Value != null).Select(x => bool.Parse(x.Value ?? "false")).FirstOrDefault() },
                    { "Country", request.Configuration.Where(x => x.Key.Equals("Country", comparisonType) && x.Value != null).Select(x => x.Value).FirstOrDefault() ?? string.Empty },
                    { "ApplicationSecret", request.Configuration.Where(x => x.Key.Equals("ApplicationSecret", comparisonType) && x.Value != null).Select(x => x.Value).FirstOrDefault() ?? string.Empty },
                    { "ApiKey", request.Configuration.Where(x => x.Key.Equals("ApiKey", comparisonType) && x.Value != null).Select(x =>x.Value).FirstOrDefault() ?? string.Empty },
                    { "ApiUrl", request.Configuration.Where(x => x.Key.Equals("ApiUrl", comparisonType) && x.Value != null).Select(x => WrapUrl(x.Value)).FirstOrDefault() ?? string.Empty },
                };
                credentials.Add(new ApiCredential(user?.UserId ?? 0, tmeConfiguration, nameof(TmeApi)));

                return new ApiCredentialConfiguration(user?.UserId ?? 0, credentials);
            };

            switch (request.Name.ToLower())
            {
                case "swarm":
                    {
                        var api = await _integrationApiFactory.CreateAsync<Integrations.SwarmApi>(user?.UserId ?? 0, getCredentialsMethod, false);
                        if (!api.IsEnabled)
                            return new TestApiResponse(nameof(Integrations.SwarmApi), "Api is not enabled.");
                        try
                        {
                            var result = await api.SearchAsync("LM555", 1);
                            if (result.Errors.Any())
                                return new TestApiResponse(nameof(Integrations.SwarmApi), string.Join(". ", result.Errors));
                            return new TestApiResponse(nameof(Integrations.SwarmApi), true);
                        }
                        catch (Exception ex)
                        {
                            return new TestApiResponse(nameof(Integrations.SwarmApi), ex.GetBaseException().Message);
                        }
                    }
                case "digikey":
                    {
                        var api = await _integrationApiFactory.CreateAsync<Integrations.DigikeyApi>(user?.UserId ?? 0, getCredentialsMethod, false);
                        if (!api.IsEnabled)
                            return new TestApiResponse(nameof(Integrations.DigikeyApi), "Api is not enabled.");
                        try
                        {
                            var result = await api.SearchAsync("LM555", 1);
                            if (result.Errors.Any())
                            {
                                var errorResult = new TestApiResponse(nameof(Integrations.DigikeyApi), string.Join(". ", result.Errors));
                                if (result.RequiresAuthentication)
                                    errorResult.AuthorizationUrl = result.RedirectUrl;
                                return errorResult;
                            }
                            return new TestApiResponse(nameof(Integrations.DigikeyApi), true);
                        }
                        catch (Exception ex)
                        {
                            return new TestApiResponse(nameof(Integrations.DigikeyApi), ex.GetBaseException().Message);
                        }
                    }
                case "mouser":
                    {
                        var api = await _integrationApiFactory.CreateAsync<Integrations.MouserApi>(user?.UserId ?? 0, getCredentialsMethod, false);
                        if (!api.IsEnabled)
                            return new TestApiResponse(nameof(Integrations.MouserApi), "Api is not enabled.");
                        try
                        {
                            if (((MouserConfiguration)api.Configuration).IsConfigured)
                            {
                                var result = await api.SearchAsync("LM555", 1);
                                if (result.Errors.Any())
                                    return new TestApiResponse(nameof(Integrations.MouserApi), string.Join(". ", result.Errors));
                                return new TestApiResponse(nameof(Integrations.MouserApi), true);
                            }

                            if (((MouserConfiguration)api.Configuration).IsOrdersConfigured)
                            {
                                var result = await api.GetOrderAsync("1111111");
                                if (result.Errors.Any() && !result.Errors.First().EndsWith("Not Found"))
                                    return new TestApiResponse(nameof(Integrations.MouserApi), string.Join(". ", result.Errors));
                                return new TestApiResponse(nameof(Integrations.MouserApi), true);
                            }

                            return new TestApiResponse(nameof(Integrations.MouserApi), false);
                        }
                        catch (MouserErrorsException ex)
                        {
                            return new TestApiResponse(nameof(Integrations.MouserApi), string.Join(". ", ex.Errors.Select(x => x.Message)));
                        }
                        catch (Exception ex)
                        {
                            return new TestApiResponse(nameof(Integrations.MouserApi), ex.GetBaseException().Message);
                        }
                    }
                case "arrow":
                    {
                        var api = await _integrationApiFactory.CreateAsync<Integrations.ArrowApi>(user?.UserId ?? 0, getCredentialsMethod, false);
                        if (!api.IsEnabled)
                            return new TestApiResponse(nameof(Integrations.ArrowApi), "Api is not enabled.");
                        try
                        {
                            var result = await api.SearchAsync("LM555", 1);
                            if (result.Errors.Any())
                                return new TestApiResponse(nameof(Integrations.ArrowApi), string.Join(". ", result.Errors));
                            return new TestApiResponse(nameof(Integrations.ArrowApi), true);
                        }
                        catch (Exception ex)
                        {
                            return new TestApiResponse(nameof(Integrations.ArrowApi), ex.GetBaseException().Message);
                        }
                    }
                case "nexar":
                case "octopart":
                    {
                        var api = await _integrationApiFactory.CreateAsync<Integrations.NexarApi>(user?.UserId ?? 0, getCredentialsMethod, false);
                        if (!api.IsEnabled)
                            return new TestApiResponse(nameof(Integrations.NexarApi), "Api is not enabled.");
                        try
                        {
                            var result = await api.SearchAsync("LM555", 1);
                            if (result.Errors.Any())
                                return new TestApiResponse(nameof(Integrations.NexarApi), string.Join(". ", result.Errors));
                            return new TestApiResponse(nameof(Integrations.NexarApi), true);
                        }
                        catch (Exception ex)
                        {
                            return new TestApiResponse(nameof(Integrations.NexarApi), ex.GetBaseException().Message);
                        }
                    }
                case "tme":
                    {
                        var api = await _integrationApiFactory.CreateAsync<Integrations.TmeApi>(user?.UserId ?? 0, getCredentialsMethod, false);
                        if (!api.IsEnabled)
                            return new TestApiResponse(nameof(Integrations.TmeApi), "Api is not enabled.");
                        try
                        {
                            //var categories = await api.GetCategoriesAsync();
                            //var files = await api.GetProductFilesAsync(new List<string> { "LM358AD-ST" });
                            //var prices = await api.GetProductPricesAsync(new List<string> { "LM358AD-ST" });
                            var result = await api.SearchAsync("LM555", 1);
                            if (result.Errors.Any())
                                return new TestApiResponse(nameof(Integrations.TmeApi), string.Join(". ", result.Errors));
                            return new TestApiResponse(nameof(Integrations.TmeApi), true);
                        }
                        catch (Exception ex)
                        {
                            return new TestApiResponse(nameof(Integrations.TmeApi), ex.GetBaseException().Message);
                        }
                    }
            }

            return new TestApiResponse(request.Name, $"Unknown api name!");
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

        public virtual async Task<UserIntegrationConfiguration> CreateOrUpdateIntegrationConfigurationAsync(UserIntegrationConfiguration integrationConfiguration)
        {
            var user = _requestContext.GetUserContext() ?? throw new UserContextUnauthorizedException();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.UserIntegrationConfigurations
                .Where(x => x.UserId == user.UserId)
                .OrderByDescending(x => x.DateCreatedUtc) // should only ever be 1, but take the most recent one just in case.
                .FirstOrDefaultAsync();
            if (entity == null)
            {
                entity = _mapper.Map<DataModel.UserIntegrationConfiguration>(integrationConfiguration);
                entity.DateCreatedUtc = DateTime.UtcNow;
                context.UserIntegrationConfigurations.Add(entity);
            }
            else
                entity = _mapper.Map(integrationConfiguration, entity);
            entity.UserId = user.UserId;
            entity.OrganizationId = user.OrganizationId;
            entity.DateModifiedUtc = DateTime.UtcNow;
            entity.UserId = user.UserId;

            await context.SaveChangesAsync();

            // reset the integrations api cache for this user
            _credentialProvider.Cache.Clear(ApiCredentialKey.Create(user.UserId));
            // reset the config cache for this user
            _configCache.Cache.Clear(user.UserId);

            return _mapper.Map<UserIntegrationConfiguration>(integrationConfiguration);
        }

        public virtual async Task<UserPrinterConfiguration> CreateOrUpdatePrinterConfigurationAsync(UserPrinterConfiguration printerConfiguration)
        {
            var user = _requestContext.GetUserContext() ?? throw new UserContextUnauthorizedException();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.UserPrinterConfigurations
                .Include(x => x.UserPrinterTemplateConfigurations)
                .Where(x => x.UserId == user.UserId)
                .OrderByDescending(x => x.DateCreatedUtc) // should only ever be 1, but take the most recent one just in case.
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

            entity.UserId = user.UserId;
            entity.OrganizationId = user.OrganizationId;
            entity.DateModifiedUtc = DateTime.UtcNow;
            // add all associated template configurations
            foreach (var templateConfig in printerConfiguration.UserPrinterTemplateConfigurations)
            {
                var templateEntity = _mapper.Map<DataModel.UserPrinterTemplateConfiguration>(templateConfig);
                templateEntity.UserPrinterConfiguration = entity;
                templateEntity.UserId = user.UserId;
                templateEntity.OrganizationId = user.OrganizationId;
                templateEntity.DateCreatedUtc = DateTime.UtcNow;
                context.UserPrinterTemplateConfigurations.Add(templateEntity);
            }

            await context.SaveChangesAsync();

            // reset the config cache for this user
            _configCache.Cache.Clear(user.UserId);

            return _mapper.Map<UserPrinterConfiguration>(printerConfiguration);
        }

        public virtual async Task<UserLocaleConfiguration> CreateOrUpdateLocaleConfigurationAsync(UserLocaleConfiguration localeConfiguration)
        {
            var user = _requestContext.GetUserContext() ?? throw new UserContextUnauthorizedException();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.UserLocaleConfigurations
                .Where(x => x.UserId == user.UserId)
                .OrderByDescending(x => x.DateCreatedUtc) // should only ever be 1, but take the most recent one just in case.
                .FirstOrDefaultAsync();
            if (entity == null)
            {
                entity = _mapper.Map<DataModel.UserLocaleConfiguration>(localeConfiguration);
                entity.DateCreatedUtc = DateTime.UtcNow;
                context.UserLocaleConfigurations.Add(entity);
            }
            else
                entity = _mapper.Map(localeConfiguration, entity);
            entity.UserId = user.UserId;
            entity.OrganizationId = user.OrganizationId;
            entity.DateModifiedUtc = DateTime.UtcNow;

            await context.SaveChangesAsync();

            // reset the config cache for this user
            _configCache.Cache.Clear(user.UserId);

            return _mapper.Map<UserLocaleConfiguration>(localeConfiguration);
        }

        public async Task<UserBarcodeConfiguration> CreateOrUpdateBarcodeConfigurationAsync(UserBarcodeConfiguration barcodeConfiguration)
        {
            var user = _requestContext.GetUserContext() ?? throw new UserContextUnauthorizedException();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.UserBarcodeConfigurations
                .Where(x => x.UserId == user.UserId)
                .OrderByDescending(x => x.DateCreatedUtc) // should only ever be 1, but take the most recent one just in case.
                .FirstOrDefaultAsync();
            if (entity == null)
            {
                entity = _mapper.Map<DataModel.UserBarcodeConfiguration>(barcodeConfiguration);
                entity.DateCreatedUtc = DateTime.UtcNow;
                context.UserBarcodeConfigurations.Add(entity);
            }
            else
                entity = _mapper.Map(barcodeConfiguration, entity);
            entity.UserId = user.UserId;
            entity.OrganizationId = user.OrganizationId;
            entity.DateModifiedUtc = DateTime.UtcNow;

            await context.SaveChangesAsync();

            // reset the config cache for this user
            _configCache.Cache.Clear(user.UserId);

            return _mapper.Map<UserBarcodeConfiguration>(barcodeConfiguration);
        }

        public virtual void ClearCachedConfigurations(int? userId = null)
        {
            var uid = userId ?? _requestContext.GetUserContext()?.UserId ?? throw new ArgumentNullException(nameof(userId));
            // reset the config cache for this user
            _configCache.Cache.Clear(uid);
        }

        public virtual async Task<UserIntegrationConfiguration> GetIntegrationConfigurationAsync(int? userId = null)
        {
            var uid = userId ?? _requestContext.GetUserContext()?.UserId;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.UserIntegrationConfigurations
                .Where(x => x.UserId == uid)
                .FirstOrDefaultAsync();
            if (entity == null)
                return GetDefaultIntegrationConfiguration();

            return _mapper.Map<UserIntegrationConfiguration>(entity);
        }

        public virtual async Task<UserPrinterConfiguration> GetPrinterConfigurationAsync(int? userId = null)
        {
            var uid = userId ?? _requestContext.GetUserContext()?.UserId;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.UserPrinterConfigurations
                .Include(x => x.UserPrinterTemplateConfigurations)
                .Where(x => x.UserId == uid)
                .FirstOrDefaultAsync();
            if (entity == null)
                return GetDefaultPrinterConfiguration();
            return _mapper.Map<UserPrinterConfiguration>(entity);
        }

        public virtual async Task<UserLocaleConfiguration> GetLocaleConfigurationAsync(int? userId = null)
        {
            var uid = userId ?? _requestContext.GetUserContext()?.UserId;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.UserLocaleConfigurations
                .Where(x => x.UserId == uid)
                .FirstOrDefaultAsync();
            if (entity == null)
                return GetDefaultLocaleConfiguration();

            return _mapper.Map<UserLocaleConfiguration>(entity);
        }

        public virtual async Task<UserBarcodeConfiguration> GetBarcodeConfigurationAsync(int? userId = null)
        {
            var uid = userId ?? _requestContext.GetUserContext()?.UserId;
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.UserBarcodeConfigurations
                .Where(x => x.UserId == uid)
                .FirstOrDefaultAsync();
            if (entity == null)
                return GetDefaultBarcodeConfiguration();

            return _mapper.Map<UserBarcodeConfiguration>(entity);
        }

        public virtual UserIntegrationConfiguration GetCachedIntegrationConfiguration(int? userId = null)
        {
            var uid = userId ?? _requestContext.GetUserContext()?.UserId ?? throw new ArgumentNullException(nameof(userId));
            return _configCache.Cache.GetOrAddConfigAsync(uid, () => GetIntegrationConfigurationAsync(uid))
                .GetAwaiter()
                .GetResult();
        }

        public virtual UserPrinterConfiguration GetCachedPrinterConfiguration(int? userId = null)
        {
            var uid = userId ?? _requestContext.GetUserContext()?.UserId ?? throw new ArgumentNullException(nameof(userId));
            return _configCache.Cache.GetOrAddConfigAsync(uid, () => GetPrinterConfigurationAsync(uid))
                .GetAwaiter()
                .GetResult();
        }

        public virtual UserLocaleConfiguration GetCachedLocaleConfiguration(int? userId = null)
        {
            var uid = userId ?? _requestContext.GetUserContext()?.UserId ?? throw new ArgumentNullException(nameof(userId));
            return _configCache.Cache.GetOrAddConfigAsync(uid, () => GetLocaleConfigurationAsync(uid))
                .GetAwaiter()
                .GetResult();
        }

        public virtual UserBarcodeConfiguration GetCachedBarcodeConfiguration(int? userId = null)
        {
            var uid = userId ?? _requestContext.GetUserContext()?.UserId ?? throw new ArgumentNullException(nameof(userId));
            return _configCache.Cache.GetOrAddConfigAsync(uid, () => GetBarcodeConfigurationAsync(uid))
                .GetAwaiter()
                .GetResult();
        }

        protected virtual UserIntegrationConfiguration GetDefaultIntegrationConfiguration()
        {
            return new UserIntegrationConfiguration
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

        protected virtual UserLocaleConfiguration GetDefaultLocaleConfiguration()
        {
            return new UserLocaleConfiguration
            {
                Currency = Model.Currencies.USD,
                Language = Model.Languages.En,
            };
        }

        protected virtual UserBarcodeConfiguration GetDefaultBarcodeConfiguration()
        {
            return new UserBarcodeConfiguration
            {
            };
        }
    }
}
