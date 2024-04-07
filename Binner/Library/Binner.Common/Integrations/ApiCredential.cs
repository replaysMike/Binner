using System.Collections.Generic;

namespace Binner.Common.Integrations
{
    /// <summary>
    /// An Api credential
    /// </summary>
    public sealed class ApiCredential
    {
        /// <summary>
        /// The user id that owns the credential
        /// </summary>
        public int UserId { get; }

        /// <summary>
        /// The name of the api
        /// </summary>
        public string? ApiName { get; internal set; }

        /// <summary>
        /// The key/value mapping of credential values
        /// </summary>
        public IDictionary<string, object> Credentials { get; }

        /// <summary>
        /// Create an Api Credential
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="credentials">Key/value mappings of credential values</param>
        /// <param name="apiName">Name of api to store the credentials as</param>
        public ApiCredential(int userId, IDictionary<string, object> credentials, string? apiName)
        {
            UserId = userId;
            Credentials = credentials;
            ApiName = apiName;
        }

        /// <summary>
        /// Get a stored Api credential
        /// </summary>
        /// <param name="key">Credential key name</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public string GetCredentialString(string key)
        {
            if (Credentials.ContainsKey(key))
                return Credentials[key]?.ToString() ?? string.Empty;
            throw new KeyNotFoundException(key);
        }

        /// <summary>
        /// Get a stored Api credential if it exists, null if not found
        /// </summary>
        /// <param name="key">Credential key name</param>
        /// <returns></returns>
        public string? TryGetCredentialString(string key)
        {
            if (Credentials.ContainsKey(key))
                return Credentials[key]?.ToString() ?? string.Empty;
            return null;
        }

        /// <summary>
        /// Get a stored Api credential
        /// </summary>
        /// <param name="key">Credential key name</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public bool GetCredentialBool(string key)
        {
            if (Credentials.ContainsKey(key))
            {
                var val = Credentials[key]?.ToString();
                if (bool.TryParse(val, out var result))
                    return result;
            }
            throw new KeyNotFoundException(key);
        }

        /// <summary>
        /// Get a stored Api credential, null if not found
        /// </summary>
        /// <param name="key">Credential key name</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public bool? TryGetCredentialBool(string key)
        {
            if (Credentials.ContainsKey(key))
            {
                var val = Credentials[key]?.ToString() ?? "false";
                if (bool.TryParse(val, out var result))
                    return result;
                return null;
            }
            return null;
        }

        /// <summary>
        /// Get a stored Api credential
        /// </summary>
        /// <param name="key">Credential key name</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public int GetCredentialInt32(string key)
        {
            if (Credentials.ContainsKey(key))
            {
                var val = Credentials[key]?.ToString();
                if (int.TryParse(val, out var result))
                    return result;
            }
            throw new KeyNotFoundException(key);
        }

        /// <summary>
        /// Get a stored Api credential, null if not found
        /// </summary>
        /// <param name="key">Credential key name</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public int? TryGetCredentialInt32(string key)
        {
            if (Credentials.ContainsKey(key))
            {
                var val = Credentials[key]?.ToString() ?? "0";
                if (int.TryParse(val, out var result))
                    return result;
                return null;
            }
            return null;
        }
    }
}
