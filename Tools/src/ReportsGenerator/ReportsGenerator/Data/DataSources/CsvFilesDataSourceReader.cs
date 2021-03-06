﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ReportsGenerator.Data.IO.Csv;
using ReportsGenerator.Data.Providers;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Data.DataSources
{
    /// <summary>
    /// An implementation of <see cref="IDataSourceReader"/> that allows to read and parse CSV files.
    /// </summary>
    public class CsvFilesDataSourceReader : IDataSourceReader
    {
        private readonly IDataProvider _dataProvider;
        private readonly IEnumerable<string> _files;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvFilesDataSourceReader"/> class.
        /// </summary>
        /// <param name="files">Collection of strings - paths to files.</param>
        /// <param name="dataProvider">An <see cref="IDataProvider"/> object to convert CSV line to <see cref="Row"/>.</param>
        /// <param name="logger">A <see cref="ILogger"/> instance.</param>
        protected CsvFilesDataSourceReader(IEnumerable<string> files, IDataProvider dataProvider, ILogger logger)
        {
            _files = files;
            _dataProvider = dataProvider;
            _logger = logger;
        }

        /// <inheritdoc />
        public IEnumerable<FieldId> SupportedFilters { get; protected init; } = Enumerable.Empty<FieldId>();

        /// <inheritdoc />
        public IEnumerable<Row> GetRows() => GetRows(_ => true);

        /// <inheritdoc />
        public IEnumerable<Row> GetRows(RowsFilter filter)
        {
            foreach (string file in _files.Where(f => FilterFile(f, filter)))
            {
                _logger.WriteInfo($"{GetType().Name}: reading file {file}");

                foreach (var row in GetRows(file, filter))
                    yield return row;
            }
        }

        /// <inheritdoc />
        public IAsyncEnumerable<Row> GetRowsAsync() => GetRowsAsync((Field _) => true);

        /// <inheritdoc />
        public IAsyncEnumerable<TResult> GetRowsAsync<TResult>(GetRowsCallback<TResult> callback) => GetRowsAsync(_ => true, callback);

        /// <inheritdoc />
        public IAsyncEnumerable<Row> GetRowsAsync(RowsFilter filter) => GetRowsAsync(filter, row => row);

        /// <inheritdoc />
        public async IAsyncEnumerable<TResult> GetRowsAsync<TResult>(RowsFilter filter, GetRowsCallback<TResult> callback)
        {
            ConcurrentStack<TResult> rows = new();
            var source = new CancellationTokenSource();
            var token = source.Token;

            void ProcessFile(string file)
            {
                token.ThrowIfCancellationRequested();
                _logger.WriteInfo($"{GetType().Name}: reading file {file}");
                var items = new List<TResult>();

                foreach (var row in GetRows(file, filter))
                {
                    token.ThrowIfCancellationRequested();
                    items.Add(callback(row));
                }

                token.ThrowIfCancellationRequested();
                if (items.Count > 0)
                {
                    rows.PushRange(items.ToArray());
                }
            }

            var tasks = _files
                .Where(f => FilterFile(f, filter))
                .Select(file => Task.Run(() => ProcessFile(file), token))
                .ToArray();

            TResult[] buffer = new TResult[10240];
            try
            {
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

                    await Task.Delay(100, token);
                }

                if (tasks.Any(t => t.IsFaulted))
                {
                    source.Cancel();
                    throw new InvalidOperationException("Files reading has failed.",
                        tasks.First(t => t.IsFaulted).Exception);
                }
            }
            finally
            {
                foreach (var task in tasks)
                {
                    task.Dispose();
                }
            }
        }

        /// <summary>
        /// Factory function to create <see cref="Field"/> object.
        /// </summary>
        /// <param name="key">Column name.</param>
        /// <param name="value">Cell value.</param>
        /// <param name="fileName">File from where it comes.</param>
        /// <returns></returns>
        protected virtual Field CsvFieldCreator(FieldId key, string value, string fileName) => new(key, value);

        /// <summary>
        /// Filters row on file level.
        /// </summary>
        /// <param name="filePath">File path to read rows from.</param>
        /// <param name="filter">Filter delegate to call to determine whether or not file should be skipped.</param>
        /// <returns><see langword="true" /> if file should be user, otherwise returns <see langword="false" />.</returns>
        protected virtual bool FilterFile(string filePath, RowsFilter filter) => true;

        /// <summary>
        /// Reads CSV file and returns rows collection
        /// </summary>
        /// <param name="filePath">File name to read rows from.</param>
        /// <param name="filter">Filter delegate to call to determine whether or not row should be skipped.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="Row"/> read from file.</returns>
        protected virtual IEnumerable<Row> GetRows(string filePath, RowsFilter filter) => GetRows(filePath);

        /// <summary>
        /// Detects whether or not <see cref="Row"/> is invalid and shouldn't be returned.
        /// </summary>
        /// <param name="row">CSV row to test.</param>
        /// <returns><see langword="true" /> if by some reason <see cref="Row"/> should be ignored, otherwise returns <see langword="false" />.</returns>
        protected virtual bool IsInvalidData(Row row) => false;

        private IEnumerable<Field> GetCsvFields(IEnumerable<FieldId> keys, IEnumerable<string> fields, string fileName)
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
            var header = csvFile.GetHeader().ToArray();
            var contents = csvFile.GetRows().ToArray();

            if (header.Length == 0 && contents.Length == 0)
                return Enumerable.Empty<Row>();

            var version = GetVersionFromHeader(header, csvFile.Name);

            return contents
                .Select(r => new Row(GetCsvFields(_dataProvider.GetFields(version), r, csvFile.Name), version))
                .Where(r => !IsInvalidData(r));
        }

        private RowVersion GetVersionFromHeader(ICollection<string> header, string fileName)
        {
            var version = _dataProvider.GetVersion(header);

            if (version != RowVersion.Unknown)
                return version;

            _logger.WriteError($"{GetType().Name}: CsvFile '{fileName}' has unknown format");
            throw new Exception("CsvFile has unknown format");
        }
    }
}