using System;

namespace Binner.Common.Integrations
{
    /// <summary>
    /// Api Credential Key
    /// Identifies a set of credentials for multiple api's for an individual user.
    /// </summary>
    public class ApiCredentialKey : IEquatable<ApiCredentialKey>
    {
        public int UserId { get; set; }

        /// <summary>
        /// Create a new credential key
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static ApiCredentialKey Create(int userId)
            => new ApiCredentialKey
            {
                UserId = userId
            };

        public bool Equals(ApiCredentialKey? other)
            => UserId.Equals(other?.UserId);

        public override bool Equals(object? obj)
        {
            var other = obj as ApiCredentialKey;
            if (other == null) return false;
            return Equals(other);
        }

        public override int GetHashCode()
            => UserId.GetHashCode();
    }
}
