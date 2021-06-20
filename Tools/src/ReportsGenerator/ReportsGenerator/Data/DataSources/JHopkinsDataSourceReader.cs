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
                FieldId.LastUpdate
            };
        }

        /// <inheritdoc/> 
        protected override Field CsvFieldCreator(FieldId key, string value, string fileName) =>
            key == FieldId.LastUpdate ? new Field(key, fileName) : new Field(key, value);

        /// <inheritdoc/>
        protected override bool IsInvalidData(Row row)
        {
            static bool IsInvalidDate(Row row, IEnumerable<string> badDates) =>
                badDates.Any(d => d == row[FieldId.LastUpdate]);

            return row[FieldId.CountryRegion] switch
            {
                "Ireland" when row[FieldId.LastUpdate] == "03-08-2020" => true,
                "The Gambia" => true,
                "The Bahamas" => true,
                "Republic of the Congo" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),
                "Guam" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),
                "Puerto Rico" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),

                "Denmark" when row[FieldId.ProvinceState] == "Greenland" =>
                    IsInvalidDate(row, new[] { "03-19-2020", "03-20-2020", "03-21-2020" }),
                "Netherlands" when row[FieldId.ProvinceState] == "Aruba" =>
                    IsInvalidDate(row, new[] { "03-18-2020", "03-19-2020" }),

                "France" when row[FieldId.ProvinceState] == "Mayotte" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),
                "France" when row[FieldId.ProvinceState] == "Guadeloupe" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),
                "France" when row[FieldId.ProvinceState] == "Reunion" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),
                "France" when row[FieldId.ProvinceState] == "French Guiana" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),
                "France" when row[FieldId.ProvinceState] == "Fench Guiana" => true,

                "Mainland China" when row[FieldId.ProvinceState] == "Hubei" && row[FieldId.Recovered] == "28.0" => true,
                "Mainland China" => IsInvalidDate(row, new[] { "03-11-2020", "03-12-2020" }),

                "US" when row[FieldId.LastUpdate] == "03-22-2020" && row[FieldId.FIPS] == "11001" && row[FieldId.Deaths] == "0" => true,
                "US" when row[FieldId.ProvinceState] == "Wuhan Evacuee" => true,
                "US" when row[FieldId.ProvinceState] == "United States Virgin Islands" => IsInvalidDate(row, new[] { "03-18-2020", "03-19-2020" }),

                "Australia" when row[FieldId.ProvinceState] == "External territories" => true,


                "United Kingdom" when row[FieldId.ProvinceState] == "Channel Islands" => IsInvalidDate(row, Enumerable.Range(14,2).Select(n=>$"03-{n}-2020")),
                "Guernsey" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),
                "Jersey" => IsInvalidDate(row, Enumerable.Range(16, 6).Select(n => $"03-{n}-2020")),

                "Cape Verde" when row[FieldId.LastUpdate] == "03-21-2020" => true,

                "" => true,

                _ => false
            };
        }

        /// <inheritdoc/>
        protected override bool FilterFile(string filePath, RowsFilter filter) => 
            filter(new Field(FieldId.LastUpdate, Path.GetFileNameWithoutExtension(filePath)));
    }
}
