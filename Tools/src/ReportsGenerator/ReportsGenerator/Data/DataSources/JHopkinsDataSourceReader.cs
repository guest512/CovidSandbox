using ReportsGenerator.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ReportsGenerator.Data.Providers;

namespace ReportsGenerator.Data.DataSources
{
    /// <summary>
    /// Represents a <see cref="CsvFilesDataSourceReader"/> for files from the John Hopkins University.
    /// </summary>
    public class JHopkinsDataSourceReader : CsvFilesDataSourceReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JHopkinsDataSourceReader"/> class.
        /// </summary>
        /// <inheritdoc/> 
        public JHopkinsDataSourceReader(IEnumerable<string> files, IDataProvider dataProvider, ILogger logger) : base(files, dataProvider, logger)
        {
            SupportedFilters = new[]
            {
                Field.LastUpdate
            };
        }

        /// <inheritdoc/> 
        protected override CsvField CsvFieldCreator(Field key, string value, string fileName)
        {
            return key == Field.LastUpdate
                ? new CsvField(key, fileName)
                : new CsvField(key, value);
        }

        /// <inheritdoc/> 
        protected override bool IsInvalidData(Row row)
        {
            static bool IsInvalidDate(Row row, IEnumerable<string> badDates) =>
                badDates.Any(d => d == row[Field.LastUpdate]);

            return row[Field.CountryRegion] switch
            {
                "Ireland" when row[Field.LastUpdate] == "03-08-2020" => true,
                "The Gambia" => true,
                "The Bahamas" => true,
                "Republic of the Congo" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),
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
                "France" when row[Field.ProvinceState] == "Fench Guiana" => true,

                "Mainland China" => IsInvalidDate(row, new[] { "03-11-2020", "03-12-2020" }),

                "US" when row[Field.LastUpdate] == "03-22-2020" && row[Field.FIPS] == "11001" && row[Field.Deaths] == "0" => true,
                "US" when row[Field.ProvinceState] == "Wuhan Evacuee" => true,
                "US" when row[Field.ProvinceState] == "United States Virgin Islands" => IsInvalidDate(row, new[] { "03-18-2020", "03-19-2020" }),

                "Australia" when row[Field.ProvinceState] == "External territories" => true,


                "United Kingdom" when row[Field.ProvinceState] == "Channel Islands" => IsInvalidDate(row, Enumerable.Range(14,2).Select(n=>$"03-{n}-2020")),
                "Guernsey" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),
                "Jersey" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),

                "Cape Verde" when row[Field.LastUpdate] == "03-21-2020" => true,

                "" => true,

                _ => false
            };
        }

        /// <inheritdoc/>
        protected override bool FilterFile(string fileName, FilterRows filter)
        {
            return filter(new CsvField(Field.LastUpdate, Path.GetFileNameWithoutExtension(fileName)));
        }
    }
}