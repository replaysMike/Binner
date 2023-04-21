namespace Binner.Model.Authentication
{
    /// <summary>
    /// Hashed password
    /// </summary>
    public class HashedPassword
    {
        private const byte PasswordMarker = 0x01;

        /// <summary>
        /// The password version
        /// </summary>
        public PasswordVersion PasswordVersion { get; private set; } = PasswordVersion.Version1;

        /// <summary>
        /// The hashed password
        /// </summary>
        public byte[] PasswordHash { get; private set; } = null!;

        /// <summary>
        /// The salt used for the hashed password
        /// </summary>
        public byte[] Salt { get; private set; } = null!;

        /// <summary>
        /// The hash iterations value used for PBKDF2
        /// </summary>
        public int HashIterations { get; private set; }

        public HashedPassword(byte[] passwordHash, byte[] salt, int hashIterations) : this(passwordHash, salt, hashIterations, PasswordVersion.Version1)
        {
        }

        public HashedPassword(byte[] passwordHash, byte[] salt, int hashIterations, PasswordVersion passwordVersion)
        {
            PasswordHash = passwordHash;
            Salt = salt;
            HashIterations = hashIterations;
            PasswordVersion = passwordVersion;
        }

        public HashedPassword(string base64EncodedPasswordHash)
        {
            var hashedPassword = FromHashedPassword(base64EncodedPasswordHash);
            Salt = hashedPassword.Salt;
            PasswordHash = hashedPassword.PasswordHash;
            HashIterations = hashedPassword.HashIterations;
        }

        private HashedPassword() { }

        /// <summary>
        /// Get a hashed password from an Base64 encoded password hash
        /// </summary>
        /// <param name="base64EncodedPasswordHash"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static HashedPassword FromHashedPassword(string base64EncodedPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(base64EncodedPasswordHash))
                throw new ArgumentException("Hashed password is empty and not valid.");

            var hashedPassword = new HashedPassword();

            // parse encoded password hash
            var bytes = Convert.FromBase64String(base64EncodedPasswordHash);
            var marker = bytes[0];
            if (marker != PasswordMarker)
                throw new ArgumentException("Provided password hash is of an invalid format.");
            hashedPassword.PasswordVersion = (PasswordVersion)bytes[1];
            switch (hashedPassword.PasswordVersion)
            {
                case PasswordVersion.Version1:
                    ReadPasswordVersion1(bytes, out var hashIterations, out var salt, out var passwordHash);
                    hashedPassword.HashIterations = hashIterations;
                    hashedPassword.Salt = salt;
                    hashedPassword.PasswordHash = passwordHash;
                    break;
                default:
                    throw new ArgumentException("Unknown password version! Possibly invalid format.");
            }
            return hashedPassword;
        }

        private static void ReadPasswordVersion1(byte[] bytes, out int hashIterations, out byte[] salt, out byte[] passwordHash)
        {
            using var stream = new MemoryStream(bytes, 2, bytes.Length - 2);
            using var reader = new BinaryReader(stream);
            var iterations = reader.ReadUInt32();
            var saltSize = reader.ReadUInt16();
            var passwordHashSize = reader.ReadUInt16();
            var saltBytes = reader.ReadBytes(saltSize);
            var passwordHashBytes = reader.ReadBytes(passwordHashSize);
            hashIterations = (int)iterations;
            salt = saltBytes;
            passwordHash = passwordHashBytes;
        }

        /// <summary>
        /// Get the encoded password hash as a Base64 string
        /// </summary>
        /// <returns></returns>
        public string GetBase64EncodedPasswordHash()
        {
            // encode the password
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            // byte 0 = format marker (0x01)
            // byte 1 = password version marker
            // byte 2-5 = iterations count (uint)
            // byte 6-7 = salt size (ushort)
            // byte 8-9 = password hash size (ushort)
            // bytes (salt.Length) = salt
            // bytes (passwordHash.Length) = password hash

            // write the header info
            writer.Write((byte)PasswordMarker);
            writer.Write((byte)PasswordVersion.Version1);
            writer.Write((uint)HashIterations);
            writer.Write((ushort)Salt.Length);
            writer.Write((ushort)PasswordHash.Length);
            // write the data
            writer.Write(Salt);
            writer.Write(PasswordHash);
            var outputBytes = stream.ToArray();
            return Convert.ToBase64String(outputBytes);
        }

        /// <summary>
        /// Get the encoded password hash as a Base64 string
        /// </summary>
        /// <returns></returns>
        public override string ToString() => GetBase64EncodedPasswordHash();

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (obj is string otherString)
            {
                var hp = GetBase64EncodedPasswordHash();
                return hp.Equals(otherString);
            }
            if (obj is HashedPassword otherHashedPassword)
            {
                return otherHashedPassword.Salt.SequenceEqual(Salt)
                    && otherHashedPassword.PasswordHash.SequenceEqual(PasswordHash)
                    && otherHashedPassword.PasswordVersion.Equals(PasswordVersion)
                    && otherHashedPassword.HashIterations.Equals(HashIterations);
            }
            return false;
        }

        public override int GetHashCode() => base.GetHashCode();
    }

    public enum PasswordVersion : byte
    {
        Version1 = 1
    }
}
