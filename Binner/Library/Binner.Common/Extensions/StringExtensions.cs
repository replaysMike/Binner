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
