namespace Binner.Common.Models
{
    public interface IPreventDuplicateResource
    {
        /// <summary>
        /// True to allow adding of a duplicate resource
        /// </summary>
        bool AllowPotentialDuplicate { get; }
    }
}
