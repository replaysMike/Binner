using Binner.Common.Cache;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Binner.Common.Integrations
{
    /// <summary>
    /// An in-memory cache of api integration credentials per entity
    /// </summary>
    public class ConfigurationCache<TStore> where TStore : IConfigStore
    {
        private readonly Dictionary<long, TStore> _configCache = new Dictionary<long, TStore>();
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public ConfigurationCache() { }

        public ConfigurationCache(Dictionary<long, TStore> credentials)
        {
            _configCache = credentials;
        }

        /// <summary>
        /// Returns true if the cache contains a key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(long key)
        {
            _lock.Wait();
            try
            {
                return _configCache.ContainsKey(key);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Clear all credentials for a given key
        /// </summary>
        /// <param name="key"></param>
        public void Clear(long key)
        {
            _lock.Wait();
            try
            {
                if (_configCache.ContainsKey(key))
                {
                    _configCache.Remove(key);
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Clear all credentials for a given key
        /// </summary>
        /// <param name="key"></param>
        public void Clear()
        {
            _lock.Wait();
            try
            {
                _configCache.Clear();
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Get or add a config
        /// </summary>
        /// <param name="key"></param>
        /// <param name="apiName"></param>
        /// <param name="addMethod"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DataException"></exception>
        public async Task<TConfig> GetOrAddConfigAsync<TConfig>(long key, Func<Task<TConfig>> addMethod)
            where TConfig : class
        {
            if (addMethod == null) throw new ArgumentNullException(nameof(addMethod));

            await _lock.WaitAsync();
            try
            {
                TConfig? config;
                if (!_configCache.ContainsKey(key))
                    _configCache.Add(key, Activator.CreateInstance<TStore>());

                var configs = _configCache[key];
                config = configs.FirstOrDefault<TConfig>();
                if (config == null)
                {
                    // no cached value yet, create it
                    config = await addMethod();
                    configs.AddOrUpdate(config);
                }

                // return the credential requested
                return config;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
