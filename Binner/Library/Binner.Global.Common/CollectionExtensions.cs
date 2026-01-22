namespace Binner.Global.Common
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Determines whether a sequence contains any elements. (null safe)
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool SafeAny<TSource>(this IEnumerable<TSource>? source)
        {
            if (source == null) return false;
            return source.Any();
        }
    }
}
