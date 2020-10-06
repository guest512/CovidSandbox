using System.Collections.Generic;
using System.IO;
using ReportsGenerator.Data.DataSources.Providers;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Data.DataSources
{
    public class YandexRussiaDataSource : IDataSource
    {
        private readonly string _location;
        private readonly ILogger _logger;
        private readonly Dictionary<RowVersion, IDataProvider> _dataProviders;

        public YandexRussiaDataSource(string location, ILogger logger)
        {
            _location = location;
            _logger = logger;
            _dataProviders = new Dictionary<RowVersion, IDataProvider>
            {
                {RowVersion.YandexRussia, new YandexRussiaDataProvider()}
            };
        }

        public IDataSourceReader GetReader()
        {
            return new YandexRussiaDataSourceReader(
                Path.Combine(_location, "Russia.csv"),
                _dataProviders, _logger);
        }
    }
}