using System.Collections.Generic;
using System.IO;
using ReportsGenerator.Data.DataSources.Providers;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Data.DataSources
{
    public class JHopkinsDataSource : IDataSource
    {
        private readonly string _location;
        private readonly ILogger _logger;
        private readonly Dictionary<RowVersion, IDataProvider> _dataProviders;

        public JHopkinsDataSource(string location, ILogger logger)
        {
            _location = location;
            _logger = logger;
            var jHopkinsProvider = new JHopkinsDataProvider();
            _dataProviders = new Dictionary<RowVersion, IDataProvider>
            {
                {RowVersion.JHopkinsV1, jHopkinsProvider},
                {RowVersion.JHopkinsV2, jHopkinsProvider},
                {RowVersion.JHopkinsV3, jHopkinsProvider},
                {RowVersion.JHopkinsV4, jHopkinsProvider}
            };
        }

        public IDataSourceReader GetReader()
        {
            return new JHopkinsDataSourceReader(
                Directory.EnumerateFiles(_location, "*.csv"),
                _dataProviders, _logger);
        }
    }
}