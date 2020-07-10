using CovidSandbox.Data.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CovidSandbox.Data
{
    public class CsvReader
    {
        private readonly IEnumerable<IDataProvider> _providers = new IDataProvider[]
        {
            new JHopkinsDataProvider(),
            new YandexRussiaDataProvider()
        };

        public IEnumerable<Row> Read(TextReader csvStream, string date = "")
        {
            var line = csvStream.ReadLine();
            if (line == null)
                yield break;

            var header = SplitRowString(line);
            var version = GetVersionFromHeader(header, out var activeProvider);
            Debug.Assert(activeProvider != null, nameof(activeProvider) + " != null");
            line = csvStream.ReadLine();

            while (line != null)
            {
                var row = SplitRowString(line);
                var fields = ReadData(activeProvider.GetFields(version), row, date);

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

        public static string[] SplitRowString(string row)
        {
            var csvRegex = new Regex("(\"(.+?)\",)|(\"(.+?)\")|(.*?,)|(.+)");
            return csvRegex
                .Matches(row)
                .Select(res => res.Groups.OfType<Group>().Last(grp => grp.Success).Value.Trim(','))
                .ToArray();
        }

        private RowVersion GetVersionFromHeader(string[] header, out IDataProvider? activeProvider)
        {
            var version = RowVersion.Unknown;
            activeProvider = null;

            foreach (var provider in _providers)
            {
                version = provider.GetVersion(header);
                if (version == RowVersion.Unknown)
                    continue;

                activeProvider = provider;
                break;
            }

            if (version == RowVersion.Unknown)
                throw new Exception("CsvStream has unknown format");

            return version;
        }
    }
}