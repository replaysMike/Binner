using System;

namespace Binner.Common.Services.Authentication
{
    /// <summary>
    /// Generates confirmation tokens
    /// </summary>
    public static class ConfirmationTokenGenerator
    {
        /// <summary>
        /// Generate a new token
        /// </summary>
        /// <returns></returns>
        public static string NewToken() 
            // note: don't allow / or +, which are valid base64 chars, as these are used in URLs without encoding
            => Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("/", "M").Replace("+", "b");
    }
}
