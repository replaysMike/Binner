namespace Binner.Common.Models
{
    public interface ISortable
    {
        /// <summary>
        /// Property to order by
        /// </summary>
        string OrderBy { get; set; }

        /// <summary>
        /// Direction to sort
        /// </summary>
        SortDirection Direction { get; set; }
    }

    public enum SortDirection
    {
        Ascending,
        Descending
    }

}
