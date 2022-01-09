using System;
using System.Runtime.Serialization;

namespace Binner.Common
{
    [Serializable]
    public class CupsException : PrintException
    {
        public CupsException()
        {
        }

        public CupsException(string message) : base(message)
        {
        }

        public CupsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CupsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
