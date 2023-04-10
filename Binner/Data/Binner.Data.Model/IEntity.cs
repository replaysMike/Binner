namespace Binner.Data.Model
{
    public interface IEntity
    {
        /// <summary>
        /// Creation date
        /// </summary>
        DateTime DateCreatedUtc { get; set; }

        /// <summary>
        /// Modified date
        /// </summary>
        DateTime DateModifiedUtc { get; set; }
    }
}
