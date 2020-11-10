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
                ("Macau", _) => "China",
                ("Macao SAR", _) => "China",
                ("Hong Kong", _) => "China",
                ("Hong Kong SAR", _) => "China",

                var x when x.province.Contains("Diamond Princess") || x.province.Contains("Grand Princess") => "Others",
                ("MS Zaandam", _) => "Others",
                ("Diamond Princess", _) => "Others",
                ("Cruise Ship", _) => "Others",

                ("French Guiana", _) => "France",
                ("Martinique", _) => "France",
                ("Mayotte", _) => "France",
                ("Saint Barthelemy", _) => "France",
                ("Guadeloupe", _) => "France",
                ("Reunion", _) => "France",
                ("St. Martin", _) => "France",
                ("Saint Martin", _) => "France",

                ("Gibraltar", _) => "UK",
                ("Channel Islands", _) => "UK",
                ("Guernsey", _) => "UK",
                ("Jersey", _) => "UK",
                ("Cayman Islands", _) => "UK",
                ("North Ireland", _) => "UK",

                ("Curacao", _) => "Netherlands",
                ("Aruba", _) => "Netherlands",

                ("Faroe Islands", _) => "Denmark",
                ("Greenland", _) => "Denmark",

                ("Guam", _) => "US",
                ("Puerto Rico", _) => "US",

                ("Vatican City", _) => "Holy See",
                ("Ivory Coast", _) => "Cote d'Ivoire",
                ("Cape Verde", _) => "Cabo Verde",
                (" Azerbaijan", _) => "Azerbaijan",
                ("Russian Federation", _) => "Russia",
                ("Viet Nam", _) => "Vietnam",
                ("United Kingdom", _) => "UK",
                ("Taiwan*", _) => "Taiwan",
                ("Gambia, The", _) => "Gambia",
                ("The Gambia", _) => "Gambia",
                ("Republic of Korea", _) => "South Korea",
                ("Korea, South", _) => "South Korea",
                ("Iran (Islamic Republic of)", _) => "Iran",
                ("Bahamas, The", _) => "Bahamas",
                ("The Bahamas", _) => "Bahamas",
                ("Mainland China", _) => "China",
                ("Taipei and environs", _) => "Taiwan",
                ("Republic of the Congo", _) => "Congo (Brazzaville)",
                ("Republic of Moldova", _) => "Moldova",
                ("Republic of Ireland", _) => "Ireland",
                ("Czech Republic", _) => "Czechia",
                ("occupied Palestinian territory", _) => "West Bank and Gaza",
                ("Palestine", _) => "West Bank and Gaza",
                ("Burma", _) => "Myanmar",
                ("East Timor", _) => "Timor-Leste",

                (_, "Crimea Republic*") => "Russia",
                (_, "Sevastopol*") => "Russia",

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
                "Guernsey" => "Guernsey",
                "Jersey" => "Jersey",
                "US" => GetCountyName(provinceRowValue, countyRowValue),
                "Canada" => GetCountyName(provinceRowValue, countyRowValue),
                _ => countyRowValue
            };
        }

        public override Origin GetOrigin(Row row) => Origin.JHopkins;

        public override string GetProvinceName(Row row)
        {
            var country = row[Field.CountryRegion];
            var province = row[Field.ProvinceState];

            return (province, country) switch
            {
                ("Unknown", _) => Consts.MainCountryRegion,
                ("unassigned", _) => Consts.MainCountryRegion,
                ("Taiwan", _) => Consts.MainCountryRegion,
                ("UK", _) => Consts.MainCountryRegion,
                ("US", _) => Consts.MainCountryRegion,
                ("None", _) => Consts.MainCountryRegion,

                var x when x.province.Contains("Grand Princess") => "Grand Princess",
                var x when x.province.Contains("Diamond Princess") => "Diamond Princess",

                (_, "North Ireland") => "Northern Ireland",
                (_, "Saint Martin") => "Saint Martin",
                (_, "French Guiana") => "French Guiana",
                (_, "Martinique") => "Martinique",
                (_, "Mayotte") => "Mayotte",
                (_, "Guam") => "Guam",
                (_, "MS Zaandam") => "MS Zaandam",
                (_, "Diamond Princess") => "Diamond Princess",
                (_, "Gibraltar") => "Gibraltar",
                (_, "Saint Barthelemy") => "Saint Barthelemy",
                (_, "Guadeloupe") => "Guadeloupe",
                (_, "Channel Islands") => "Channel Islands",
                (_, "Guernsey") => "Channel Islands",
                (_, "Jersey") => "Channel Islands",
                (_, "Curacao") => "Curacao",
                (_, "Aruba") => "Aruba",
                (_, "Cayman Islands") => "Cayman Islands",
                (_, "Reunion") => "Reunion",
                (_, "Faroe Islands") => "Faroe Islands",
                (_, "Greenland") => "Greenland",
                (_, "Puerto Rico") => "Puerto Rico",
                (_, "Macao SAR") => "Macau",
                (_, "Macau") => "Macau",
                (_, "Hong Kong SAR") => "Hong Kong",
                (_, "Hong Kong") => "Hong Kong",

                var x when string.IsNullOrEmpty(x.province) || x.province == x.country => Consts.MainCountryRegion,

                (_, "Russia") => _namesService.GetCyrillicName(province),
                ("Crimea Republic*", _) => _namesService.GetCyrillicName("Crimea Republic"),
                ("Sevastopol*", _) => _namesService.GetCyrillicName("Sevastopol"),

                ("United States Virgin Islands", _) => "Virgin Islands",
                ("Virgin Islands, U.S.", _) => "Virgin Islands",
                (_, "US") => GetProvinceName(province),

                (_, "Canada") => GetProvinceName(province),

                _ => province
            };
        }

        private string GetProvinceName(string provinceRowValue) =>
            Utils.TrySplitStateToStateCounty(provinceRowValue, out _, out var state)
                ? state.Length == 2 || state == "D.C." ? _namesService.GetStateFullName(state) : state
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