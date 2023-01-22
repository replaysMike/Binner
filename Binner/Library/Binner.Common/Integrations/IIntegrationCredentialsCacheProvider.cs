namespace Binner.Common.Integrations
{
    /// <summary>
    /// Provides an IntegrationCredentialsCache
    /// </summary>
    public interface IIntegrationCredentialsCacheProvider
    {
        /// <summary>
        /// Get the api credentials cache instance
        /// </summary>
        IntegrationCredentialsCache Cache { get; }
    }
}
