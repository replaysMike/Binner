namespace Binner.Model.Swarm
{
    public enum DataSource
    {
        /// <summary>
        /// Uri was manually entered by an admin
        /// </summary>
        ManualInput = 0,
        /// <summary>
        /// Uri found by the crawler
        /// </summary>
        Crawler,
        /// <summary>
        /// Uri submitted by the public api
        /// </summary>
        SwarmApi,
    }
}
