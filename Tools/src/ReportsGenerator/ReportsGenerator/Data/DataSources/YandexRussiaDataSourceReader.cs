using System.Collections.Generic;
using System.Linq;
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
            SupportedFilters = new[]
            {
                Field.LastUpdate
            };
        }

        /// <inheritdoc/>
        protected override IEnumerable<Row> GetRows(string filePath, FilterRows filter)
        {
            var simpleBannedCache = new List<string>();

            foreach (Row row in base.GetRows(filePath, filter))
            {
                if(simpleBannedCache.Contains(row[Field.LastUpdate]))
                    continue;

                if (filter(new CsvField(Field.LastUpdate, row[Field.LastUpdate]))) 
                    yield return row;
                else
                    simpleBannedCache.Add(row[Field.LastUpdate]);
            }
        }
    }
}