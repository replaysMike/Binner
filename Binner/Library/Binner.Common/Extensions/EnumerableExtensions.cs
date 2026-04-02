using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Binner.Common.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Order an IEnumerable using natual sort
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<T> OrderByNaturalSort<T>(this IEnumerable<T> source, Func<T, string> selector)
        {
            var max = source
                .SelectMany(i => Regex.Matches(selector(i), @"\d+").Cast<Match>().Select(m => (int?)m.Value.Length))
                .Max() ?? 0;

            return source.OrderBy(i => Regex.Replace(selector(i), @"\d+", m => m.Value.PadLeft(max, '0')));
        }

        /// <summary>
        /// Order a IDictionary using natual sort
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<KeyValuePair<TKey, TValue>> OrderByNaturalSort<TKey, TValue>(this IDictionary<TKey, TValue> source, Func<KeyValuePair<TKey, TValue>, string> selector)
        {
            var max = source
                .SelectMany(i => Regex.Matches(selector(i), @"\d+").Cast<Match>().Select(m => (int?)m.Value.Length))
                .Max() ?? 0;

            return source.OrderBy(i => Regex.Replace(selector(i), @"\d+", m => m.Value.PadLeft(max, '0')));
        }

        /// <summary>
        /// Recursively select a property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEnumerable<T> SelectRecursive<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
        {
            foreach (var parent in source)
            {
                yield return parent;

                var children = selector(parent);
                foreach (var child in SelectRecursive(children, selector))
                    yield return child;
            }
        }

        /// <summary>
        /// Returns true if any string in the collection starts with the specified search string
        /// </summary>
        /// <param name="source"></param>
        /// <param name="searchStr">String to compare</param>
        /// <param name="comparisonType">String comparison type</param>
        /// <returns></returns>
        public static bool StartsWithAny(this IEnumerable<string> source, string searchStr, StringComparison comparisonType = StringComparison.CurrentCulture)
        {
            if (!source.Any()) return false;
            foreach (var str in source)
            {
                if (str.StartsWith(searchStr, comparisonType))
                    return true;
            }
            return false;
        }
    }
}
