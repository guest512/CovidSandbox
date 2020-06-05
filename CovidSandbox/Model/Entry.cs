using CovidSandbox.Data;
using System;

namespace CovidSandbox.Model
{
    internal class Entry
    {
        public Entry(Row rowData)
        {
            FIPS = TryGetValue(rowData[Field.FIPS]);
            Admin2 = rowData[Field.Admin2];
            ProvinceState = rowData[Field.ProvinceState];
            CountryRegion = ProcessCountryName(rowData[Field.CountryRegion]);
            LastUpdate = Utils.ParseDate(rowData[Field.LastUpdate]);
            Confirmed = TryGetValue(rowData[Field.Confirmed]);
            Deaths = TryGetValue(rowData[Field.Deaths]);
            Recovered = TryGetValue(rowData[Field.Recovered]);
            Active = TryGetValue(rowData[Field.Active]);
        }

        public uint? Active { get; }
        public string Admin2 { get; }
        public uint? Confirmed { get; }
        public string CountryRegion { get; }
        public uint? Deaths { get; }
        public uint? FIPS { get; }
        public DateTime LastUpdate { get; }
        public string ProvinceState { get; }
        public uint? Recovered { get; }

        public override string ToString() => string.IsNullOrEmpty(ProvinceState)
            ? $"{CountryRegion}, {LastUpdate.ToShortDateString()}: {Confirmed}-{Active}-{Recovered}-{Deaths}"
            : $"{CountryRegion}({ProvinceState}), {LastUpdate.ToShortDateString()}: {Confirmed}-{Active}-{Recovered}-{Deaths}";

        private static uint? TryGetValue(string stringValue) => uint.TryParse(stringValue, out var intValue) ? (uint?)intValue : null;

        private static string ProcessCountryName(string countryName)
        {
            return countryName switch
            {
                " Azerbaijan" => "Azerbaijan",
                "Russian Federation" => "Russia",
                "Viet Nam" => "Vietnam",
                "United Kingdom" => "UK",
                "Taiwan*" => "Taiwan",
                "Gambia, The" => "Gambia",
                "The Gambia" => "Gambia",
                "Korea, South" => "South Korea",
                "Macao SAR" => "Macau",
                "Iran (Islamic Republic of)" => "Iran",
                "Hong Kong SAR" => "Hong Kong",
                "Bahamas, The" => "Bahamas",
                "The Bahamas" => "Bahamas",
                "Mainland China" => "China",
                "Taipei and environs" => "Taiwan",
                "St. Martin" => "Saint Martin",
                "Republic of the Congo" => "Congo (Brazzaville)",
                "Republic of Moldova" => "Moldova",
                "Republic of Ireland" => "Ireland",
                _ => countryName
            };
        }
    }
}