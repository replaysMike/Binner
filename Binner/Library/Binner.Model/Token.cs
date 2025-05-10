using Binner.Model.Authentication;

namespace Binner.Model
{
    /// <summary>
    /// A user token
    /// </summary>
    public class Token
    {
        public TokenTypes TokenType { get; set; }
        public string Value { get; set; } = null!;
        public DateTime? DateCreatedUtc { get; set; }
        public DateTime? DateExpiredUtc { get; set; }
    }
}
