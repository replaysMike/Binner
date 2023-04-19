using System;
using System.Net;

namespace Binner.Legacy.Extensions
{
    public static class IpAddressExtensions
    {
        /// <summary>
        /// Get an IPAddress as 64 bit integer
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static long ToLong(this IPAddress ipAddress)
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

        /// <summary>
        /// Get an IPAddress from a 64 bit integer
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static IPAddress FromLong(long ipAddress)
            => ipAddress.ToIpAddress();

        /// <summary>
        /// Get an IPAddress from a 64 bit integer
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static IPAddress ToIpAddress(this long ipAddress)
        {
            try
            {
                var ipAddressStr = ipAddress.ToString();
                if (IPAddress.TryParse(ipAddressStr, out var ip))
                    return ip;
            }
            catch (Exception)
            {
            }
            return IPAddress.None;
        }
    }
}
