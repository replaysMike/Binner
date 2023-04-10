using System.Linq.Expressions;

namespace Binner.StorageProvider.EntityFrameworkCore
{
    internal static class QueryableExtensions
    {
        /// <summary>
        /// Filters a sequence of values based on a predicate if a condition is met.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="condition"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        internal static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, bool condition, Expression<Func<TSource, bool>> predicate)
            where TSource : class
        {
            if (condition)
                return source.Where(predicate);
            else
                return source;
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate if a condition is met.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="condition"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        internal static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, bool condition, Expression<Func<TSource, int, bool>> predicate)
            where TSource : class
        {
            if (condition)
                return source.Where(predicate);
            else
                return source;
        }
    }
}
