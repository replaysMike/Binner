using AutoMapper;
using Binner.Data;
using Binner.Global.Common;
using Binner.Model;
using Binner.Services.Integrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Octokit;
using System.Net.Mime;
using System.Runtime.Caching;

namespace Binner.Services
{
    public class VersionManagementService : IVersionManagementService
    {
        private const string GitHubEndpoint = "https://github.com/replaysMike/Binner/releases";
        private static readonly Lazy<MemoryCache> _cache = new Lazy<MemoryCache>(() => new MemoryCache("VersionManagement"));
        private static readonly TimeSpan ConnectionTimeout = TimeSpan.FromSeconds(5);
        protected readonly ILogger<VersionManagementService> _logger;
        protected readonly IDbContextFactory<BinnerContext> _contextFactory;
        protected readonly IRequestContextAccessor _requestContext;
        protected readonly IMapper _mapper;

        public VersionManagementService(ILogger<VersionManagementService> logger, IDbContextFactory<BinnerContext> contextFactory, IRequestContextAccessor requestContext, IMapper mapper)
        {
            _logger = logger;
            _contextFactory = contextFactory;
            _requestContext = requestContext;
            _mapper = mapper;
        }

        /// <summary>
        /// Get the latest version of Binner
        /// </summary>
        /// <returns></returns>
        public async Task<BinnerVersion> GetLatestVersionAsync()
        {
            try
            {
                var cacheValue = _cache.Value.GetCacheItem("LatestVersion");
                if (cacheValue == null)
                {
                    var client = new GitHubClient(new ProductHeaderValue("Binner"));

                    // set a short timeout for this operation
                    client.Connection.SetRequestTimeout(ConnectionTimeout);

                    var releases = await client.Repository.Release.GetAll("replaysMike", "Binner");
                    var latest = releases.FirstOrDefault(x => !x.Prerelease && !x.Draft);
                    if (latest != null)
                    {
                        var value = new BinnerVersion(latest.TagName, latest.Body, latest.HtmlUrl, false);
                        _cache.Value.Add("LatestVersion", value, new CacheItemPolicy { AbsoluteExpiration = DateTime.UtcNow.AddMinutes(15) });
                        return value;
                    }
                }
                else
                {
                    var value = (BinnerVersion)cacheValue.Value;
                    value.IsCached = true;
                    return value;
                }
            }
            catch (RateLimitExceededException)
            {
                // rate limited
            }
            catch (Exception)
            {
                // swallow exception
            }

            return new BinnerVersion("v1.0", "Binner", GitHubEndpoint, false);
        }

        public virtual async Task<ICollection<MessageState>> GetSystemMessagesAsync()
        {
            try
            {
                var userContext = _requestContext.GetUserContext() ?? throw new UserContextUnauthorizedException();
                await using var context = await _contextFactory.CreateDbContextAsync();
                // fetch system messages from binner.io, then store any new ones.
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Accept", MediaTypeNames.Application.Json);
                client.DefaultRequestHeaders.Add("User-Agent", ApiConstants.BinnerAgent);
                client.Timeout = TimeSpan.FromSeconds(5);
#if BINNERIO
                var response = await client.GetStringAsync(ApiConstants.BinnerLocalSystemMessageUrl);
#else
                var response = await client.GetStringAsync(ApiConstants.BinnerSystemMessageUrl);
#endif
                if (!string.IsNullOrEmpty(response))
                {
                    var results = JsonConvert.DeserializeObject<ICollection<MessageState>>(response);
                    if (results != null)
                    {
                        // take no more than X messages
                        results = results.Take(ApiConstants.BinnerSystemMessagesCount).ToList();
                        // filter out messages that are already read by the user
                        foreach (var message in results)
                        {
                            var existingMessage = await context.MessageStates
                                .Where(x => x.UserId == userContext.UserId && x.OrganizationId == userContext.OrganizationId
                                    && x.MessageId == message.MessageId)
                                .FirstOrDefaultAsync();
                            if (existingMessage == null)
                            {
                                context.MessageStates.Add(new Data.Model.MessageState
                                {
                                    MessageId = message.MessageId,
                                    DateCreatedUtc = DateTime.UtcNow,
                                    UserId = userContext.UserId,
                                    OrganizationId = userContext.OrganizationId,
                                });
                                message.ReadDateUtc = null; // mark as unread
                            }
                            else
                            {
                                // message already exists, return the existing read date status
                                message.ReadDateUtc = existingMessage.ReadDateUtc;
                            }
                        }
                        await context.SaveChangesAsync();
                        return results;
                    }
                }
            }
            catch (Exception ex)
            {
                // bad response or communication error
                _logger.LogError(ex, $"Failed to retreive system messages!");
            }

            return new List<MessageState>()
            {
            };

        }

        public async Task UpdateSystemMessagesReadAsync(UpdateSystemMessagesRequest request)
        {
            var userContext = _requestContext.GetUserContext() ?? throw new UserContextUnauthorizedException();
            await using var context = await _contextFactory.CreateDbContextAsync();

            foreach (var messageId in request.MessageIds)
            {
                var entity = await context.MessageStates
                    .Where(x => x.UserId == userContext.UserId && x.OrganizationId == userContext.OrganizationId
                        && x.MessageId == messageId)
                    .FirstOrDefaultAsync();
                if (entity == null)
                    context.MessageStates.Add(new Data.Model.MessageState
                    {
                        MessageId = messageId,
                        ReadDateUtc = DateTime.UtcNow,
                        UserId = userContext.UserId,
                        OrganizationId = userContext.OrganizationId,
                        DateCreatedUtc = DateTime.UtcNow
                    });
                else
                    entity.ReadDateUtc = DateTime.UtcNow;
            }
            await context.SaveChangesAsync();
        }

        public class BinnerVersion
        {
            public string Version { get; set; }
            public string Description { get; set; }
            public string Url { get; set; }
            public bool IsCached { get; set; }

            public BinnerVersion(string version, string description, string url, bool isCached)
            {
                Version = version;
                Description = description;
                Url = url;
                IsCached = isCached;
            }
        }
    }
}
