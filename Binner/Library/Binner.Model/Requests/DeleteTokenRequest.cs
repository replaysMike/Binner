using Binner.Model.Authentication;

namespace Binner.Model.Requests
{
    public class DeleteTokenRequest
    {
        public TokenTypes TokenType { get; set; }
        public string Value { get; set; } = null!;
    }
}
