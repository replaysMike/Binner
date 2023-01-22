namespace Binner.Common.Models
{
    public class Keyword
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int KeywordId { get; set; }

        /// <summary>
        /// Keyword name
        /// </summary>
        public string Name { get; set; } = null!;
    }
}
