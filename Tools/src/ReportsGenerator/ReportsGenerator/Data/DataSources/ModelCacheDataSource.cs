using System.Collections.Generic;
using System.IO;
using ReportsGenerator.Data.Providers;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Data.DataSources
{
    public class ModelCacheDataSource : IDataSource
    {
        private readonly ILogger _logger;
        private readonly string _location;

        public ModelCacheDataSource(string location, ILogger logger)
        {
            _location = location;
            _logger = logger;
        }

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