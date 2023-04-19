namespace Binner.Model.Requests
{
    public interface IPreventDuplicateResource
    {
        /// <summary>
        /// True to allow adding of a duplicate resource
        /// </summary>
        bool AllowPotentialDuplicate { get; }
    }
}
