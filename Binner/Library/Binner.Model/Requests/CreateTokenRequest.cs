using Binner.Model.Authentication;

namespace Binner.Model.Requests
{
    public class CreateTokenRequest
    {
        public TokenTypes TokenType { get; set; }
        public string? TokenConfig { get; set; }
    }
}
