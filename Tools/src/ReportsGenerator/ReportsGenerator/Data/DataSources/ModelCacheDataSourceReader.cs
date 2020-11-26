using System.Collections.Generic;
using ReportsGenerator.Data.Providers;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Data.DataSources
{
    public class ModelCacheDataSourceReader: CsvFilesDataSourceReader
    {
        public ModelCacheDataSourceReader(IEnumerable<string> files, IDataProvider dataProvider, ILogger logger) : base(files, dataProvider, logger)
        {
        }
    }
}