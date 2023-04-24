using Binner.Global.Common;
using System;
using System.IO;
using System.Reflection;

namespace Binner.Common.Auth
{
    public class SecurityKeyProvider
    {
        public enum KeyTypes
        {
            Jwt
        }

        private string GetTokenFilename(KeyTypes keyType)
        {
            var keyTypeName = "";
            switch (keyType)
            {
                case KeyTypes.Jwt:
                    keyTypeName = "jwt.key";
                    break;
                default:
                    throw new NotImplementedException($"Invalid key type: '{keyType}'");
            }
            var assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                var path = Path.GetDirectoryName(assembly.Location);
                if (string.IsNullOrEmpty(path))
                    throw new BinnerConfigurationException("Failed to get assembly path!");
                var filename = Path.Combine(path, keyTypeName);
                return filename;
            }
            throw new BinnerConfigurationException("Error: Could not get entry assembly information to store the JWT key!");
        }

        /// <summary>
        /// Load the key from disk
        /// </summary>
        /// <param name="keyType"></param>
        /// <param name="length"></param>
        /// <returns>The key or null if it does not exist</returns>
        public string LoadOrGenerateKey(KeyTypes keyType, int length)
        {
            var filename = GetTokenFilename(keyType);
            if (File.Exists(filename))
            {
                var key = File.ReadAllText(filename);
                return key;
            }

            return CreateKey(filename);
        }

        /// <summary>
        /// Create a new key and save to disk
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>The new key</returns>
        public string CreateKey(string filename)
        {
            var key = GenerateSecurityKey();
            File.WriteAllText(filename, key);
            return key;
        }

        private string GenerateSecurityKey() => ConfirmationTokenGenerator.NewSecurityToken(40);
    }
}
