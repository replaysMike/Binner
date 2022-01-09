using System;
using System.Collections.Generic;
using System.Threading;

namespace Binner.Common
{
    /// <summary>
    /// Singleton server context lives for the lifetime of the application
    /// </summary>
    public class ServerContext : IDisposable
    {
        private static readonly Lazy<ServerContext> _instance = new Lazy<ServerContext>(() => new ServerContext());
        private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private bool _isDisposed;

        /// <summary>
        /// Get an instance of the server context
        /// </summary>
        public static ServerContext Instance => _instance.Value;
        
        /// <summary>
        /// Context data
        /// </summary>
        private IDictionary<string, object> _contextData = new Dictionary<string, object>();

        private ServerContext()
        {
        }

        /// <summary>
        /// Set context data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Unique key to identify the data</param>
        /// <param name="data"></param>
        public static void Set<T>(string key, T data)
        {
            ServerContext.Instance.SetData<T>(key, data);
        }

        /// <summary>
        /// Get context data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Unique key to identify the data</param>
        /// <returns></returns>
        public static T Get<T>(string key)
        {
            return ServerContext.Instance.GetData<T>(key);
        }

        /// <summary>
        /// Remove context data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Unique key to identify the data</param>
        public static void Remove<T>(string key)
        {
            ServerContext.Instance.RemoveData<T>(key);
        }

        /// <summary>
        /// Set context data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Unique key to identify the data</param>
        /// <param name="data"></param>
        public void SetData<T>(string key, T data)
        {
            _lock.Wait();
            try
            {
                var wrappedKey = WrapKey<T>(key);
                if (_contextData.ContainsKey(wrappedKey))
                {
                    _contextData[wrappedKey] = data;
                }
                else
                {
                    _contextData.Add(wrappedKey, data);
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Get context data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Unique key to identify the data</param>
        /// <returns></returns>
        public T GetData<T>(string key)
        {
            _lock.Wait();
            try
            {
                var wrappedKey = WrapKey<T>(key);
                if (_contextData.ContainsKey(wrappedKey))
                {
                    return (T)_contextData[wrappedKey];
                }

                return default(T);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Remove a context data by key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Unique key to identify the data</param>
        public void RemoveData<T>(string key)
        {
            _lock.Wait();
            try
            {
                var wrappedKey = WrapKey<T>(key);
                if(_contextData.ContainsKey(wrappedKey))
                    _contextData.Remove(wrappedKey);
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Allows for multiple datatypes to be stored with the same key name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        private string WrapKey<T>(string key)
        {
            return $"{key}:{typeof(T).Name}";
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (_isDisposed)
                return;

            if (isDisposing)
            {
                _lock.Wait();
                try
                {
                    _contextData.Clear();
                    _contextData = null;
                }
                finally
                {
                    _lock.Release();
                    _lock.Dispose();
                    _lock = null;
                }

            }
            _isDisposed = true;
        }
    }
}
