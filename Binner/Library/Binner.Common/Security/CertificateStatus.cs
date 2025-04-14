using System;

namespace Binner.Common.Security
{
    /// <summary>
    /// Status of certificate registration
    /// </summary>
    [Flags]
    public enum CertificateState
    {
        None,
        Created,
        Registered,
        Error
    }
}
