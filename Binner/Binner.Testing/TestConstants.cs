using Binner.Global.Common;

namespace Binner.Testing
{
    public static class TestConstants
    {
        public const int UserId = 1;
        public const int OrganizationId = 1;
        public const string UserName = "testuser";
        public const string Password = "password";

        public const string OrderId = "1-TEST-ND";
        public const string MouserSupplier = "mouser";

        public static readonly UserContext UserContext = new UserContext()
        {
            UserId = UserId,
            OrganizationId = OrganizationId,
            CanLogin = true,
            Name = "Test User",
            EmailAddress = "test@example.com",
            IsAdmin = true
        };
    }
}
