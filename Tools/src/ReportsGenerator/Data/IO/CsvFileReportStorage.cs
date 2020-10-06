using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace ReportsGenerator.Data.IO
{
    public class CsvFileReportStorage : IReportStorage
    {
        private readonly string _countriesFolder;
        private readonly string _daysFolder;

        public CsvFileReportStorage(string rootDir, bool rewrite)
        {
            Root = string.IsNullOrEmpty(rootDir) ? "./out" : rootDir;
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
                    return new CsvFileWriter(_daysFolder, names[0]);

                case WriterType.Country:
                    var countryFolder = Path.Combine(_countriesFolder, names[0]);
                    Directory.CreateDirectory(countryFolder);
                    return new CsvFileWriter(countryFolder, names[0]);

                case WriterType.Province:
                    var provinceFolder = Path.Combine(_countriesFolder, names[0], "regions");
                    Directory.CreateDirectory(provinceFolder);
                    return new CsvFileWriter(provinceFolder, names[1]);

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
    }
}