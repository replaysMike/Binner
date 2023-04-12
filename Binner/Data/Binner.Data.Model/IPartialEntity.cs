namespace Binner.Data.Model
{
    public interface IPartialEntity
    {
        /// <summary>
        /// Creation date
        /// </summary>
        DateTime DateCreatedUtc { get; set; }
    }
}
