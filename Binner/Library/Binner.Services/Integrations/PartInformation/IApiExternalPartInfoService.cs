using Binner.Model;

namespace Binner.Services.Integrations.PartInformation
{
    public interface IApiExternalPartInfoService
    {
        Task<IServiceResult<PartResults?>> GetPartInformationAsync(string partNumber, string partType = "", string mountingType = "", string supplierPartNumbers = "");
    }
}
