using ReportsGenerator.Utils;
using System.IO;
using ReportsGenerator.Data.Providers;

namespace ReportsGenerator.Data.DataSources
{
    /// <summary>
    /// Represents an abstraction for folder with CSV files from the Yandex Dashboard.
    /// </summary>
    public class YandexRussiaDataSource : IDataSource
    {
        private readonly string _location;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="YandexRussiaDataSource"/> class.
        /// </summary>
        /// <param name="location">Path to folder.</param>
        /// <param name="logger">A <see cref="ILogger"/> instance.</param>
        public YandexRussiaDataSource(string location, ILogger logger)
        {
            _location = location;
            _logger = logger;
        }

        /// <inheritdoc />
        public IDataSourceReader GetReader()
        {
            return new YandexRussiaDataSourceReader(
                Path.Combine(_location, "Russia.csv"),
                new YandexRussiaDataProvider(),
                _logger);
        }
    }
}