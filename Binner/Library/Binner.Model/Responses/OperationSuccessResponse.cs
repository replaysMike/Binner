namespace Binner.Model.Responses
{
    public class OperationSuccessResponse
    {
        public string ResponseMessage { get; } = "OK";

        public OperationSuccessResponse() { }

        public OperationSuccessResponse(string responseMessage)
        {
            ResponseMessage = responseMessage;
        }
    }
}
