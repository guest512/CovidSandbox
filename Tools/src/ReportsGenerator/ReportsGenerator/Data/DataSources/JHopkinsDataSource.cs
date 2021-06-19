using System.IO;
using ReportsGenerator.Data.Providers;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Data.DataSources
{
    /// <summary>
    /// Represents an abstraction for folder with CSV files from the John Hopkins University.
    /// </summary>
    public class JHopkinsDataSource : IDataSource
    {
        private readonly string _location;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JHopkinsDataSource"/> class.
        /// </summary>
        /// <param name="location">Path to folder.</param>
        /// <param name="logger">A <see cref="ILogger"/> instance.</param>
        public JHopkinsDataSource(string location, ILogger logger)
        {
            _location = location;
            _logger = logger;
        }

        /// <inheritdoc />
        public IDataSourceReader GetReader() => new JHopkinsDataSourceReader(
            Directory.EnumerateFiles(_location, "*.csv"), 
            new JHopkinsDataProvider(), 
            _logger);
    }
}