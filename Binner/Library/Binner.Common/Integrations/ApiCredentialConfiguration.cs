using System.Collections.Generic;

namespace Binner.Common.Integrations
{
    public class ApiCredentialConfiguration
    {
        public int UserId { get; }

        public List<ApiCredential> ApiCredentials { get; }

        public ApiCredentialConfiguration(int userId, List<ApiCredential> apiCredentials)
        {
            UserId = userId;
            ApiCredentials = apiCredentials;
        }
    }
}
