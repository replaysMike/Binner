using System;
using System.Collections.Generic;
using System.Text;

namespace Binner.Web.Configuration
{
    /// <summary>
    /// Service Configuration
    /// </summary>
    public class WebHostServiceConfiguration
    {
        /// <summary>
        /// The server name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The server ip to bind to
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// The port number to host
        /// </summary>
        public int Port { get; set; } = 4433;

        /// <summary>
        /// Maximum number of items to cache
        /// </summary>
        public int MaxCacheItems { get; set; } = 1024;

        /// <summary>
        /// The number of minutes to set the sliding expiration cache to (default: 30)
        /// </summary>
        public int CacheSlidingExpirationMinutes { get; set; } = 30;

        /// <summary>
        /// The number of minutes to set the absolute expiration to (default: 0)
        /// </summary>
        public int CacheAbsoluteExpirationMinutes { get; set; } = 0;

        /// <summary>
        /// The origin to allow for Cors
        /// </summary>
        public string CorsAllowOrigin { get; set; }
    }
}
