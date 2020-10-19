using ReportsGenerator.Utils;
using System.IO;
using ReportsGenerator.Data.Providers;

namespace ReportsGenerator.Data.DataSources
{
    public class JHopkinsDataSource : IDataSource
    {
        private readonly string _location;
        private readonly ILogger _logger;

        public JHopkinsDataSource(string location, ILogger logger)
        {
            _location = location;
            _logger = logger;
        }

        public IDataSourceReader GetReader()
        {
            return new JHopkinsDataSourceReader(
                Directory.EnumerateFiles(_location, "*.csv"),
                new JHopkinsDataProvider(),
                _logger);
        }
    }
}