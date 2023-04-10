using System.Runtime.Serialization;

namespace Binner.Model.Common
{
    /// <summary>
    /// Storage provider exception
    /// </summary>
    [Serializable]
    public class StorageProviderException : Exception
    {
        public string ProviderName { get; }

        public StorageProviderException(string providerName)
        {
            ProviderName = providerName;
        }

        public StorageProviderException(string providerName, string? message) : base(message)
        {
            ProviderName = providerName;
        }

        public StorageProviderException(string providerName, string? message, Exception? innerException) : base(message, innerException)
        {
            ProviderName = providerName;
        }

        protected StorageProviderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ProviderName = string.Empty;
        }
    }
}
