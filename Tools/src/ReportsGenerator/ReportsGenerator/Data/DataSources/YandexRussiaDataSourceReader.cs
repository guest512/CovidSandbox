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
                FieldId.LastUpdate
            };
        }

        /// <inheritdoc/>
        protected override IEnumerable<Row> GetRows(string filePath, RowsFilter filter)
        {
            var simpleBannedCache = new List<string>();

            foreach (Row row in base.GetRows(filePath, filter))
            {
                if(simpleBannedCache.Contains(row[FieldId.LastUpdate]))
                    continue;

                if (filter(new Field(FieldId.LastUpdate, row[FieldId.LastUpdate]))) 
                    yield return row;
                else
                    simpleBannedCache.Add(row[FieldId.LastUpdate]);
            }
        }
    }
}