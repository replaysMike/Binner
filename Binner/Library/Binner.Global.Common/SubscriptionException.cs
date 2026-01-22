using System.Runtime.Serialization;

namespace Binner.Global.Common
{
    [Serializable]
    public class SubscriptionException : Exception
    {
        public SubscriptionException()
        {
        }

        public SubscriptionException(string message) : base(message)
        {
        }

        public SubscriptionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SubscriptionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
