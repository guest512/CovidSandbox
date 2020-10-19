using ReportsGenerator.Data.Providers;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Data.DataSources
{
    public class YandexRussiaDataSourceReader : CsvFilesDataSourceReader
    {
        public YandexRussiaDataSourceReader(string file, IDataProvider dataProvider, ILogger logger) : base(new[] { file }, dataProvider, logger)
        {
        }
    }
}