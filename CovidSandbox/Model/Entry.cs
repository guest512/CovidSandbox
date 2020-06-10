using CovidSandbox.Data;
using System;

namespace CovidSandbox.Model
{
    public readonly struct Entry
    {
        public Entry(Row rowData)
        {
            ProvinceState = ProcessProvinceName(rowData[Field.CountryRegion], rowData[Field.ProvinceState]);
            CountryRegion = ProcessCountryName(rowData[Field.CountryRegion], rowData[Field.ProvinceState]);
            LastUpdate = Data.Utils.ParseDate(rowData[Field.LastUpdate]);
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

        private static string ProcessCountryName(string countryName, string provinceName)
        {
            return countryName switch
            {
                "Mainland China" when provinceName == "Hong Kong" => "Hong Kong",
                "China" when provinceName == "Hong Kong" => "Hong Kong",

                "Mainland China" when provinceName == "Macau" => "Macau",
                "China" when provinceName == "Macau" => "Macau",
                "Mainland China" when provinceName == "Macao SAR" => "Macau",
                "China" when provinceName == "Macao SAR" => "Macau",

                "US" when provinceName == "Diamond Princess" => "Others",
                "Diamond Princess" => "Others",

                "French Guiana" => "France",
                "Martinique" => "France",
                "Mayotte" => "France",
                "Saint Barthelemy" => "France",
                "Guadeloupe" => "France",
                "Reunion" => "France",

                "Gibraltar" => "UK",
                "Channel Islands" => "UK",
                "Cayman Islands" => "UK",

                "Curacao" => "Netherlands",
                "Aruba" => "Netherlands",

                "Faroe Islands" => "Denmark",
                "Greenland" => "Denmark",

                "Guam" => "US",
                "Puerto Rico" => "US",

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
                "Czech Republic" => "Czechia",
                _ => countryName
            };
        }

        private static string ProcessProvinceName(string countryName, string provinceName)
        {
            return provinceName switch
            {
                _ when countryName == "French Guiana" => "French Guiana",
                _ when countryName == "Martinique" => "Martinique",
                _ when countryName == "Mayotte" => "Mayotte",
                _ when countryName == "Guam" => "Guam",
                _ when countryName == "Diamond Princess" => "Diamond Princess",
                _ when countryName == "Gibraltar" => "Gibraltar",
                _ when countryName == "Saint Barthelemy" => "Saint Barthelemy",
                _ when countryName == "Guadeloupe" => "Guadeloupe",
                _ when countryName == "Channel Islands" => "Channel Islands",
                _ when countryName == "Curacao" => "Curacao",
                _ when countryName == "Aruba" => "Aruba",
                _ when countryName == "Cayman Islands" => "Cayman Islands",
                _ when countryName == "Reunion" => "Reunion",
                _ when countryName == "Faroe Islands" => "Faroe Islands",
                _ when countryName == "Greenland" => "Greenland",
                _ when countryName == "Puerto Rico" => "Puerto Rico",
                "Diamond Princess cruise ship" => "Diamond Princess",
                _ => provinceName
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