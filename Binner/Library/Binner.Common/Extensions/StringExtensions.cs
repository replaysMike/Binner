using Binner.Common.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Binner.Common.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Returns true if string starts with a digit
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool StartsWithDigit(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;
            return Char.IsDigit(str[0]);
        }

        /// <summary>
        /// Returns true if string ends with a digit
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool EmdsWithDigit(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;
            return Char.IsDigit(str[str.Length - 1]);
        }

        /// <summary>
        /// Returns true if the string contains any of a list of values
        /// </summary>
        /// <param name="str"></param>
        /// <param name="valuesToCheck">Values to check the string for</param>
        /// <returns></returns>
        public static bool ContainsAny(this string str, IEnumerable<string> valuesToCheck)
        {
            return ContainsAny(str, valuesToCheck, StringComparison.InvariantCulture);
        }

        /// <summary>
        /// Returns true if the string contains any of a list of values
        /// </summary>
        /// <param name="str"></param>
        /// <param name="valuesToCheck">Values to check the string for</param>
        /// <param name="comparisonType"></param>
        /// <returns></returns>
        public static bool ContainsAny(this string str, IEnumerable<string> valuesToCheck, StringComparison comparisonType)
        {
            foreach (var value in valuesToCheck)
            {
                if (str.Contains(value, comparisonType))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the string contains all of a list of values
        /// </summary>
        /// <param name="str"></param>
        /// <param name="valuesToCheck">Values to check the string for</param>
        /// <returns></returns>
        public static bool ContainsAll(this string str, IEnumerable<string> valuesToCheck)
        {
            return ContainsAll(str, valuesToCheck, StringComparison.InvariantCulture);
        }

        /// <summary>
        /// Returns true if the string contains all of a list of values
        /// </summary>
        /// <param name="str"></param>
        /// <param name="valuesToCheck">Values to check the string for</param>
        /// <param name="comparisonType"></param>
        /// <returns></returns>
        public static bool ContainsAll(this string str, IEnumerable<string> valuesToCheck, StringComparison comparisonType)
        {
            foreach (var value in valuesToCheck)
            {
                if (!str.Contains(value, comparisonType))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns true if string contains a digit
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool ContainsDigit(this string str)
        {
            return str.Any(x => Char.IsDigit(x));
        }

        /// <summary>
        /// Returns true if string contains both a digit and an alpha letter
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool ContainsDigitAndLetter(this string str)
        {
            return str.Any(x => Char.IsLetterOrDigit(x));
        }

        /// <summary>
        /// Returns true if string contains an alpha letter
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool ContainsLetter(this string str)
        {
            return str.Any(x => Char.IsLetter(x));
        }

        /// <summary>
        /// Returns true if string is all an alpha letter or digit
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsLetterOrDigit(this string str)
        {
            return str.All(x => Char.IsLetterOrDigit(x));
        }

        /// <summary>
        /// Returns true if string is all an alpha letter or digit
        /// </summary>
        /// <param name="str"></param>
        /// <param name="allowAdditionalCharacters">Additional characters that will be allowed</param>
        /// <returns></returns>
        public static bool IsLetterOrDigitAnd(this string str, string allowAdditionalCharacters)
        {
            return str.All(x => Char.IsLetterOrDigit(x) || allowAdditionalCharacters.Contains(x));
        }

        /// <summary>
        /// Returns true if string is all alpha letters
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsLetter(this string str)
        {
            return str.All(x => Char.IsLetter(x));
        }

        /// <summary>
        /// Returns true if string is all digits
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsDigit(this string str)
        {
            return str.All(x => Char.IsDigit(x));
        }

        /// <summary>
        /// Returns true if string contains a date
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool ContainsDate(this string str)
        {
            var searchStr = str.ToLower();
            if (searchStr.ContainsDigitAndLetter() &&
                (
                searchStr.Contains("jan")
                || searchStr.Contains("feb")
                || searchStr.Contains("mar")
                || searchStr.Contains("apr")
                || searchStr.Contains("may")
                || searchStr.Contains("jun")
                || searchStr.Contains("jul")
                || searchStr.Contains("aug")
                || searchStr.Contains("sep")
                || searchStr.Contains("oct")
                || searchStr.Contains("nov")
                || searchStr.Contains("dec")
                ))
                return true;

            return false;
        }

        /// <summary>
        /// Get a comma delimited nested string. Will split occurrances of "test1,test2,test3 (something, something2),test4" properly.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="delimiter">Delimiter to split on</param>
        /// <param name="groupings">Groups of tuples that are allowed as nested containers. default: '(', ')'.</param>
        /// <returns></returns>
        public static List<string> SplitNestedString(this string str, char delimiter = ',', IEnumerable<(char, char)>? groupings = null)
        {
            var result = new List<string>();
            if (!string.IsNullOrEmpty(str))
            {
                // default grouping allowed
                if (groupings == null)
                    groupings = new[] { ('(', ')') };

                var startGrouping = -1;
                var endGrouping = -1;
                var startNestedGrouping = -1;
                for (var i = 0; i < str.Length; i++)
                {
                    if (startGrouping == -1)
                        startGrouping = i;
                    if (groupings.Any(x => x.Item1.Equals(str[i])))
                        startNestedGrouping = i;    // start closure
                    if (groupings.Any(x => x.Item2.Equals(str[i])) && startNestedGrouping > -1)
                    {
                        startNestedGrouping = -1;   // end closure
                    }
                    if (str[i] == delimiter && startNestedGrouping == -1)
                    {
                        endGrouping = i;
                    }
                    // add a grouping
                    if (startGrouping != -1 && (endGrouping != -1 || i == str.Length - 1))
                    {
                        if (endGrouping == -1) endGrouping = str.Length;
                        result.Add(str.Substring(startGrouping, endGrouping - startGrouping));
                        startGrouping = -1;
                        endGrouping = -1;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Ensure a string doesn't exceed a max length defined by a <see cref="MaxLengthAttribute"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        public static string? EnsureMaxLength<T>(this string? str, Expression<Func<T, string>> propertyExpression)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            // ensure it doesn't exceed max length
            var maxLength = AttributeHelpers.GetMaxLength<T>(propertyExpression);
            if(str.Length > maxLength)
                return str.Substring(0, maxLength);
            return str;
        }

        /// <summary>
        /// Select words from a string up to a maximum number of words.
        /// All whitespace/line breaks will be removed.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxWordsToReturn">The maximum number of words to return</param>
        /// <returns></returns>
        public static string? SelectMaxWords(this string? str, int maxWordsToReturn)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;
            var trimmedStr = str
                .Replace("\r", "").Replace("\n", "").Replace("\t", " ")
                .Split(" ", maxWordsToReturn, StringSplitOptions.RemoveEmptyEntries);
            // the last item contains the rest of the string, so take the first word of it and discard the rest
            if (trimmedStr.Length > 0)
                trimmedStr[trimmedStr.Length - 1] = trimmedStr[trimmedStr.Length - 1]
                    .Split(" ", 2, StringSplitOptions.RemoveEmptyEntries)[0];
            return string.Join(" ", trimmedStr);
        }

        /// <summary>
        /// Trim every string in an array
        /// </summary>
        /// <param name="strings"></param>
        /// <returns></returns>
        public static string[] Trim(this string[] strings)
        {
            return strings.Select(s => s.Trim()).ToArray();
        }

        /// <summary>
        /// Replace all occurrences of a string from a list of strings to replace
        /// </summary>
        /// <param name="str"></param>
        /// <param name="strings"></param>
        /// <param name="replacementStr"></param>
        /// <returns></returns>
        public static string ReplaceAll(this string str, IEnumerable<string> strings, string replacementStr = "")
        {
            if (string.IsNullOrEmpty(str))
                return str;
            foreach(var removeStr in strings)
            {
                str = str.Replace(removeStr, replacementStr);
            }
            return str;
        }

        /// <summary>
        /// Replace string value at a given position
        /// </summary>
        /// <param name="text"></param>
        /// <param name="position">The position to replace at</param>
        /// <param name="replacement">The string to replace it with</param>
        /// <returns></returns>
        public static string ReplaceAt(this string text, int position, string replacement)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < text.Length; i++)
            {
                if (i == position)
                    builder.Append(replacement);
                else
                    builder.Append(text[i]);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Replace string value at a given position
        /// </summary>
        /// <param name="text"></param>
        /// <param name="position">The position to replace at</param>
        /// <param name="replacement">The character to replace it with</param>
        /// <returns></returns>
        public static string ReplaceAt(this string text, int position, char replacement)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < text.Length; i++)
            {
                if (i == position)
                    builder.Append(replacement);
                else
                    builder.Append(text[i]);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Replace first occurrence of a string
        /// </summary>
        /// <param name="text"></param>
        /// <param name="search"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
        public static string ReplaceFirst(this string text, string search, string replace)
        {
            var pos = text.IndexOf(search);
            if (pos == -1) return text;
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        /// <summary>
        /// Replace last occurrence of a string
        /// </summary>
        /// <param name="text"></param>
        /// <param name="search"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
        public static string ReplaceLast(this string text, string search, string replace)
        {
            var pos = text.LastIndexOf(search);
            if (pos == -1) return text;
            return text.Remove(pos, search.Length).Insert(pos, replace);
        }

        /// <summary>
        /// Mask out part of a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Sanitize(this string? text, double percentage = 0.5)
        {
            if (text == null || string.IsNullOrEmpty(text)) return string.Empty;
            if (percentage > 1) percentage = 1;
            if (percentage < 0) percentage = 0;

            var len = (int)(text.Length * percentage);
            return text.Substring(0, len).PadRight(text.Length, '*');
        }
    }
}
