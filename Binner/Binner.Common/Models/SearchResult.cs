namespace Binner.Common.Models
{
    /// <summary>
    /// A ranked search result
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SearchResult<T>
    {
        public T Result { get; }
        public int Rank { get; }

        public SearchResult(T result, int rank)
        {
            Result = result;
            Rank = rank;
        }
    }
}
