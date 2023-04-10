namespace Binner.Model.Authentication
{
    /// <summary>
    /// Jwt user tokens (MS)
    /// </summary>
    public class UserTokens
    {
        public Guid ClaimsId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Name { get;set;} = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public TimeSpan DateExpiredUtc { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public long UserId { get; set; }
        public DateTime ExpiredTime { get; set; }
    }
}
