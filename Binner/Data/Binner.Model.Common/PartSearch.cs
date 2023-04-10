namespace Binner.Model.Common
{
    /// <summary>
    /// A ranked search result
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PartSearch : Part
    {
        /// <summary>
        /// The rank of the result
        /// </summary>
        public int Rank { get; }
    }
}
