using System.Collections.Generic;
using System.Linq;
using ReportsGenerator.Data;
using ReportsGenerator.Data.DataSources;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Model
{
    /// <summary>
    /// Represents an implementation of <see cref="INames"/> and <see cref="IStatsProvider"/> interfaces.
    /// </summary>
    public class MiscStorage : INames, IStatsProvider
    {
        private readonly MiscDataSource _dataSource;
        private readonly ILogger _logger;

        private Dictionary<string, string>? _russianRegions;
        private Dictionary<string, string>? _states;
        private Dictionary<string, StatEntry>? _stats;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiscStorage"/> class.
        /// </summary>
        /// <param name="dataSource">Data sources for the storage. It should contain files with information about abbreviations, files with Cyrillic and Latin names, etc.</param>
        /// <param name="logger">A <see cref="ILogger"/> instance.</param>
        public MiscStorage(MiscDataSource dataSource, ILogger logger)
        {
            _dataSource = dataSource;
            _logger = logger;
        }

        private static string GenerateName(string county = "", string province = "", string country = "")
        {
            return string.IsNullOrEmpty(county)
                ? string.IsNullOrEmpty(province) ? country : string.Join(", ", province, country)
                : string.Join(", ", county, province, country);
        }

        /// <inheritdoc />
        public string GetCountryStatsName(string countryName)
        {
            return countryName switch
            {
                "Myanmar" => "Burma",
                "Curacao" => "Netherlands",
                "Aruba" => "Netherlands",
                "Guernsey" => "United Kingdom",
                "Jersey" => "United Kingdom",
                "North Ireland" => "United Kingdom",
                "Channel Islands" => "United Kingdom",
                "Cayman Islands" => "United Kingdom",
                "Gibraltar" => "United Kingdom",
                "Greenland" => "Denmark",
                "Faroe Islands" => "Denmark",
                "Mayotte" => "France",
                "Saint Martin" => "France",
                "St. Martin" => "France",
                "Guadeloupe" => "France",
                "Reunion" => "France",
                "Martinique" => "France",
                "Saint Barthelemy" => "France",
                "French Guiana" => "France",
                "Republic of Moldova" => "Moldova",
                "Russian Federation" => "Russia",
                "Vatican City" => "Holy See (the)",
                "Viet Nam" => "Vietnam",
                "Republic of Ireland" => "Ireland",
                "Republic of the Congo" => "Congo (Brazzaville)",
                "Bahamas, The" => "Bahamas",
                "Gambia, The" => "Gambia",
                "Iran (Islamic Republic of)" => "Iran",
                "occupied Palestinian territory" => "West Bank and Gaza",
                "Palestine" => "West Bank and Gaza",
                "Mainland China" => "China",
                "Republic of Korea" => "Korea, South",
                "South Korea" => "Korea, South",
                "UK" => "United Kingdom",
                "Ivory Coast" => "Cote d'Ivoire",
                "Czech Republic" => "Czechia",
                "Cape Verde" => "Cabo Verde",
                "Taiwan" => "Taiwan*",
                _ => countryName
            };
        }

        /// <inheritdoc />
        public string GetCyrillicName(string latinName)
        {
            if (_russianRegions == null)
                ProcessRows();

            return _russianRegions![latinName];
        }

        /// <inheritdoc />
        public string GetLatinName(string cyrillicName)
        {
            if (_russianRegions == null)
                ProcessRows();

            return _russianRegions!.First(kvp => kvp.Value == cyrillicName).Key;
        }

        /// <inheritdoc />
        public string GetStateFullName(string stateAbbrev)
        {
            if (_states == null)
                ProcessRows();

            return _states![stateAbbrev];
        }

        /// <inheritdoc />
        public string GetStatsName(Row row)
        {
            if (_russianRegions == null)
                ProcessRows();

            var county = row[Field.Admin2].Trim();
            var province = row[Field.ProvinceState].Trim();
            var country = row[Field.CountryRegion].Trim();

            if (_russianRegions!.ContainsValue(province))
            {
                province = _russianRegions.First(kvp => kvp.Value == province).Key;
                country = "Russia";
            }

            // The difference between data files and statistics files is huge.
            // To build a key, we need to translates strings from data files
            // to values that used by statistical files.
            //
            // For instance:
            // Data file: country='US', state='Chicago', county=''
            // Stats file: country='US', state='Illinois', county='Chicago'

            switch (country)
            {
                case "Curacao":
                case "Aruba":
                    province = country;
                    country = "Netherlands";
                    break;

                case "Guernsey":
                case "Jersey":
                    province = "Channel Islands";
                    country = "United Kingdom";
                    break;

                case "North Ireland":
                    province = "Northern Ireland";
                    country = "United Kingdom";
                    break;

                case "Channel Islands":
                case "Cayman Islands":
                case "Gibraltar":
                    province = country;
                    country = "United Kingdom";
                    break;

                case "Greenland":
                case "Faroe Islands":
                    province = country;
                    country = "Denmark";
                    break;

                case "Mayotte":
                case "Saint Martin":
                case "St. Martin":
                    province = "St Martin";
                    country = "France";
                    break;

                case "Guadeloupe":
                case "Reunion":
                case "Martinique":
                case "Saint Barthelemy":
                case "French Guiana":
                    province = country;
                    country = "France";
                    break;

                case "Republic of Moldova":
                    country = "Moldova";
                    break;

                case "Russian Federation":
                    country = "Russia";
                    break;

                case "Vatican City":
                    country = "Holy See (the)";
                    break;

                case "Viet Nam":
                    country = "Vietnam";
                    break;

                case "Republic of Ireland":
                    country = "Ireland";
                    break;

                case "Republic of the Congo":
                    country = "Congo (Brazzaville)";
                    break;

                case "Bahamas, The":
                    country = "Bahamas";
                    break;

                case "Gambia, The":
                    country = "Gambia";
                    break;

                case "Iran (Islamic Republic of)":
                    country = "Iran";
                    break;

                case "occupied Palestinian territory":
                case "Palestine":
                    country = "West Bank and Gaza";
                    break;

                case "Mainland China":
                case "Macau":
                case "Macao SAR":
                case "Hong Kong":
                case "Hong Kong SAR":
                    country = "China";
                    break;

                case "Republic of Korea":
                case "South Korea":
                    country = "Korea, South";
                    break;

                case "UK":
                    country = "United Kingdom";
                    break;

                case "Ivory Coast":
                    country = "Cote d'Ivoire";
                    break;

                case "Czech Republic":
                    country = "Czechia";
                    break;

                case "Cape Verde":
                    country = "Cabo Verde";
                    break;

                case "East Timor":
                    country = "Timor-Leste";
                    break;
            }

            switch (province)
            {
                case "US":
                case "Denmark":
                case "Netherlands":
                case "UK":
                case "United Kingdom":
                case "France":
                case "None":
                case "Repatriated Travellers":
                    province = string.Empty;
                    break;

                case "Taiwan":
                    province = string.Empty;
                    country = "Taiwan*";
                    break;

                case "Fench Guiana":
                    province = "French Guiana";
                    break;

                case "Bavaria":
                    province = "Bayern";
                    break;

                case "Dadar Nagar Haveli":
                    province = "Dadra and Nagar Haveli and Daman and Diu";
                    break;

                case "Jervis Bay Territory":
                    province = "New South Wales";
                    break;

                case "Falkland Islands (Islas Malvinas)":
                    province = "Falkland Islands (Malvinas)";
                    break;

                case "Chicago":
                    province = GetStateFullName("IL");
                    break;

                case "Washington, D.C.":
                    province = GetStateFullName("DC");
                    break;

                case "United States Virgin Islands":
                case "Virgin Islands, U.S.":
                    province = "Virgin Islands";
                    break;

                case "Edmonton, Alberta":
                case "Calgary, Alberta":
                    province = "Alberta";
                    break;

                case "Crimea Republic":
                case "Sevastopol":
                    province += "*";
                    country = "Ukraine";
                    break;

                default:
                    if (Utils.TrySplitStateToStateCounty(province, out _, out var state) && state.Length == 2)
                        province = GetStateFullName(state);
                    break;
            }

            switch (county)
            {
                case "unassigned" when country == "US":
                case "Unknown" when country == "US":
                case "Out-of-state" when country == "US":
                    county = "Unassigned";
                    break;

                case "Southwest":
                    county = "Southwest Utah";
                    break;

                case "Sterling":
                case "Soldotna":
                    county = "Kenai Peninsula";
                    break;

                case "LeSeur":
                    county = "Le Sueur";
                    break;

                case "Doña Ana":
                    county = "Dona Ana";
                    break;

                case "Nashua":
                    county = "Hillsborough";
                    break;

                case "Brockton":
                    county = "Plymouth";
                    break;

                case "New York City":
                    county = "New York";
                    break;

                case "Desoto":
                    county = "DeSoto";
                    break;

                case "Yakutat":
                    county = "Yakutat plus Hoonah-Angoon";
                    break;

                case "Lake and Peninsula":
                case "Bristol Bay":
                    county = "Bristol Bay plus Lake Peninsula";
                    break;

                default:
                    if (county.EndsWith(" County"))
                        county = county[..^7];
                    break;
            }

            if (county.Contains("Diamond Princess") || province.Contains("Diamond Princess") ||
                country.Contains("Diamond Princess") || province.Contains("Cruise Ship"))
                return "Diamond Princess";

            return GenerateName(county, province, country);
        }

        /// <summary>
        /// Forces storage initialization to ensure that it will be ready before its usage.
        /// </summary>
        public void Init() => ProcessRows();

        /// <inheritdoc />
        public string LookupContinentName(string name)
        {
            if (_stats == null)
                ProcessRows();

            return _stats![name].Continent;
        }

        /// <inheritdoc />
        public long LookupPopulation(string name)
        {
            if (_stats == null)
                ProcessRows();

            return _stats![name].Population;
        }

        private Dictionary<string, StatEntry> CreateStats(IEnumerable<Row> mainStats,
            ICollection<Row> continentInfo)
        {
            var res = new Dictionary<string, StatEntry>();
            foreach (var row in mainStats)
            {
                var name = GetStatsName(row);
                var entry = new StatEntry(
                    continentInfo.First(ci => ci[Field.Iso3] == row[Field.Iso3])[Field.ContinentName],
                    row[Field.Code3],
                    row[Field.Iso3],
                    row[Field.Population],
                    name);

                if (res.ContainsKey(name)) // Ignore duplicates
                {
                    var old = res[name];
                    _logger.WriteWarning(
                        $"Stats duplication for '{name}'." +
                        $" Old: {old.Iso3} - {old.Continent} - {old.Population}." +
                        $" New: Old: {entry.Iso3} - {entry.Continent} - {entry.Population}");
                }
                else
                {
                    res.Add(name, entry);
                }
            }

            return res;
        }

        private void ProcessRows()
        {
            _logger.WriteInfo("Initialize misc storage...");
            var rows = _dataSource.GetReader().GetRows().GroupBy(r => r.Version)
                .ToDictionary(gr => gr.Key);

            _logger.WriteInfo("Initialize russian regions info...");
            _russianRegions = rows[RowVersion.Translation].ToDictionary(r => r[Field.English], r => r[Field.Russian]);
            _logger.WriteInfo("Initialize US & Canada states abbreviations info...");
            _states = rows[RowVersion.State].ToDictionary(r => r[Field.Abbreviation], r => r[Field.Name]);
            _logger.WriteInfo("Initialize statistics..");
            _stats = CreateStats(rows[RowVersion.StatsBase], rows[RowVersion.StatsEx].ToArray());
        }

        private readonly struct StatEntry
        {
            public StatEntry(string continent, string code3, string iso3, string population, string name)
            {
                Continent = continent;
                Code3 = string.IsNullOrEmpty(code3) ? 0 : int.Parse(code3);
                Iso3 = iso3;
                Population = population.AsLong();
                Name = name;
            }

            public int Code3 { get; }
            public string Continent { get; }

            public string Iso3 { get; }
            public string Name { get; }
            public long Population { get; }
        }
    }
}