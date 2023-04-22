using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace Binner.Global.Common
{
    /// <summary>
    /// Provides the password hashing implementation
    /// </summary>
    public static class PasswordHasher
    {
        /// <summary>
        /// The iteration level of the PBKDF2 algorithm
        /// </summary>
        private const int IterationCount = 10000;

        /// <summary>
        /// 256 bit key size
        /// </summary>
        private const int KeySize = 256;

        /// <summary>
        /// 128 bit salt size
        /// </summary>
        private const int SaltKeySize = 128;

        /// <summary>
        /// Generate a password hash
        /// </summary>
        /// <param name="password">Password to hash</param>
        /// <returns></returns>
        public static HashedPassword GeneratePasswordHash(string password)
        {
            var salt = GenerateSalt();
            var passwordHash = KeyDerivation.Pbkdf2(
                    password,
                    salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: IterationCount,
                    numBytesRequested: KeySize / 8);
            return new HashedPassword(passwordHash, salt, IterationCount);
        }

        /// <summary>
        /// Generate a password hash
        /// </summary>
        /// <param name="password">Password to hash</param>
        /// <param name="salt">Salt to use</param>
        /// <param name="hashIterations">Hash iterations to use</param>
        /// <returns></returns>
        public static HashedPassword GeneratePasswordHash(string password, byte[] salt, int hashIterations)
        {
            var passwordHash = KeyDerivation.Pbkdf2(
                    password,
                    salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: hashIterations,
                    numBytesRequested: KeySize / 8);
            return new HashedPassword(passwordHash, salt, hashIterations);
        }

        /// <summary>
        /// Verify a password credential
        /// </summary>
        /// <param name="password"></param>
        /// <param name="hashedPassword"></param>
        /// <returns>True if credentials match</returns>
        public static bool Verify(string password, string hashedPassword)
        {
            var existingCredential = HashedPassword.FromHashedPassword(hashedPassword);
            return Verify(password, existingCredential);
        }

        /// <summary>
        /// Verify a password credential
        /// </summary>
        /// <param name="password"></param>
        /// <param name="hashedPassword"></param>
        /// <returns>True if credentials match</returns>
        public static bool Verify(string password, HashedPassword hashedPassword)
        {
            var credentialToTest = GeneratePasswordHash(password, hashedPassword.Salt, hashedPassword.HashIterations);
            return credentialToTest.Equals(hashedPassword);
        }

        /// <summary>
        /// Generate a random salt
        /// </summary>
        /// <returns></returns>
        private static byte[] GenerateSalt()
            => RandomNumberGenerator.GetBytes(SaltKeySize / 8);
    }
}
