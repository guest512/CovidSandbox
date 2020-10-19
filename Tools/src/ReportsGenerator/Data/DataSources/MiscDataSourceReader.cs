using System.Collections.Generic;
using ReportsGenerator.Data.Providers;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Data.DataSources
{
    public class MiscDataSourceReader : CsvFilesDataSourceReader
    {
        public MiscDataSourceReader(IEnumerable<string> files, IDataProvider dataProvider, ILogger logger) : base(files, dataProvider, logger)
        {
        }
    }
}