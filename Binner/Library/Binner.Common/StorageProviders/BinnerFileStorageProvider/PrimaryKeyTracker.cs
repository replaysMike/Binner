using System;
using System.Collections.Generic;
using System.Threading;

namespace Binner.Common.StorageProviders
{
    /// <summary>
    /// Tracks primary keys on objects
    /// </summary>
    public class PrimaryKeyTracker : IDisposable
    {
        private IDictionary<string, long> _primaryKeys;
        private SemaphoreSlim _dataLock = new SemaphoreSlim(1, 1);
        private bool _isDisposed;

        public PrimaryKeyTracker(IDictionary<string, long> keys)
        {
            _primaryKeys = keys;
        }

        /// <summary>
        /// Get the next primary key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long GetNextKey<T>()
        {
            return GetNextKey(typeof(T).Name);
        }

        /// <summary>
        /// Get the next primary key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long GetNextKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            _dataLock.Wait();
            try
            {
                if (!_primaryKeys.ContainsKey(key))
                    _primaryKeys.Add(key, 0);
                var nextKey = _primaryKeys[key];
                _primaryKeys[key]++;
                return nextKey;
            }
            finally
            {
                _dataLock.Release();
            }
        }

        /// <summary>
        /// Set the value of a key to a specific value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        public void SetNextKey<T>(long value)
        {
            SetNextKey(typeof(T).Name, value);
        }

        /// <summary>
        /// Set the value of a key to a specific value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetNextKey(string key, long value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            _dataLock.Wait();
            try
            {
                if (!_primaryKeys.ContainsKey(key))
                    throw new ArgumentException("Invalid key name specified", nameof(key));
                _primaryKeys[key] = value;
            }
            finally
            {
                _dataLock.Release();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (_isDisposed)
                return;
            if (isDisposing)
            {
                _dataLock.Wait();
                try
                {
                    _primaryKeys.Clear();
                }
                finally
                {
                    _dataLock.Release();
                    _dataLock.Dispose();
                }
            }
            _isDisposed = true;
        }
    }
}
