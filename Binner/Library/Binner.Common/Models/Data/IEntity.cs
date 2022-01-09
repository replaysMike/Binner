using System;

namespace Binner.Common.Models
{
    public interface IEntity
    {
        /// <summary>
        /// Creation date
        /// </summary>
        DateTime DateCreatedUtc { get; set; }
    }
}
