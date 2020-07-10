﻿using CovidSandbox.Data;

namespace CovidSandbox.Model.Processors
{
    public sealed class JHopkinsRowProcessor : BaseRowProcessor
    {
        public override string GetCountryName(Row row)
        {
            var countryRowValue = row[Field.CountryRegion];
            var provinceRowValue = row[Field.ProvinceState];

            return countryRowValue switch
            {
                "Mainland China" when provinceRowValue == "Hong Kong" => "Hong Kong",
                "China" when provinceRowValue == "Hong Kong" => "Hong Kong",

                "Mainland China" when provinceRowValue == "Macau" => "Macau",
                "China" when provinceRowValue == "Macau" => "Macau",
                "Mainland China" when provinceRowValue == "Macao SAR" => "Macau",
                "China" when provinceRowValue == "Macao SAR" => "Macau",

                "US" when provinceRowValue == "Diamond Princess" => "Others",
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
                _ => countryRowValue
            };
        }

        public override string GetCountyName(Row row) => row[Field.Admin2];

        public override uint GetFips(Row row) => (uint)TryGetValue(row[Field.FIPS]);

        public override Origin GetOrigin(Row row) => Origin.JHopkins;

        public override string GetProvinceName(Row row)
        {
            var countryRowValue = row[Field.CountryRegion];
            var provinceRowValue = row[Field.ProvinceState];

            return provinceRowValue switch
            {
                _ when countryRowValue == "French Guiana" => "French Guiana",
                _ when countryRowValue == "Martinique" => "Martinique",
                _ when countryRowValue == "Mayotte" => "Mayotte",
                _ when countryRowValue == "Guam" => "Guam",
                _ when countryRowValue == "Diamond Princess" => "Diamond Princess",
                _ when countryRowValue == "Gibraltar" => "Gibraltar",
                _ when countryRowValue == "Saint Barthelemy" => "Saint Barthelemy",
                _ when countryRowValue == "Guadeloupe" => "Guadeloupe",
                _ when countryRowValue == "Channel Islands" => "Channel Islands",
                _ when countryRowValue == "Curacao" => "Curacao",
                _ when countryRowValue == "Aruba" => "Aruba",
                _ when countryRowValue == "Cayman Islands" => "Cayman Islands",
                _ when countryRowValue == "Reunion" => "Reunion",
                _ when countryRowValue == "Faroe Islands" => "Faroe Islands",
                _ when countryRowValue == "Greenland" => "Greenland",
                _ when countryRowValue == "Puerto Rico" => "Puerto Rico",

                "" when countryRowValue == "United Kingdom" => MainCountryRegion,
                "United Kingdom" when countryRowValue == "United Kingdom" => MainCountryRegion,

                "" when countryRowValue == "France" => MainCountryRegion,

                "Unknown" => MainCountryRegion,

                "Diamond Princess cruise ship" => "Diamond Princess",

                _ => provinceRowValue
            };
        }
    }
}