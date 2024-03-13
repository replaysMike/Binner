using Binner.Common.Integrations.Models;

namespace Binner.Model.Integrations
{
    public class ApiResponseState
    {
        public bool IsSuccess { get; set; }
        public IApiResponse? Response { get; init; }

        public ApiResponseState(bool isSuccess, IApiResponse? response)
        {
            IsSuccess = isSuccess;
            Response = response;
        }
    }
}
