using System.Collections.Generic;

namespace Binner.Common.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Return value as typed value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T As<T>(this object? value) => value is T t ? t : default!;

        /// <summary>
        /// Get value from Dictionary, returns null if key does not exist
        /// </summary>
        /// <typeparam name="TK"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TV GetValue<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV defaultValue = default) 
            => dict.TryGetValue(key, out var value) ? value : defaultValue;
    }
}
