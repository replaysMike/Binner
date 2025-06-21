using Binner.Model;

namespace Binner.Services.Integrations
{
    public interface IBaseIntegrationBehavior
    {
        Task<List<CommonPart>> MapCommonPartIdsAsync(List<CommonPart> parts);
        ICollection<string> DetermineKeywordsFromPart(CommonPart part, ICollection<PartType> partTypes);
        Task<PartType?> DeterminePartTypeAsync(CommonPart part);
        IDictionary<PartType, int> GetMatchingPartTypes(CommonPart part, ICollection<PartType> partTypes);
    }
}
