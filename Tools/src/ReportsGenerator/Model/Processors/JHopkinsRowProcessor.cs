using ReportsGenerator.Data;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Model.Processors
{
    public class JHopkinsRowProcessor : BaseRowProcessor
    {
        private readonly INames _namesService;

        public JHopkinsRowProcessor(INames namesService, IStatsProvider statsProvider, ILogger logger) : base(statsProvider, logger)
        {
            _namesService = namesService;
        }

        public override string GetCountryName(Row row)
        {
            var country = row[Field.CountryRegion];
            var province = row[Field.ProvinceState];

            return (country, province) switch
            {
                ("Mainland China", "Hong Kong") => "Hong Kong",
                ("China", "Hong Kong") => "Hong Kong",

                ("Mainland China", "Macau") => "Macau",
                ("China", "Macau") => "Macau",
                ("Mainland China", "Macao SAR") => "Macau",
                ("China", "Macao SAR") => "Macau",

                var x when x.province.Contains("Diamond Princess") => "Others",
                ("Diamond Princess", _) => "Others",
                ("Cruise Ship", _) => "Others",

                ("French Guiana", _) => "France",
                ("Martinique", _) => "France",
                ("Mayotte", _) => "France",
                ("Saint Barthelemy", _) => "France",
                ("Guadeloupe", _) => "France",
                ("Reunion", _) => "France",

                ("Gibraltar", _) => "UK",
                ("Channel Islands", _) => "UK",
                ("Cayman Islands", _) => "UK",

                ("Curacao", _) => "Netherlands",
                ("Aruba", _) => "Netherlands",

                ("Faroe Islands", _) => "Denmark",
                ("Greenland", _) => "Denmark",

                ("Guam", _) => "US",
                ("Puerto Rico", _) => "US",

                (" Azerbaijan", _) => "Azerbaijan",
                ("Russian Federation", _) => "Russia",
                ("Viet Nam", _) => "Vietnam",
                ("United Kingdom", _) => "UK",
                ("Taiwan*", _) => "Taiwan",
                ("Gambia, The", _) => "Gambia",
                ("The Gambia", _) => "Gambia",
                ("Korea, South", _) => "South Korea",
                ("Macao SAR", _) => "Macau",
                ("Iran (Islamic Republic of)", _) => "Iran",
                ("Hong Kong SAR", _) => "Hong Kong",
                ("Bahamas, The", _) => "Bahamas",
                ("The Bahamas", _) => "Bahamas",
                ("Mainland China", _) => "China",
                ("Taipei and environs", _) => "Taiwan",
                ("St. Martin", _) => "Saint Martin",
                ("Republic of the Congo", _) => "Congo (Brazzaville)",
                ("Republic of Moldova", _) => "Moldova",
                ("Republic of Ireland", _) => "Ireland",
                ("Czech Republic", _) => "Czechia",
                ("occupied Palestinian territory", _) => "Palestine",
                _ => country
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

        public override uint GetFips(Row row) => (uint)row[Field.FIPS].AsLong();

        public override Origin GetOrigin(Row row) => Origin.JHopkins;

        public override string GetProvinceName(Row row)
        {
            var country = row[Field.CountryRegion];
            var province = row[Field.ProvinceState];

            return (province, country) switch
            {
                ("Unknown", _) => Consts.MainCountryRegion,
                ("unassigned", _) => Consts.MainCountryRegion,
                ("Hong Kong", _) => Consts.MainCountryRegion,
                ("Macau", _) => Consts.MainCountryRegion,
                ("Taiwan", _) => Consts.MainCountryRegion,
                ("UK", _) => Consts.MainCountryRegion,
                ("US", _) => Consts.MainCountryRegion,

                var x when x.province.Contains("Diamond Princess") => "Diamond Princess",

                (_, "French Guiana") => "French Guiana",
                (_, "Martinique") => "Martinique",
                (_, "Mayotte") => "Mayotte",
                (_, "Guam") => "Guam",
                (_, "Diamond Princess") => "Diamond Princess",
                (_, "Gibraltar") => "Gibraltar",
                (_, "Saint Barthelemy") => "Saint Barthelemy",
                (_, "Guadeloupe") => "Guadeloupe",
                (_, "Channel Islands") => "Channel Islands",
                (_, "Curacao") => "Curacao",
                (_, "Aruba") => "Aruba",
                (_, "Cayman Islands") => "Cayman Islands",
                (_, "Reunion") => "Reunion",
                (_, "Faroe Islands") => "Faroe Islands",
                (_, "Greenland") => "Greenland",
                (_, "Puerto Rico") => "Puerto Rico",

                var x when string.IsNullOrEmpty(x.province) || x.province == x.country => Consts.MainCountryRegion,

                (_, "Russia") => _namesService.GetCyrillicName(province),

                ("Virgin Islands, U.S.", _) => "Virgin Islands",
                (_, "US") => GetProvinceName(province),

                (_, "Canada") => GetProvinceName(province),

                _ => province
            };
        }

        private string GetProvinceName(string provinceRowValue) =>
            Utils.TrySplitStateToStateCounty(provinceRowValue, out _, out var state)
                ? state.Length == 2 ? _namesService.GetStateFullName(state) : state
                : provinceRowValue;

        private string GetCountyName(string provinceRowValue, string countyRowValue)
        {
            if (!Utils.TrySplitStateToStateCounty(provinceRowValue, out var county, out _))
                return countyRowValue;

            if (county.EndsWith(" County"))
                county = county[..^7];

            return county;
        }
    }
}