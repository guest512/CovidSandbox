using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ReportsGenerator.Data.DataSources;
using ReportsGenerator.Data.DataSources.Providers;
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
                var rowValues = SplitRowString(line);
                var fields = ReadData(_providers[version].GetFields(version), rowValues, date);

                var row = new Row(fields, version);
                
                if(!IsInvalidData(row))
                    yield return row;

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
    }
}