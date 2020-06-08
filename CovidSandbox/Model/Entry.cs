using CovidSandbox.Data;
using System;

namespace CovidSandbox.Model
{
    public readonly struct Entry
    {
        public Entry(Row rowData)
        {
            ProvinceState = rowData[Field.ProvinceState];
            CountryRegion = ProcessCountryName(rowData[Field.CountryRegion]);
            LastUpdate = Utils.ParseDate(rowData[Field.LastUpdate]);
            Confirmed = TryGetValue(rowData[Field.Confirmed]);
            Deaths = TryGetValue(rowData[Field.Deaths]);
            Recovered = TryGetValue(rowData[Field.Recovered]);
            Active = TryGetValue(rowData[Field.Active]);
        }

        public static Entry Empty { get; }

        public uint Active { get; }

        public uint Confirmed { get; }

        public string CountryRegion { get; }

        public uint Deaths { get; }

        public DateTime LastUpdate { get; }

        public string ProvinceState { get; }

        public uint Recovered { get; }

        public static bool operator !=(Entry left, Entry right) => !(left == right);

        public static bool operator ==(Entry left, Entry right) => left.Equals(right);

        public override bool Equals(object? obj)
        {
            return obj is Entry other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Active, Confirmed, CountryRegion, Deaths, LastUpdate, ProvinceState, Recovered);
        }

        public override string ToString() => string.IsNullOrEmpty(ProvinceState)
            ? $"{CountryRegion}, {LastUpdate.ToShortDateString()}: {Confirmed}-{Active}-{Recovered}-{Deaths}"
            : $"{CountryRegion}({ProvinceState}), {LastUpdate.ToShortDateString()}: {Confirmed}-{Active}-{Recovered}-{Deaths}";

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

        private static uint TryGetValue(string stringValue) => uint.TryParse(stringValue, out var intValue) ? intValue : 0;

        private bool Equals(Entry other)
        {
            return Active == other.Active && Confirmed == other.Confirmed && CountryRegion == other.CountryRegion &&
                   Deaths == other.Deaths && LastUpdate.Equals(other.LastUpdate) &&
                   ProvinceState == other.ProvinceState && Recovered == other.Recovered;
        }
    }
}