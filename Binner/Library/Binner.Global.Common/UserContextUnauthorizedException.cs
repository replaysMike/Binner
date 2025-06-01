using System;
using System.Runtime.CompilerServices;

namespace Binner.Global.Common
{
    public class UserContextUnauthorizedException : Exception
    {
        const string DefaultMessage = "Action requires valid user context.";
        public string Method { get; }

        public int LineNumber { get; }

        public override string Message => $"{base.Message}. Caller: {Method}:{LineNumber}";

        public UserContextUnauthorizedException([CallerMemberName] string callerName = "", [CallerLineNumber] int callerLineNumber = 0) : base(DefaultMessage)
        {
            Method = callerName;
            LineNumber = callerLineNumber;
        }

        public UserContextUnauthorizedException(string message, [CallerMemberName] string callerName = "", [CallerLineNumber] int callerLineNumber = 0) : base(message)
        {
            Method = callerName;
            LineNumber = callerLineNumber;
        }

        public UserContextUnauthorizedException(Exception exception, [CallerMemberName] string callerName = "", [CallerLineNumber] int callerLineNumber = 0) : base(DefaultMessage, exception)
        {
            Method = callerName;
            LineNumber = callerLineNumber;
        }
    }
}
