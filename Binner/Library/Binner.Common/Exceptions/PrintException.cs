using System;
using System.Runtime.Serialization;

namespace Binner.Common
{
    [Serializable]
    public class PrintException : Exception
    {
        public PrintException()
        {
        }

        public PrintException(string message) : base(message)
        {
        }

        public PrintException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PrintException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
