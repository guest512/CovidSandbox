using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ReportsGenerator.Data.Providers;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Data
{
    public class CsvReader
    {
        private readonly ILogger _logger;
        private readonly IDictionary<RowVersion, IDataProvider> _providers;

        public CsvReader(IDictionary<RowVersion, IDataProvider> dataProviders, ILogger logger)
        {
            _providers = dataProviders;
            _logger = logger;
        }

        public static string[] SplitRowString(string row)
        {
            var csvRegex = new Regex("(\"(.+?)\",)|(\"(.+?)\")|(.*?,)|(.+)");
            return csvRegex
                .Matches(row)
                .Select(res => res.Groups.OfType<Group>().Last(grp => grp.Success).Value.Trim(','))
                .ToArray();
        }

        public IEnumerable<Row> Read(TextReader csvStream, string date = "")
        {
            var line = csvStream.ReadLine();
            if (line == null)
                yield break;

            var header = SplitRowString(line);
            var version = GetVersionFromHeader(header);
            line = csvStream.ReadLine();

            while (line != null)
            {
                var row = SplitRowString(line);
                var fields = ReadData(_providers[version].GetFields(version), row, date);

                yield return new Row(fields, version);
                line = csvStream.ReadLine();
            }
        }

        private static IEnumerable<CsvField> ReadData(IEnumerable<Field> keys, IEnumerable<string> fields, string date)
        {
            using var keyEnumerator = keys.GetEnumerator();
            using var fieldsEnumerator = fields.GetEnumerator();

            while (keyEnumerator.MoveNext() && fieldsEnumerator.MoveNext())
            {
                if (keyEnumerator.Current == Field.LastUpdate && !string.IsNullOrEmpty(date))
                    yield return new CsvField(keyEnumerator.Current, date);
                else
                    yield return new CsvField(keyEnumerator.Current, fieldsEnumerator.Current);
            }
        }

        private RowVersion GetVersionFromHeader(string[] header)
        {
            foreach (var (_, provider) in _providers)
            {
                var version = provider.GetVersion(header);

                if (version != RowVersion.Unknown)
                    return version;
            }

            _logger.WriteError("CsvStream has unknown format");
            throw new Exception("CsvStream has unknown format");
        }
    }
}