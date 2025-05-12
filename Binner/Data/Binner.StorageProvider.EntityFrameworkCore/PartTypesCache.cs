using Binner.Data;
using Binner.Model.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Binner.StorageProvider.EntityFrameworkCore
{
    /// <summary>
    /// Cache of Part Types
    /// </summary>
    public class PartTypesCache : IPartTypesCache
    {
        // cache age can be set from configuration, this services as an un-configured default value
        private static TimeSpan _maxCacheAge = TimeSpan.FromMinutes(30);
        
        private static IDbContextFactory<BinnerContext>? _contextFactory;
        private static readonly object PartTypesCacheLock = new ();
        private static DateTime CacheAge { get; set; }
        private static readonly Lazy<List<CachedPartTypeResponse>> CacheInstance = new (LoadPartTypesCache);
        private static List<CachedPartTypeResponse> LockedCacheInstance 
        {
            get
            {
                lock (PartTypesCacheLock)
                {
                    if (CacheInstance.IsValueCreated && DateTime.UtcNow - CacheAge >= _maxCacheAge)
                    {
                        // refresh stale cache
                        InvalidateAndReload();
                    }
                    return CacheInstance.Value;
                }
            }
        }

        private static void InvalidateAndReload()
        {
            CacheInstance.Value.Clear();
            var newCache = LoadPartTypesCache();
            CacheInstance.Value.AddRange(newCache);
            CacheAge = DateTime.UtcNow;
        }

        private static List<CachedPartTypeResponse> LoadPartTypesCache()
        {
            if (_contextFactory == null) throw new InvalidOperationException($"{nameof(PartTypesCache)} must be injected to an instance reference before being accessed!");
            using var context = _contextFactory.CreateDbContext();
            CacheAge = DateTime.UtcNow;
            var partTypes = context
                .PartTypes
                .Include(x => x.ParentPartType)
                .OrderBy(x => x.PartTypeId)
                // note: this is a performance boost rather than mapping afterwards, as the part count can be translated by EF
                .Select(x => new CachedPartTypeResponse
                {
                    PartTypeId = x.PartTypeId,
                    Name = x.Name,
                    Icon = x.Icon,
                    ParentPartTypeId = x.ParentPartTypeId,
                    Parts = x.Parts.Count,
                    DateCreatedUtc = x.DateCreatedUtc,
                    IsSystem = x.UserId == null && x.OrganizationId == null,
                    UserId = x.UserId,
                    OrganizationId = x.OrganizationId,
                    Description = x.Description,
                    Keywords = x.Keywords,
                    ReferenceDesignator = x.ReferenceDesignator,
                    SymbolId = x.SymbolId,
                    DateModifiedUtc = x.DateModifiedUtc
                })
                .ToList();
            // recursively map all partTypes and add up part counts to include its children
            partTypes = GetPartTypesWithPartCounts(partTypes);

            return partTypes;
        }

        private static List<CachedPartTypeResponse> GetPartTypesWithPartCounts(List<CachedPartTypeResponse> partTypes)
        {
            var parents = partTypes.Where(x => x.ParentPartTypeId == null).ToList();
            for (var i = 0; i < parents.Count; i++)
            {
                parents[i] = MapPartType(partTypes, parents[i]);
            }
            return partTypes;
        }

        private static CachedPartTypeResponse MapPartType(List<CachedPartTypeResponse> partTypes, CachedPartTypeResponse partType)
        {
            var childPartTypes = partTypes.Where(x => x.ParentPartTypeId == partType.PartTypeId).ToList();
            var childCount = 0l;
            for (var i = 0; i < childPartTypes.Count; i++)
            {
                childPartTypes[i] = MapPartType(partTypes, childPartTypes[i]);
                childPartTypes[i].ParentPartType = partType;
                childCount += childPartTypes[i].Parts;
            }

            partType.Parts += childCount;
            return partType;
        }

        /// <summary>
        /// Get the PartTypes cache
        /// </summary>
        public List<CachedPartTypeResponse> Cache => LockedCacheInstance;

        public PartTypesCache(IDbContextFactory<BinnerContext> contextFactory, WebHostServiceConfiguration configuration)
        {
            _contextFactory = contextFactory;
            _maxCacheAge = configuration.MaxPartTypesCacheLifetime;
        }

        /// <summary>
        /// Reload the part types cache
        /// </summary>
        public void InvalidateCache()
        {
            lock (PartTypesCacheLock)
            {
                // refresh stale cache
                InvalidateAndReload();
            }
        }
    }
}
