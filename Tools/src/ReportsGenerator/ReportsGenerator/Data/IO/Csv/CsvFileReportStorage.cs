using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace ReportsGenerator.Data.IO.Csv
{
    /// <summary>
    /// Represents an implementation of <see cref="IReportStorage{TFormat}"/> for predefined folders structure with CSV files.
    /// </summary>
    /// <inheritdoc />
    public class CsvFileReportStorage : IReportStorage<string>
    {
        private readonly string _countriesFolder;
        private readonly string _daysFolder;
        private readonly string _statsFolder;
        private readonly string _modelCacheFolder;
        private readonly List<CsvFileWriterProxy> _writersCache = new();
        private readonly SemaphoreSlim _writersLocker = new(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvFileReportStorage"/> class.
        /// </summary>
        /// <param name="rootDir">A root directory path where all reports should be stored.</param>
        /// <param name="rewrite">A flag indicating whether or not previous results should be removed.</param>
        public CsvFileReportStorage(string rootDir, bool rewrite)
        {
            Root = string.IsNullOrEmpty(rootDir) ? "./out" : rootDir;
            _countriesFolder = Path.Combine(Root, "reports", "countries");
            _daysFolder = Path.Combine(Root, "reports", "dayByDay");
            _statsFolder = Path.Combine(Root, "stats");
            _modelCacheFolder = Path.Combine(Root, ".cache");

            Initialize(rewrite);
        }

        /// <summary>
        /// Gets a path to the root directory.
        /// </summary>
        public string Root { get; }

        /// <inheritdoc />
        public void Close()
        {
            foreach (var writer in _writersCache)
            {
                writer.Free();
            }
        }

        /// <inheritdoc />
        public IReportDataWriter<string> GetWriter(IEnumerable<string> name, ReportType type)
        {
            var names = name.Select(n => n.Replace('*', '_')).ToArray();
            var fileName = names.Last();
            var append = false;
            List<string> parts = new();

            switch (type)
            {
                case ReportType.Day:
                    parts.Add(_daysFolder);
                    break;

                case ReportType.Country:
                    parts.Add(_countriesFolder);
                    parts.Add(names[0]);
                    if (names.Length == 2)
                        parts.Add("regions");

                    break;

                case ReportType.Stats:
                    append = true;
                    parts.Add(_statsFolder);
                    fileName = "countries";
                    if (names.Length > 1)
                    {
                        parts.Add(names[0]);
                        fileName = "regions";
                    }

                    if (names.Length > 2)
                    {
                        parts.Add(names[1]);
                        fileName = "counties";
                    }

                    break;

                case ReportType.Model:
                    append = true;
                    parts.Add(_modelCacheFolder);
                    fileName = names[0] == "data" ? "model" : "metadata";

                    break;

                default:
                    throw new ArgumentException($"Unknown report type - {type}", nameof(type), null);
            }

            var folder = Path.Combine(parts.ToArray());
            Directory.CreateDirectory(folder);

            _writersLocker.Wait();

            var writer = _writersCache.FirstOrDefault(wc => wc.Folder == folder && wc.FileName == fileName);

            if (writer == null)
            {
                writer = new CsvFileWriterProxy(folder, fileName, append);
                _writersCache.Add(writer);
            }

            Debug.Assert(writer.Append == append);

            writer.Acquire();

            if (_writersCache.Count > 32)
            {
                foreach (var unusedWriter in _writersCache.Where(wr => wr.UsersCount == 0).ToArray())
                {
                    unusedWriter.Free();
                    _writersCache.Remove(unusedWriter);
                }
            }

            _writersLocker.Release();

            return writer;
        }

        private void Initialize(bool rewrite)
        {
            if (rewrite && Directory.Exists(Root) && Directory.EnumerateFileSystemEntries(Root).Any())
            {
                Directory.Delete(Root, true);
                Thread.Sleep(200);
            }

            foreach (var folder in new[] { Root, _countriesFolder, _daysFolder })
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }
        }

        /// <summary>
        /// Helper class to work with files and file streams in multi threaded environment.
        /// </summary>
        private class CsvFileWriterProxy : IReportDataWriter<string>
        {
            private readonly Lazy<CsvFileWriter> _realWriter;

            private readonly SemaphoreSlim _semaphore = new(1, 1);

            public CsvFileWriterProxy(string folder, string fileName, in bool append)
            {
                Folder = folder;
                FileName = fileName;
                Append = append;
                UsersCount = 0;

                _realWriter = new Lazy<CsvFileWriter>(() => new CsvFileWriter(Folder, FileName, Append));
            }

            public bool Append { get; }
            public string FileName { get; }
            public string Folder { get; }
            public int UsersCount { get; private set; }

            public void Acquire()
            {
                _semaphore.Wait();
                UsersCount++;
                _semaphore.Release();
            }

            public void Close()
            {
                _semaphore.Wait();
                UsersCount--;
                _semaphore.Release();
            }

            public void Free()
            {
                if (UsersCount > 0)
                    throw new InvalidOperationException("Can't free resources that used by someone else.");

                if (_realWriter.IsValueCreated)
                    ((IDisposable)_realWriter.Value).Dispose();
            }

            public void WriteDataLine(IEnumerable<string> data)
            {
                _semaphore.Wait();
                _realWriter.Value.WriteDataLine(data);
                _semaphore.Release();
            }

            public void WriteHeader(IEnumerable<string> header)
            {
                _semaphore.Wait();
                _realWriter.Value.WriteHeader(header);
                _semaphore.Release();
            }
        }
    }
}