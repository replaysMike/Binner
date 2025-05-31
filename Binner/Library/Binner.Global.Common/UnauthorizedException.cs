using System;

namespace Binner.Global.Common
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException() : base("User is not logged in")
        {

        }
        public UnauthorizedException(string message) : base(message)
        {
        }
    }
}
