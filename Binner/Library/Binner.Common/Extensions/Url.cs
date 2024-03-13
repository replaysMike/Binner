using System;
using System.Reflection;

namespace Binner.Common.Extensions
{
    public static class Url
    {
        /// <summary>
        /// Create a Uri
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encodeUri">True to allow url encode</param>
        /// <returns></returns>
        public static Uri Create(string url, bool encodeUri = true)
        {
            var uri = new Uri(url);
            if (!encodeUri) ForceCanonicalPathAndQuery(uri);
            return uri;
        }

        /// <summary>
        /// Combine multiple paths and create Uri
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static Uri Combine(params string[] uriParts)
        {
            return Combine(true, uriParts);
        }

        /// <summary>
        /// Combine multiple paths and create Uri
        /// </summary>
        /// <param name="encodeUri">True to allow url encode</param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static Uri Combine(bool encodeUri, params string[] uriParts)
        {
            string uri = string.Empty;
            if (uriParts != null && uriParts.Length > 0)
            {
                char[] trims = new char[] { '\\', '/' };
                uri = (uriParts[0] ?? string.Empty).TrimEnd(trims);
                for (int i = 1; i < uriParts.Length; i++)
                {
                    uri = string.Format("{0}/{1}", uri.TrimEnd(trims), (uriParts[i] ?? string.Empty).TrimStart(trims));
                }
            }
            return Create(uri, encodeUri);
        }

        /// <summary>
        /// This is a hack to force Uri class not to encode, since it always does
        /// </summary>
        /// <param name="uri"></param>
        private static void ForceCanonicalPathAndQuery(Uri uri)
        {
            var paq = uri.PathAndQuery; // need to access PathAndQuery
            var flagsFieldInfo = typeof(Uri).GetField("_flags", BindingFlags.Instance | BindingFlags.NonPublic);
            var flags = (ulong?)flagsFieldInfo?.GetValue(uri);
            var flagsBefore = flags;
            flags &= ~((ulong)0xC30); // Flags.PathNotCanonical|Flags.QueryNotCanonical|Flags.E_QueryNotCanonical|Flags.E_PathNotCanonical
            flagsFieldInfo?.SetValue(uri, flags);
        }
    }
}
