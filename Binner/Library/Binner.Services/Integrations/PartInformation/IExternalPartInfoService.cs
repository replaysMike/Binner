using Binner.Model;
using Binner.Model.Configuration;

namespace Binner.Services.Integrations.PartInformation
{
    public interface IExternalPartInfoService
    {
        /// <summary>
        /// Get part information for a part number, in a global context (no user available)
        /// </summary>
        /// <param name="integrationConfiguration">Integration configuration to use for the apis</param>
        /// <param name="inventoryPart">The part record already in inventory (optional)</param>
        /// <param name="partNumber">Part number to get information on</param>
        /// <param name="partType">Known part type (optional)</param>
        /// <param name="mountingType">Known mounting type (optional)</param>
        /// <param name="supplierPartNumbers">Comma delimited list of supplier part numbers (optional)</param>
        /// <param name="maxResults">Max number of results to fetch per api</param>
        /// <returns></returns>
        Task<IServiceResult<PartResults?>> GetGlobalPartInformationAsync(IntegrationConfiguration integrationConfiguration, Part? inventoryPart, string partNumber, string partType = "", string mountingType = "", string supplierPartNumbers = "", int maxResults = ApiConstants.MaxRecords);

        /// <summary>
        /// Get part information for a part number using the user context
        /// </summary>
        /// <param name="inventoryPart">The part record already in inventory (optional)</param>
        /// <param name="partNumber">Part number to get information on</param>
        /// <param name="partType">Known part type (optional)</param>
        /// <param name="mountingType">Known mounting type (optional)</param>
        /// <param name="supplierPartNumbers">Comma delimited list of supplier part numbers (optional)</param>
        /// <param name="maxResults">Max number of results to fetch per api</param>
        /// <returns></returns>
        Task<IServiceResult<PartResults?>> GetPartInformationAsync(Part? inventoryPart, string partNumber, string partType = "", string mountingType = "", string supplierPartNumbers = "", int maxResults = ApiConstants.MaxRecords);
    }
}