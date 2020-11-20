namespace ReportsGenerator.Data.DataSources
{
    /// <summary>
    /// An abstraction for different data sources - file, web-service, database, etc.
    /// </summary>
    public interface IDataSource
    {
        /// <summary>
        /// Gets data source's reader.
        /// </summary>
        /// <returns>An <see cref="IDataSourceReader"/> instance.</returns>
        IDataSourceReader GetReader();
    }
}