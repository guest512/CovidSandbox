using ReportsGenerator.Data.Providers;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Data.DataSources
{
    /// <summary>
    /// Represents a <see cref="CsvFilesDataSourceReader"/> for files from the Yandex Dashboard.
    /// </summary>
    public class YandexRussiaDataSourceReader : CsvFilesDataSourceReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="YandexRussiaDataSourceReader"/> class.
        /// </summary>
        /// <inheritdoc/>
        public YandexRussiaDataSourceReader(string file, IDataProvider dataProvider, ILogger logger) : base(new[] { file }, dataProvider, logger)
        {
        }
    }
}