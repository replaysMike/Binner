using System;
using System.Runtime.Serialization;

namespace Binner.Legacy
{
    /// <summary>
    /// Binner configuration exception
    /// </summary>
    [Serializable]
    public class BinnerConfigurationException : Exception
    {
        public BinnerConfigurationException()
        {
        }

        public BinnerConfigurationException(string message) : base(message)
        {
        }

        public BinnerConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BinnerConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
