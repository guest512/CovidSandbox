using System.Collections.Generic;
using ReportsGenerator.Data.DataSources.Providers;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Data.DataSources
{
    public class YandexRussiaDataSourceReader : CsvFilesDataSourceReader
    {
        public YandexRussiaDataSourceReader(string file, IDictionary<RowVersion, IDataProvider> dataProviders, ILogger logger) : base(new[] { file }, dataProviders, logger)
        {
        }
    }
}