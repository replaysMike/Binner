using Binner.Model;

namespace Binner.Services
{
    public interface IPartTypeDetection : IPartTypeDetection<CommonPart>
    {
    }

    public interface IPartTypeDetection<TPart>
    {
        /// <summary>
        /// Determine the PartType for a CommonPart
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        Task<PartType?> DeterminePartTypeAsync(TPart part);

        /// <summary>
        /// Get all matching part types for a CommonPart with a ranking for each match
        /// </summary>
        /// <param name="part"></param>
        /// <param name="partTypes"></param>
        /// <returns></returns>
        IDictionary<PartType, int> GetMatchingPartTypes(TPart part, ICollection<PartType> partTypes);

        /// <summary>
        /// Get part type information for a system part type
        /// </summary>
        /// <param name="partTypeEnum"></param>
        /// <returns></returns>
        PartTypeInfoAttribute? GetPartTypeInfo(SystemDefaults.DefaultPartTypes? partTypeEnum);

        /// <summary>
        /// Remove plurals from a word to improve matching (e.g. "capacitors" -> "capacitor")
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        string? RemovePlurals(string? input);
    }
}