using System.IO;
using ReportsGenerator.Data.Providers;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Data.DataSources
{
    /// <summary>
    /// Represents an abstraction for folder with helper CSV files.
    /// </summary>
    public class MiscDataSource : IDataSource
    {
        private readonly string _location;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiscDataSource"/> class.
        /// </summary>
        /// <param name="location">Path to folder.</param>
        /// <param name="logger">A <see cref="ILogger"/> instance.</param>
        public MiscDataSource(string location, ILogger logger)
        {
            _location = location;
            _logger = logger;
        }

        /// <inheritdoc />
        public IDataSourceReader GetReader()
        {
            return new MiscDataSourceReader(
                Directory.EnumerateFileSystemEntries(_location),
                new MiscDataProvider(),
                _logger);
        }
    }
}