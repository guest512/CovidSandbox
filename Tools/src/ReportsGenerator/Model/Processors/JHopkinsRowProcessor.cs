using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ReportsGenerator.Data;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Model.Processors
{
    public class JHopkinsRowProcessor : BaseRowProcessor
    {
        private readonly Dictionary<string, string> _canadaStates;
        private readonly string _filesPath;
        private readonly Dictionary<string, string> _russianRegions;
        private readonly Regex _stateCountyRegex = new Regex(@"^([\w\s]+?), (\w\w)$");
        private readonly Dictionary<string, string> _usStates;

        public JHopkinsRowProcessor(ILogger logger) : this("Data/Misc", logger)
        {
        }

        protected JHopkinsRowProcessor(string filesPath, ILogger logger) : base(logger)
        {
            _filesPath = filesPath;
            _russianRegions = GetCountryStatesRegions("Russian_Regions.csv");
            _usStates = GetCountryStatesRegions("Us_States.csv");
            _canadaStates = GetCountryStatesRegions("Canada_States.csv");
        }

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

                _ when provinceRowValue.Contains("Diamond Princess") => "Others",
                "Diamond Princess" => "Others",
                "Cruise Ship" => "Others",

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
                "occupied Palestinian territory" => "Palestine",
                _ => countryRowValue
            };
        }

        public override string GetCountyName(Row row)
        {
            var countyRowValue = row[Field.Admin2];
            var provinceRowValue = row[Field.ProvinceState];
            var countryRowValue = row[Field.CountryRegion];

            return countryRowValue switch
            {
                "US" => GetCountyName(provinceRowValue, countyRowValue),
                "Canada" => GetCountyName(provinceRowValue, countyRowValue),
                _ => countyRowValue
            };
        }

        public override uint GetFips(Row row) => (uint)TryGetValue(row[Field.FIPS]);

        public override Origin GetOrigin(Row row) => Origin.JHopkins;

        public override string GetProvinceName(Row row)
        {
            var countryRowValue = row[Field.CountryRegion];
            var provinceRowValue = row[Field.ProvinceState];

            return provinceRowValue switch
            {
                "Unknown" => Consts.MainCountryRegion,
                "unassigned" => Consts.MainCountryRegion,
                "Hong Kong" => Consts.MainCountryRegion,
                "Macau" => Consts.MainCountryRegion,
                "Taiwan" => Consts.MainCountryRegion,
                "UK" => Consts.MainCountryRegion,
                "US" => Consts.MainCountryRegion,

                _ when provinceRowValue.Contains("Diamond Princess") => "Diamond Princess",

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

                _ when string.IsNullOrEmpty(provinceRowValue) => Consts.MainCountryRegion,
                _ when countryRowValue == provinceRowValue => Consts.MainCountryRegion,

                _ when countryRowValue == "Russia" => _russianRegions[provinceRowValue],
                _ when countryRowValue == "US" => GetUsProvinceName(provinceRowValue),
                _ when countryRowValue == "Canada" => GetCanadaProvinceName(provinceRowValue),

                _ => provinceRowValue
            };
        }

        private string GetCanadaProvinceName(string provinceRowValue)
        {
            var match = _stateCountyRegex.Match(provinceRowValue);
            return match.Success ? _canadaStates[match.Groups[2].Value] : provinceRowValue;
        }

        private Dictionary<string, string> GetCountryStatesRegions(string filename)
        {
            var lines = File.ReadAllLines(Path.Combine(_filesPath, filename));

            static KeyValuePair<string, string> Selector(string x)
            {
                var values = x.Split(',');
                return new KeyValuePair<string, string>(values[0], values[1]);
            }

            return new Dictionary<string, string>(lines.Skip(1).Select(Selector));
        }

        private string GetCountyName(string provinceRowValue, string countyRowValue)
        {
            var match = _stateCountyRegex.Match(provinceRowValue);
            if (!match.Success)
                return countyRowValue;

            var countyFromProvince = match.Groups[1].Value.Trim();
            if (countyFromProvince.EndsWith(" County"))
                countyFromProvince = countyFromProvince[..^7];

            return countyFromProvince;
        }

        private string GetUsProvinceName(string provinceRowValue)
        {
            var match = _stateCountyRegex.Match(provinceRowValue);
            return match.Success ? _usStates[match.Groups[2].Value] : provinceRowValue;
        }
    }
}