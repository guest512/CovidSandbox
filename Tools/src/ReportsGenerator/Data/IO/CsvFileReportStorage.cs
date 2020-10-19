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
        private readonly string _statsFolder;

        public CsvFileReportStorage(string rootDir, bool rewrite)
        {
            Root = string.IsNullOrEmpty(rootDir) ? "./out" : rootDir;
            _countriesFolder = Path.Combine(Root, "reports", "countries");
            _daysFolder = Path.Combine(Root, "reports", "dayByDay");
            _statsFolder = Path.Combine(Root, "stats");

            Initialize(rewrite);
        }

        public string Root { get; }

        public IReportDataWriter GetWriter(IEnumerable<string> name, WriterType reportType)
        {
            var names = name.Select(n => n.Replace('*', '_')).ToArray();
            var fileName = names.Last();
            var append = false;
            List<string> parts = new List<string>();

            switch (reportType)
            {
                case WriterType.Day:
                    parts.Add(_daysFolder);
                    parts.Add(names[0]);
                    break;

                case WriterType.Country:
                    parts.Add(_countriesFolder);
                    parts.Add(names[0]);
                    if (names.Length == 2)
                        parts.Add("regions");

                    break;

                case WriterType.Stats:
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

                default:
                    throw new ArgumentOutOfRangeException(nameof(reportType), reportType, null);
            }

            var folder = Path.Combine(parts.ToArray());
            Directory.CreateDirectory(folder);


            return new CsvFileWriter(folder, fileName, append);
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