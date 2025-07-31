using Binner.Model;
using Binner.Model.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Binner.Common.Integrations
{
    public interface IPartInformationProvider
    {
        /// <summary>
        /// Get part information from external api (without user context)
        /// </summary>
        /// <param name="integrationConfiguration">Integration configuration to use for the apis</param>
        /// <param name="partNumber">Part number to fetch info on</param>
        /// <param name="partType"></param>
        /// <param name="mountingType"></param>
        /// <param name="supplierPartNumbers">Comma delimited list of known supplier part numbers</param>
        /// <param name="partTypes"></param>
        /// <param name="inventoryPart"></param>
        /// <param name="maxResults">Max number of results to fetch</param>
        /// <returns></returns>
        Task<PartInformationResults> FetchPartInformationAsync(IntegrationConfiguration integrationConfiguration, string partNumber, string partType, string mountingType, string supplierPartNumbers, ICollection<PartType> partTypes, Part? inventoryPart, int maxResults = 50);

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
        /// <param name="maxResults">Max number of results to fetch</param>
        /// <returns></returns>
        Task<PartInformationResults> FetchPartInformationAsync(string partNumber, string partType, string mountingType, string supplierPartNumbers, int userId, ICollection<PartType> partTypes, Part? inventoryPart, int maxResults = 50);
    }
}