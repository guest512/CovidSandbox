using ReportsGenerator.Data.DataSources.Providers;
using ReportsGenerator.Utils;
using System.IO;

namespace ReportsGenerator.Data.DataSources
{
    public class YandexRussiaDataSource : IDataSource
    {
        private readonly string _location;
        private readonly ILogger _logger;

        public YandexRussiaDataSource(string location, ILogger logger)
        {
            _location = location;
            _logger = logger;
        }

        public IDataSourceReader GetReader()
        {
            return new YandexRussiaDataSourceReader(
                Path.Combine(_location, "Russia.csv"),
                new YandexRussiaDataProvider(),
                _logger);
        }
    }
}