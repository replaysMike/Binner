using Binner.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binner.Common.Integrations
{
    public interface IPartInformationProvider
    {
        /// <summary>
        /// Get part information from external api
        /// </summary>
        /// <param name="partNumber">Part number to fetch info on</param>
        /// <param name="partType"></param>
        /// <param name="mountingType"></param>
        /// <param name="supplierPartNumbers">Comma delimited list of known supplier part numbers</param>
        /// <param name="userId"></param>
        /// <param name="partTypes"></param>
        /// <param name="inventoryPart"></param>
        /// <returns></returns>
        Task<PartInformationResults> FetchPartInformationAsync(string partNumber, string partType, string mountingType, string supplierPartNumbers, int userId, ICollection<PartType> partTypes, Part? inventoryPart);
    }
}