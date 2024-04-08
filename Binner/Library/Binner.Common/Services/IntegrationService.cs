using Binner.Common.Integrations;
using Binner.Global.Common;
using Binner.Model.Configuration.Integrations;
using Binner.Model.Requests;
using Binner.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public class IntegrationService
    {
        private readonly IIntegrationApiFactory _integrationApiFactory;
        private readonly RequestContextAccessor _requestContext;
        private readonly ICredentialService _credentialService;
        private readonly IIntegrationCredentialsCacheProvider _credentialProvider;

        public IntegrationService(IIntegrationApiFactory integrationApiFactory, RequestContextAccessor requestContextAccessor, ICredentialService credentialService, IIntegrationCredentialsCacheProvider credentialProvider)
        {
            _integrationApiFactory = integrationApiFactory;
            _requestContext = requestContextAccessor;
            _credentialService = credentialService;
            _credentialProvider = credentialProvider;
        }

        public async Task<TestApiResponse> TestApiAsync(TestApiRequest request)
        {
            var user = _requestContext.GetUserContext();

#pragma warning disable CS1998
            var getCredentialsMethod = async () =>
#pragma warning restore CS1998
            {
                // create a db context
                //using var context = await _contextFactory.CreateDbContextAsync();
                /*var userIntegrationConfiguration = await context.UserIntegrationConfigurations
                    .Where(x => x.UserId.Equals(userId))
                    .FirstOrDefaultAsync()
                    ?? new Data.DataModel.UserIntegrationConfiguration();*/
                // todo: temporary until we move integration configuration to the UI
                var comparisonType = StringComparison.InvariantCultureIgnoreCase;

                // build the credentials list
                var credentials = new List<ApiCredential>();

                // add user defined credentials
                var swarmConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", request.Configuration.Where(x => x.Key.Equals("Enabled", comparisonType) && x.Value != null).Select(x => bool.Parse(x.Value ?? "false")).FirstOrDefault() },
                    { "ApiKey", request.Configuration.Where(x => x.Key.Equals("ApiKey", comparisonType) && x.Value != null).Select(x =>x.Value).FirstOrDefault() ?? string.Empty },
                    { "ApiUrl", request.Configuration.Where(x => x.Key.Equals("ApiUrl", comparisonType) && x.Value != null).Select(x => WrapUrl(x.Value)).FirstOrDefault() ?? string.Empty },
                    { "Timeout", request.Configuration.Where(x => x.Key.Equals("Timeout", comparisonType) && x.Value != null).Select(x => TimeSpan.Parse(x.Value ?? "00:00:05")).FirstOrDefault() },
                };
                credentials.Add(new ApiCredential(user?.UserId ?? 0, swarmConfiguration, nameof(SwarmApi)));

                var digikeyConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", request.Configuration.Where(x => x.Key.Equals("Enabled", comparisonType) && x.Value != null).Select(x => bool.Parse(x.Value ?? "false")).FirstOrDefault() },
                    { "Site", request.Configuration.Where(x => x.Key.Equals("Site", comparisonType) && x.Value != null).Select(x => int.Parse(x.Value ?? "0")).FirstOrDefault() },
                    { "ClientId", request.Configuration.Where(x => x.Key.Equals("ClientId", comparisonType) && x.Value != null).Select(x =>x.Value).FirstOrDefault() ?? string.Empty },
                    { "ClientSecret", request.Configuration.Where(x => x.Key.Equals("ClientSecret", comparisonType) && x.Value != null).Select(x =>x.Value).FirstOrDefault() ?? string.Empty },
                    { "oAuthPostbackUrl", request.Configuration.Where(x => x.Key.Equals("oAuthPostbackUrl", comparisonType) && x.Value != null).Select(x =>x.Value).FirstOrDefault() ?? string.Empty },
                    { "ApiUrl", request.Configuration.Where(x => x.Key.Equals("ApiUrl", comparisonType) && x.Value != null).Select(x =>  WrapUrl(x.Value)).FirstOrDefault() ?? string.Empty }
                };
                credentials.Add(new ApiCredential(user?.UserId ?? 0, digikeyConfiguration, nameof(DigikeyApi)));

                var mouserConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", request.Configuration.Where(x => x.Key.Equals("Enabled", comparisonType) && x.Value != null).Select(x => bool.Parse(x.Value ?? "false")).FirstOrDefault() },
                    { "CartApiKey", request.Configuration.Where(x => x.Key.Equals("CartApiKey", comparisonType) && x.Value != null).Select(x =>x.Value).FirstOrDefault() ?? string.Empty },
                    { "OrderApiKey", request.Configuration.Where(x => x.Key.Equals("OrderApiKey", comparisonType) && x.Value != null).Select(x => x.Value).FirstOrDefault() ?? string.Empty },
                    { "SearchApiKey", request.Configuration.Where(x => x.Key.Equals("SearchApiKey", comparisonType) && x.Value != null).Select(x => x.Value).FirstOrDefault() ?? string.Empty },
                    { "ApiUrl", request.Configuration.Where(x => x.Key.Equals("ApiUrl", comparisonType) && x.Value != null).Select(x => WrapUrl(x.Value)).FirstOrDefault() ?? string.Empty },
                };
                credentials.Add(new ApiCredential(user?.UserId ?? 0, mouserConfiguration, nameof(MouserApi)));

                var arrowConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", request.Configuration.Where(x => x.Key.Equals("Enabled", comparisonType) && x.Value != null).Select(x => bool.Parse(x.Value ?? "false")).FirstOrDefault() },
                    { "Username", request.Configuration.Where(x => x.Key.Equals("Username", comparisonType) && x.Value != null).Select(x => x.Value).FirstOrDefault() ?? string.Empty },
                    { "ApiKey", request.Configuration.Where(x => x.Key.Equals("ApiKey", comparisonType) && x.Value != null).Select(x =>x.Value).FirstOrDefault() ?? string.Empty },
                    { "ApiUrl", request.Configuration.Where(x => x.Key.Equals("ApiUrl", comparisonType) && x.Value != null).Select(x => WrapUrl(x.Value)).FirstOrDefault() ?? string.Empty },
                };
                credentials.Add(new ApiCredential(user?.UserId ?? 0, arrowConfiguration, nameof(ArrowApi)));

                var octopartConfiguration = new Dictionary<string, object>
                {
                    { "Enabled", request.Configuration.Where(x => x.Key.Equals("Enabled", comparisonType) && x.Value != null).Select(x => bool.Parse(x.Value ?? "false")).FirstOrDefault() },
                    { "ClientId", request.Configuration.Where(x => x.Key.Equals("ClientId", comparisonType) && x.Value != null).Select(x => x.Value).FirstOrDefault() ?? string.Empty },
                    { "ClientSecret", request.Configuration.Where(x => x.Key.Equals("ClientSecret", comparisonType) && x.Value != null).Select(x => x.Value).FirstOrDefault() ?? string.Empty },
                };
                credentials.Add(new ApiCredential(user?.UserId ?? 0, octopartConfiguration, nameof(NexarApi)));

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

        public async Task<TestApiResponse> ForgetCachedCredentialsAsync(ForgetCachedCredentialsRequest request)
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

        private static string WrapUrl(string? url)
        {
            if (url == null)
                return string.Empty;
            if (!url.Contains("http://") && !url.Contains("https://"))
                url = $"https://{url}";
            return url;
        }
    }
}
