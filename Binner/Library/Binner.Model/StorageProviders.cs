namespace Binner.Model
{
    public enum StorageProviders
    {
        /// <summary>
        /// Sqlite (alias)
        /// </summary>
        Binner,
        /// <summary>
        /// Sqlite
        /// </summary>
        Sqlite,
        /// <summary>
        /// MS Sql Server
        /// </summary>
        SqlServer,
        /// <summary>
        /// Postgresql
        /// </summary>
        Postgresql,
        /// <summary>
        /// MySql / MariaDb
        /// </summary>
        MySql,
        /// <summary>
        /// In-memory database, not persistant used for testing only
        /// </summary>
        InMemory
    }
}
