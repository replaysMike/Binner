using System.Security.Cryptography;

namespace Binner.Global.Common
{
    /// <summary>
    /// Generates confirmation tokens
    /// </summary>
    public static class TokenGenerator
    {
        /// <summary>
        /// Generate a new token
        /// </summary>
        /// <returns></returns>
        public static string NewToken() 
            // note: don't allow / or +, which are valid base64 chars, as these are used in URLs without encoding
            => Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("/", "M").Replace("+", "b").Replace("==", "");

        /// <summary>
        /// Generate a new token
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string NewSecurityToken(int length)
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            return Convert.ToBase64String(bytes).Replace("/", "M").Replace("+", "b").Replace("==", "").Substring(0, length);
        }
    }
}
