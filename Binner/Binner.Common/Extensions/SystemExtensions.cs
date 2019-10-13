using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace Binner.Common.Extensions
{
    public static class SystemExtensions
    {
        /// <summary>
        /// Get the description name attribute value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(this System.Type value)
        {
            //var fi = value.GetField(value.ToString());
            var attributes = (DescriptionAttribute[])value.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString().ExpandOnCase();
        }

        /// <summary>
        /// Get the display name attribute value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDisplayName(this System.Type value)
        {
            //var fi = value.GetField(value.ToString());
            var attributes = (DisplayNameAttribute[])value.GetCustomAttributes(typeof(DisplayNameAttribute), false);
            if (attributes.Length > 0)
                return attributes[0].DisplayName;
            else
                return value.ToString().ExpandOnCase();
        }

        /// <summary>
        /// Expand a string into multiple words based on casing
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ExpandOnCase(this string str)
        {
            return Regex.Replace(str, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled);
        }

        /// <summary>
        /// Get a distinct set of results
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="items"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        {
            return items.GroupBy(property).Select(x => x.First());
        }
    }
}
