using ReportsGenerator.Data.DataSources.Providers;
using ReportsGenerator.Data.IO;
using ReportsGenerator.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportsGenerator.Data.DataSources
{
    public class CsvFilesDataSourceReader : IDataSourceReader
    {
        private readonly IDictionary<RowVersion, IDataProvider> _dataProviders;
        private readonly IEnumerable<string> _files;
        private readonly ILogger _logger;

        protected CsvFilesDataSourceReader(IEnumerable<string> files, IDictionary<RowVersion, IDataProvider> dataProviders, ILogger logger)
        {
            _files = files;
            _dataProviders = dataProviders;
            _logger = logger;
        }

        public IEnumerable<Row> GetRows()
        {
            foreach (string file in _files)
            {
                _logger.WriteInfo($"--Processing file: {file}");

                foreach (var row in GetRows(file))
                    yield return row;
            }
        }

        public async IAsyncEnumerable<Row> GetRowsAsync()
        {
            await foreach (var row in GetRowsAsync(row => row))
                yield return row;
        }

        public async IAsyncEnumerable<T> GetRowsAsync<T>(Func<Row, T> callback)
        {
            ConcurrentStack<T> rows = new ConcurrentStack<T>();

            void ProcessFile(string file)
            {
                _logger.WriteInfo($"--Processing file: {file}");
                rows.PushRange(GetRows(file).Select(callback).ToArray());
            }

            var tasks = _files
                .Select(file => Task.Run(() => ProcessFile(file)))
                .ToArray();

            T[] buffer = new T[10240];

            while (rows.Any() || !tasks.All(t => t.IsCompleted))
            {
                while (rows.Any())
                {
                    var bufferLength = rows.TryPopRange(buffer);
                    for (var i = 0; i < bufferLength; i++)
                        yield return buffer[i];
                }

                await Task.Delay(100);
            }

            foreach (var task in tasks)
            {
                task.Dispose();
            }
        }

        protected virtual CsvField CsvFieldCreator(Field key, string value, string fileName) => new CsvField(key, value);

        private static bool IsInvalidData(Row row)
        {
            static bool IsInvalidDate(Row row, IEnumerable<string> badDates) =>
                badDates.Any(d => d == row[Field.LastUpdate]);

            return row[Field.CountryRegion] switch
            {
                "Ireland" when row[Field.LastUpdate] == "03-08-2020" => true,
                "The Gambia" => true,
                "The Bahamas" => true,
                "Republic of the Congo" => IsInvalidDate(row, Enumerable.Range(17, 5).Select(n => $"03-{n}-2020")),
                "Guam" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),
                "Puerto Rico" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),

                "Denmark" when row[Field.ProvinceState] == "Greenland" =>
                    IsInvalidDate(row, new[] { "03-19-2020", "03-20-2020", "03-21-2020" }),
                "Netherlands" when row[Field.ProvinceState] == "Aruba" =>
                    IsInvalidDate(row, new[] { "03-18-2020", "03-19-2020" }),

                "France" when row[Field.ProvinceState] == "Mayotte" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),
                "France" when row[Field.ProvinceState] == "Guadeloupe" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),
                "France" when row[Field.ProvinceState] == "Reunion" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),
                "France" when row[Field.ProvinceState] == "French Guiana" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),

                "Mainland China" => IsInvalidDate(row, new[] { "03-11-2020", "03-12-2020" }),

                "US" when row[Field.LastUpdate] == "03-22-2020" && row[Field.FIPS] == "11001" && row[Field.Deaths] == "0" => true,

                _ => false
            };
        }

        private IEnumerable<CsvField> GetCsvFields(IEnumerable<Field> keys, IEnumerable<string> fields, string fileName)
        {
            using var keyEnumerator = keys.GetEnumerator();
            using var fieldsEnumerator = fields.GetEnumerator();

            while (keyEnumerator.MoveNext() && fieldsEnumerator.MoveNext())
            {
                yield return CsvFieldCreator(keyEnumerator.Current, fieldsEnumerator.Current, fileName);
            }
        }

        private IEnumerable<Row> GetRows(string fileName)
        {
            var csvFile = new CsvFile(fileName);

            var version = GetVersionFromHeader(csvFile.GetHeader(), fileName);

            return csvFile
                .GetRows()
                .Select(r => new Row(GetCsvFields(_dataProviders[version].GetFields(version), r, fileName), version))
                .Where(r => !IsInvalidData(r));
        }

        private RowVersion GetVersionFromHeader(IEnumerable<string> header, string fileName)
        {
            foreach (var (_, provider) in _dataProviders)
            {
                var version = provider.GetVersion(header);

                if (version != RowVersion.Unknown)
                    return version;
            }

            _logger.WriteError($"CsvFile '{fileName}' has unknown format");
            throw new Exception("CsvFile has unknown format");
        }
    }
}