namespace Binner.Common.Extensions
{
    public static class StringExtensions
    {
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
    }
}
