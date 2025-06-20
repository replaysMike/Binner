namespace Binner.Model.Responses
{
    public class PasswordRecoveryResponse
    {
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }

        public PasswordRecoveryResponse() { }

        public PasswordRecoveryResponse(bool isSuccessful, string? errorMessage)
        {
            IsSuccessful = isSuccessful;
            ErrorMessage = errorMessage;
        }
    }
}
