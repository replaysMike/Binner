using System;
using System.Collections.Generic;
using System.Text;

namespace Binner.Common.Models
{
    public interface IServiceResult<T>
    {
        /// <summary>
        /// Response object
        /// </summary>
        T Response { get; set; }

        /// <summary>
        /// Name of api
        /// </summary>
        string ApiName { get; set; }

        /// <summary>
        /// Requires authentication to continue
        /// </summary>
        bool RequiresAuthentication { get; set; }

        /// <summary>
        /// A redirect Url location if requires authentication
        /// </summary>
        string RedirectUrl { get; set; }

        /// <summary>
        /// Errors
        /// </summary>
        IEnumerable<string> Errors { get; set; }
    }
}
