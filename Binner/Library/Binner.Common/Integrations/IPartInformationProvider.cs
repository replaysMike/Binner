using Binner.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binner.Common.Integrations
{
    public interface IPartInformationProvider
    {
        Task<PartInformationResults> FetchPartInformationAsync(string partNumber, string partType, string mountingType, string supplierPartNumbers, int userId, ICollection<PartType> partTypes, Part? inventoryPart);
    }
}