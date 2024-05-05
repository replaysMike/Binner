using System;
using System.Net;

namespace Binner.Common.Extensions
{
    public static class IpAddressExtensions
    {
        /// <summary>
        /// Get an IPAddress as 32 bit integer
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static int ToInt(this IPAddress ipAddress)
        {
            try
            {
                return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(ipAddress.GetAddressBytes(), 0));
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Get an IPAddress as 32 bit integer
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static uint ToUInt(this IPAddress ipAddress)
        {
            try
            {
                return (uint)ToInt(ipAddress);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Get an IPAddress as 64 bit integer
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static long ToLong(this IPAddress ipAddress)
        {
            try
            {
                return IPAddress.NetworkToHostOrder(BitConverter.ToInt64(ipAddress.GetAddressBytes(), 0));
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Get an IPAddress as 64 bit integer
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static ulong ToULong(this IPAddress ipAddress)
        {
            try
            {
                return (ulong)ToLong(ipAddress);
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
        /// Get an IPAddress from a 32 bit integer
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static IPAddress FromInt(int ipAddress)
        {
            try
            {
                return new IPAddress(IPAddress.NetworkToHostOrder(ipAddress));
            }
            catch (Exception)
            {
            }
            return IPAddress.None;
        }

        /// <summary>
        /// Get an IPAddress from a 32 bit integer
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static IPAddress ToIpAddress(this int ipAddress)
        {
            try
            {
                return new IPAddress(IPAddress.NetworkToHostOrder(ipAddress));
            }
            catch (Exception)
            {
            }
            return IPAddress.None;
        }

        /// <summary>
        /// Get an IPAddress from a 32 bit integer
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static IPAddress ToIpAddress(this uint ipAddress)
        {
            try
            {
                return new IPAddress(IPAddress.NetworkToHostOrder((int)ipAddress));
            }
            catch (Exception)
            {
            }
            return IPAddress.None;
        }

        /// <summary>
        /// Get an IPAddress from a 32 bit integer
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static IPAddress ToIpAddress(this long ipAddress)
        {
            try
            {
                return new IPAddress(IPAddress.NetworkToHostOrder(ipAddress));
            }
            catch (Exception)
            {
            }
            return IPAddress.None;
        }

        /// <summary>
        /// Get an IPAddress from a 64 bit integer
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static IPAddress ToIpAddress(this ulong ipAddress)
        {
            try
            {
                return new IPAddress(IPAddress.NetworkToHostOrder((long)ipAddress));
            }
            catch (Exception)
            {
            }
            return IPAddress.None;
        }
    }
}
