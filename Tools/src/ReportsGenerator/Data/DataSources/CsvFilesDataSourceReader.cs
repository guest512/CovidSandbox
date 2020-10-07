using ReportsGenerator.Data.DataSources.Providers;
using ReportsGenerator.Data.IO;
using ReportsGenerator.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReportsGenerator.Data.DataSources
{
    public class CsvFilesDataSourceReader : IDataSourceReader
    {
        private readonly IDataProvider _dataProvider;
        private readonly IEnumerable<string> _files;
        private readonly ILogger _logger;

        protected CsvFilesDataSourceReader(IEnumerable<string> files, IDataProvider dataProvider, ILogger logger)
        {
            _files = files;
            _dataProvider = dataProvider;
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
            var source = new CancellationTokenSource();
            var token = source.Token;


            void ProcessFile(string file)
            {
                token.ThrowIfCancellationRequested();
                _logger.WriteInfo($"--Processing file: {file}");
                var items = new List<T>();

                foreach (var row in GetRows(file))
                {
                    token.ThrowIfCancellationRequested();
                    items.Add(callback(row));
                }

                token.ThrowIfCancellationRequested();
                rows.PushRange(items.ToArray());
            }

            var tasks = _files
                .Select(file => Task.Run(() => ProcessFile(file), token))
                .ToArray();

            T[] buffer = new T[10240];

            while (rows.Any() || !tasks.All(t => t.IsCompleted))
            {
                while (rows.Any())
                {
                    if (tasks.Any(t => t.IsFaulted))
                    {
                        source.Cancel();
                        throw new InvalidOperationException("Files reading has failed.",
                            tasks.First(t => t.IsFaulted).Exception);
                    }

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

        protected virtual bool IsInvalidData(Row row) => false;

        private IEnumerable<CsvField> GetCsvFields(IEnumerable<Field> keys, IEnumerable<string> fields, string fileName)
        {
            using var keyEnumerator = keys.GetEnumerator();
            using var fieldsEnumerator = fields.GetEnumerator();

            while (keyEnumerator.MoveNext() && fieldsEnumerator.MoveNext())
            {
                yield return CsvFieldCreator(keyEnumerator.Current, fieldsEnumerator.Current, fileName);
            }
        }

        private IEnumerable<Row> GetRows(string filePath)
        {
            var csvFile = new CsvFileReader(filePath);

            if (!csvFile.GetHeader().Any() && !csvFile.GetRows().Any())
                return Enumerable.Empty<Row>();

            var version = GetVersionFromHeader(csvFile.GetHeader(), csvFile.Name);

            return csvFile
                .GetRows()
                .Select(r => new Row(GetCsvFields(_dataProvider.GetFields(version), r, csvFile.Name), version))
                .Where(r => !IsInvalidData(r));
        }

        private RowVersion GetVersionFromHeader(IEnumerable<string> header, string fileName)
        {
            var version = _dataProvider.GetVersion(header);

            if (version != RowVersion.Unknown)
                return version;

            _logger.WriteError($"CsvFile '{fileName}' has unknown format");
            throw new Exception("CsvFile has unknown format");
        }
    }
}