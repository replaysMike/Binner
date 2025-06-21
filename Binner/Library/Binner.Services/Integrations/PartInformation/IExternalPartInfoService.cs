using Binner.Model;

namespace Binner.Services.Integrations.PartInformation
{
    public interface IExternalPartInfoService
    {
        /// <summary>
        /// Get part information for a part number
        /// </summary>
        /// <param name="inventoryPart">The part record already in inventory (optional)</param>
        /// <param name="partNumber">Part number to get information on</param>
        /// <param name="partType">Known part type (optional)</param>
        /// <param name="mountingType">Known mounting type (optional)</param>
        /// <param name="supplierPartNumbers">Comma delimited list of supplier part numbers (optional)</param>
        /// <returns></returns>
        Task<IServiceResult<PartResults?>> GetPartInformationAsync(Part? inventoryPart, string partNumber, string partType = "", string mountingType = "", string supplierPartNumbers = "");
    }
}