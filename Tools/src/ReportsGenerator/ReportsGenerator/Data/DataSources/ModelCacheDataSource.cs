using System.Collections.Generic;
using System.IO;
using ReportsGenerator.Data.Providers;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Data.DataSources
{
    /// <summary>
    /// Represents an abstraction for folder with cached data model CSV files.
    /// </summary>
    public class ModelCacheDataSource : IDataSource
    {
        private readonly ILogger _logger;
        private readonly string _location;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelCacheDataSource"/> class.
        /// </summary>
        /// <param name="location">Path to folder.</param>
        /// <param name="logger">A <see cref="ILogger"/> instance.</param>
        public ModelCacheDataSource(string location, ILogger logger)
        {
            _location = location;
            _logger = logger;
        }

        /// <inheritdoc />
        public IDataSourceReader GetReader()
        {
            var files = new List<string>();

            if (Directory.Exists(_location))
            {
                files.AddRange(Directory.EnumerateFiles(_location));
            }

            return new ModelCacheDataSourceReader(files, new ModelCacheDataProvider(), _logger);
        }
    }
}