using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ReportsGenerator.Data.IO
{
    public class CsvFileReportStorage : IReportStorage
    {
        private readonly string _countriesFolder;
        private readonly string _daysFolder;

        public CsvFileReportStorage(string rootDir, bool rewrite)
        {
            Root = string.IsNullOrEmpty(rootDir) ? "./out" :rootDir;
            _countriesFolder = Path.Combine(Root, "reports", "countries");
            _daysFolder = Path.Combine(Root, "reports", "dayByDay");

            Initialize(rewrite);
        }

        public string Root { get; }

        public IReportDataWriter GetWriter(IEnumerable<string> name, WriterType reportType)
        {
            var names = name.Select(n => n.Replace('*', '_')).ToArray();
            switch (reportType)
            {
                case WriterType.Day:
                    return new CsvFileDataWriter(_daysFolder, names[0]);

                case WriterType.Country:
                    var countryFolder = Path.Combine(_countriesFolder, names[0]);
                    Directory.CreateDirectory(countryFolder);
                    return new CsvFileDataWriter(countryFolder, names[0]);

                case WriterType.Province:
                    var provinceFolder = Path.Combine(_countriesFolder, names[0], "regions");
                    Directory.CreateDirectory(provinceFolder);
                    return new CsvFileDataWriter(provinceFolder, names[1]);

                default:
                    throw new ArgumentOutOfRangeException(nameof(reportType), reportType, null);
            }
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

        private class CsvFileDataWriter : IReportDataWriter
        {
            private readonly FileStream _file;
            private readonly TextWriter _fileWriter;

            public CsvFileDataWriter(string folder, string fileName)
            {
                _file = File.OpenWrite(Path.Combine(folder, $"{fileName}.csv"));
                _fileWriter = new StreamWriter(_file, Encoding.UTF8, 1024, true);
            }

            public void Close()
            {
                _fileWriter.Close();
                _file.Close();
            }

            public void WriteDataLine(IEnumerable<string> data)
            {
                _fileWriter.WriteLine(string.Join(',', data));
            }

            public void WriteHeader(IEnumerable<string> header)
            {
                _fileWriter.Flush();
                _file.Position = 0;
                _fileWriter.WriteLine(string.Join(',', header));
            }
        }
    }
}