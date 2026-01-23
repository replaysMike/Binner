using System.Net;

namespace Binner.Global.Common.Extensions
{
    internal static class IpAddressExtensions
    {
        /// <summary>
        /// Get an IPAddress as 64 bit integer
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        internal static long ToLong(this IPAddress ipAddress)
        {
            try
            {
                var ipLong = BitConverter.ToInt32(ipAddress.GetAddressBytes(), 0);
                return IPAddress.NetworkToHostOrder(ipLong);
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
