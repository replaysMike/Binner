namespace Binner.Model.Common
{
    public interface IEntity
    {
        /// <summary>
        /// Creation date
        /// </summary>
        DateTime DateCreatedUtc { get; set; }
    }
}
