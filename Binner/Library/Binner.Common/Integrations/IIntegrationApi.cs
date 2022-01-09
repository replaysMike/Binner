namespace Binner.Common.Integrations
{
    /// <summary>
    /// An integrated Api
    /// </summary>
    public interface IIntegrationApi
    {
        /// <summary>
        /// True if the Api is configured/enabled
        /// </summary>
        bool IsConfigured { get; }
    }
}
