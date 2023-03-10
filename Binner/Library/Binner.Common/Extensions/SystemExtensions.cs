using Binner.Model.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
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
        /// Uppercase the first character of a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UcFirst(this string str)
        {
            return str.First().ToString().ToUpper() + str.Substring(1);
        }

        /// <summary>
        /// Get the numeric value from a currency formatted string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static double FromCurrency(this string? input)
        {
            if (input == null) return 0d;

            var result = 0d;
            var allCulturesByCurrencySymbol = CultureInfo.GetCultures(CultureTypes.AllCultures)
                .GroupBy(c => c.NumberFormat.CurrencySymbol)
                .ToDictionary(c => c.Key, c => c.ToList());
            var culturesMatchingInputCurrencySymbol = allCulturesByCurrencySymbol.FirstOrDefault(c => input.Contains(c.Key));

            // try to find the closest matching culture to the input string
            var foundMatching = false;
            if (culturesMatchingInputCurrencySymbol.Value?.Any() == true)
            {
                foreach (var c in culturesMatchingInputCurrencySymbol.Value)
                {
                    if (double.TryParse(input, NumberStyles.Currency | NumberStyles.AllowDecimalPoint, c, out var successResult))
                    {
                        // success, use this culture
                        foundMatching = true;
                        result = successResult;
                        break;
                    }
                }
            }

            // found no matching value to the format
            if (!foundMatching)
            {
                if (!double.TryParse(input, NumberStyles.Currency | NumberStyles.AllowDecimalPoint, System.Threading.Thread.CurrentThread.CurrentCulture, out result))
                    double.TryParse(input, out result);
            }

            return result;
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

        /// <summary>
        /// Get a distinct set of results
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="items"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static IEnumerable<T> DistinctBy<T, TKey>(this IOrderedEnumerable<T> items, Func<T, TKey> property)
        {
            return items
                .GroupBy(property)
                .Select(x => x.First());
        }

        /// <summary>
        /// Sorts the elements of a sequence according to a key and the sort order.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="query" />.</typeparam>
        /// <param name="query">A sequence of values to order.</param>
        /// <param name="key">Name of the property of <see cref="TSource"/> by which to sort the elements.</param>
        /// <param name="ascending">True for ascending order, false for descending order.</param>
        /// <returns>An <see cref="T:System.Linq.IOrderedQueryable`1" /> whose elements are sorted according to a key and sort order.</returns>
        public static IEnumerable<TSource> OrderBy<TSource>(this IEnumerable<TSource> query, string key, SortDirection direction)
        {
            if (string.IsNullOrWhiteSpace(key))
                return query;

            var lambda = CreateExpression<TSource>(key);

            var func = lambda.Compile();
            return direction == SortDirection.Ascending
                ? Enumerable.OrderBy<TSource, object>(query, func)
                : Enumerable.OrderByDescending<TSource, object>(query, func);
        }

        private static Expression<Func<TSource, object>> CreateExpression<TSource>(string propertyName)
        {
            var param = Expression.Parameter(typeof(TSource), "x");

            Expression body = param;
            foreach (var member in propertyName.Split('.'))
                body = Expression.PropertyOrField(body, member);

            return Expression.Lambda<Func<TSource, object>>(Expression.Convert(body, typeof(object)), param);
        }
    }
}
