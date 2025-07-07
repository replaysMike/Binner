using System;

namespace Binner.Common.Cache
{
    public class CacheItem<TConfig> where TConfig : class
    {
        public string Name => typeof(TConfig).Name;
        public TConfig Value { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime LastAccessed { get; set; }

        public CacheItem(TConfig value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value), "Cache item value cannot be null");
            LastUpdated = DateTime.UtcNow;
            LastAccessed = DateTime.UtcNow;
        }

        public void Touch() => LastAccessed = DateTime.UtcNow;
    }
}
