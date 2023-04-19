using Octokit;
using System;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Binner.Common.Services
{
    public class VersionManagementService
    {
        private const string GitHubEndpoint = "https://github.com/replaysMike/Binner/releases";
        private static readonly Lazy<MemoryCache> _cache = new Lazy<MemoryCache>(() => new MemoryCache("VersionManagement"));

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
