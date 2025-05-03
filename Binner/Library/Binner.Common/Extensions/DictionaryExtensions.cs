using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

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
        public static TV? GetValue<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV? defaultValue = default)
        {
            if (dict.TryGetValue(key, out var value))
            {
                if (value is JsonElement val)
                {
                    switch(val.ValueKind)
                    {
                        case JsonValueKind.Null:
                            return defaultValue;
                        case JsonValueKind.String:
                            return val.GetString()!.As<TV>();
                        case JsonValueKind.Number:
                            return val.GetInt32().As<TV>();
                        case JsonValueKind.True:
                            return val.GetBoolean().As<TV>();
                        case JsonValueKind.False:
                            return val.GetInt32().As<TV>();
                        default:
                            break;
                    }
                    return defaultValue;
                }
                return value;
            }
            return defaultValue;
        }
    }
}
