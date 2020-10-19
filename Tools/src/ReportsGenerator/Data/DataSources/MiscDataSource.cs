using ReportsGenerator.Data.Providers;
using ReportsGenerator.Utils;
using System.IO;

namespace ReportsGenerator.Data.DataSources
{
    public class MiscDataSource : IDataSource
    {
        private readonly string _location;
        private readonly ILogger _logger;

        public MiscDataSource(string location, ILogger logger)
        {
            _location = location;
            _logger = logger;
        }

        public IDataSourceReader GetReader()
        {
            return new MiscDataSourceReader(
                Directory.EnumerateFileSystemEntries(_location),
                new MiscDataProvider(),
                _logger);
        }
    }
}