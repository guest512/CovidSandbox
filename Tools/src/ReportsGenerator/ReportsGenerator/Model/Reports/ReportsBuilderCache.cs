using System;
using System.Collections.Generic;
using System.Linq;
using ReportsGenerator.Data;
using ReportsGenerator.Data.DataSources;
using ReportsGenerator.Model.Reports.Intermediate;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Model.Reports
{
    /// <summary>
    /// Represents a global cache for <see cref="ReportsBuilder"/>.
    /// At the moment this object internally split on two parts: data that it has from initialization process, data that was added later.
    /// You can use only the data that was get from initialization! All new data can be used only for saving operations.
    /// </summary>
    public class ReportsBuilderCache
    {
        private readonly Dictionary<string, List<ModelDataReport>> _cachedNewDataReports = new();
        private readonly Dictionary<string, List<ModelMetadataReport>> _cachedNewStatsReports = new();
        private readonly Dictionary<string, List<ModelDataReport>> _cachedOldDataReports = new();
        private readonly Dictionary<string, List<ModelMetadataReport>> _cachedOldStatsReports = new();
        private readonly List<string> _countries = new();
        private readonly ILogger _logger;
        private readonly object _lockerData = new();
        private readonly object _lockerStats = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportsBuilderCache"/> class.
        /// </summary>
        /// <param name="statsProvider">A <see cref="IStatsProvider"/> object to lookup for new countries/provinces/counties information.</param>
        /// <param name="logger">A <see cref="ILogger"/> instance.</param>
        public ReportsBuilderCache(IStatsProvider statsProvider, ILogger logger)
        {
            StatsProvider = statsProvider;
            _logger = logger;
        }

        /// <summary>
        /// Gets countries list that cache knows from initialization.
        /// This list doesn't include countries that were added via Add.. functions.
        /// </summary>
        ///
        public IEnumerable<string> Countries => _countries;

        /// <summary>
        /// Gets a last day found during the cache initialization.
        /// </summary>
        public DateTime LastDay { get; private set; } = DateTime.MinValue;

        /// <summary>
        /// Gets a <see cref="IStatsProvider"/> used by this cache for new records.
        /// </summary>
        public IStatsProvider StatsProvider { get; }

        /// <summary>
        /// Adds county, if needed, to the cache.
        /// </summary>
        /// <param name="country">Country where county is belonged to.</param>
        /// <param name="county">County name.</param>
        /// <param name="statsName">County name to lookup in <see cref="IStatsProvider"/>.</param>
        /// <param name="province">Province where county is belonged to.</param>
        public void AddCountyMetadata(string country, string province, string county, string statsName)
        {
            AddProvinceMetadata(country, province, statsName[(statsName.IndexOf(',') + 2)..]);

            if (_countries.Contains(country) &&
                _cachedOldStatsReports[country].Any(r => r.Province == province && r.County == county) ||
                _cachedNewStatsReports[country].Any(r => r.Province == province && r.County == county))
                return;

            _logger.WriteInfo($"CACHE: New {country} county {province}, {county} metadata added!");
            _cachedNewStatsReports[country].Add(
                new ModelMetadataReport(country,
                    new StatsInfoReport(
                        country,
                        county,
                        province,
                        StatsProvider.LookupContinentName(statsName),
                        StatsProvider.LookupPopulation(statsName),
                        statsName)));
        }

        /// <summary>
        /// Adds new data report, to the cache.
        /// </summary>
        /// <param name="country">Country where for which data is added.</param>
        /// <param name="report">Data to add.</param>
        public void AddNewDataReport(string country, BasicReport report)
        {
            if (!_cachedNewDataReports.ContainsKey(country))
            {
                lock (_lockerData)
                {
                    if (!_cachedNewDataReports.ContainsKey((country)))
                    {
                        _cachedNewDataReports.Add(country, new List<ModelDataReport>());
                    }
                }
            }

            // Too verbose.
            //_logger.WriteInfo($"CACHE: New {country} data for {report.Day} - {report.Name} added!");

            _cachedNewDataReports[country].Add(new ModelDataReport(country, report));
        }

        /// <summary>
        /// Adds province, if needed, to the cache.
        /// </summary>
        /// <param name="country">Country where province is belonged to.</param>
        /// <param name="statsName">Province name to lookup in <see cref="IStatsProvider"/>.</param>
        /// <param name="province">Province name.</param>
        public void AddProvinceMetadata(string country, string province, string statsName)
        {
            AddCountryMetadata(country);

            if (_countries.Contains(country) &&
                _cachedOldStatsReports[country].Any(r => r.Province == province) ||
                _cachedNewStatsReports[country].Any(r => r.Province == province))
                return;

            _logger.WriteInfo($"CACHE: New {country} province {province} metadata added!");
            _cachedNewStatsReports[country].Add(
                new ModelMetadataReport(country,
                    new StatsInfoReport(
                        country,
                        province,
                        country,
                        StatsProvider.LookupContinentName(statsName),
                        StatsProvider.LookupPopulation(statsName),
                        statsName)));
        }

        /// <summary>
        /// Gets data reports as <see cref="IFormattableReport{TRow,TName}"/> objects used to build reports.
        /// </summary>
        /// <param name="deltaOnly">Flag indicating whether or not only fresh added reports should be returned.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="IFormattableReport{TRow,TName}"/>.</returns>
        public IEnumerable<IFormattableReport<int, string>> DumpData(bool deltaOnly)
        {
            if (!deltaOnly)
            {
                foreach (var dataReport in _cachedOldDataReports.Values.SelectMany(dr => dr))
                {
                    yield return dataReport;
                }
            }

            foreach (var dataReport in _cachedNewDataReports.Values.SelectMany(dr => dr))
            {
                yield return dataReport;
            }
        }

        /// <summary>
        /// Gets metadata reports as <see cref="IFormattableReport{TRow,TName}"/> objects used to build reports.
        /// </summary>
        /// <param name="deltaOnly">Flag indicating whether or not only fresh added reports should be returned.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="IFormattableReport{TRow,TName}"/>.</returns>
        public IEnumerable<IFormattableReport<int, string>> DumpMetadata(bool deltaOnly)
        {
            if (!deltaOnly)
            {
                foreach (var dataReport in _cachedOldStatsReports.Values.SelectMany(dr => dr))
                {
                    yield return dataReport;
                }
            }

            foreach (var dataReport in _cachedNewStatsReports.Values.SelectMany(dr => dr))
            {
                yield return dataReport;
            }
        }

        /// <summary>
        /// Gets data reports for the particular <paramref name="country"/> from the usable part of the cache.
        /// </summary>
        /// <param name="country">Country name for which reports should be returned.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="BasicReport"/>.</returns>
        public IEnumerable<BasicReport> GetCachedDataReports(string country) => _cachedOldDataReports[country].Select(CreateBasicReport);

        /// <summary>
        /// Gets metadata reports for the particular <paramref name="country"/> from the usable part of the cache.
        /// </summary>
        /// <param name="country">Country name for which reports should be returned.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="BasicMetadataReport"/>.</returns>
        public IEnumerable<BasicMetadataReport> GetCachedMetadataReports(string country) => _cachedOldStatsReports[country].Select(CreateBasicReport);

        /// <summary>
        /// Initializes cache with data provided by the <paramref name="cacheDataSource"/>. This data will be used in Get.. functions.
        /// </summary>
        /// <param name="cacheDataSource">Cache data source.</param>
        public void Initialize(IDataSource cacheDataSource)
        {
            var dataRows = 0;
            var metadataRows = 0;

            _logger.IndentIncrease();

            _logger.WriteInfo("Cache initialization started");

            foreach (var row in cacheDataSource.GetReader().GetRows())
            {
                var country = row[FieldId.CountryRegion];
                if (!_countries.Contains(country))
                {
                    _logger.WriteInfo($"Found records about {country}");
                    _countries.Add(country);
                    _cachedOldDataReports.Add(country, new List<ModelDataReport>());
                    _cachedOldStatsReports.Add(country, new List<ModelMetadataReport>());
                }
                switch (row.Version)
                {
                    case RowVersion.ModelCacheData:
                        dataRows++;
                        var dataReport = new ModelDataReport(row);
                        if (dataReport.Day > LastDay)
                            LastDay = dataReport.Day;

                        _cachedOldDataReports[country].Add(dataReport);
                        break;

                    case RowVersion.ModelCacheMetaData:
                        metadataRows++;
                        _cachedOldStatsReports[country].Add(new ModelMetadataReport(row));
                        break;
                }
            }

            _logger.WriteInfo($"Cache initialized with {dataRows} data rows and {metadataRows} metadata rows");

            _logger.IndentDecrease();
        }

        private static BasicReport CreateBasicReport(ModelDataReport cachedReport)
        {
            var template = new BasicReport
            {
                Day = cachedReport.Day,
                Total = cachedReport.Total
            };

            if (!string.IsNullOrEmpty(cachedReport.County))
                return template with { Name = cachedReport.County, Parent = cachedReport.Province };

            if (!string.IsNullOrEmpty(cachedReport.Province))
                return template with { Name = cachedReport.Province, Parent = cachedReport.Country };

            return template with { Name = cachedReport.Country };
        }

        private static BasicMetadataReport CreateBasicReport(ModelMetadataReport cachedReport) =>
            new(cachedReport.Country, cachedReport.Province, cachedReport.County, cachedReport.StatsName);

        private void AddCountryMetadata(string country)
        {
            if (!_cachedNewStatsReports.ContainsKey(country))
            {
                lock (_lockerStats)
                {
                    if (!_cachedNewStatsReports.ContainsKey(country))
                    {
                        _cachedNewStatsReports.Add(country, new List<ModelMetadataReport>());
                    }
                }
            }

            if (_countries.Contains(country) &&
                _cachedOldStatsReports[country].Any(r => r.Country == country && r.Province == string.Empty) ||
                _cachedNewStatsReports[country].Any(r => r.Country == country && r.Province == string.Empty))
                return;

            _logger.WriteInfo($"CACHE: New country {country} metadata added!");
            var statsName = StatsProvider.GetCountryStatsName(country);
            _cachedNewStatsReports[country].Add(new ModelMetadataReport(
                country,
                new StatsInfoReport(
                    country,
                    country,
                    Continent: StatsProvider.LookupContinentName(statsName),
                    Population: StatsProvider.LookupPopulation(statsName),
                    StatsName: statsName)));
        }

        private class ModelDataReport : IFormattableReport<int, string>
        {
            private static readonly string[] FormattableReportProperties =
            {
                "Day",
                "Country",
                "Province",
                "County",
                "Total"
            };

            public ModelDataReport(string countryName, BasicReport report)
            {
                Country = countryName;

                if (report.Parent == string.Empty)
                {
                    Province = County = string.Empty;
                }
                else if (report.Parent == countryName)
                {
                    Province = report.Name;
                    County = string.Empty;
                }
                else
                {
                    Province = report.Parent;
                    County = report.Name;
                }

                Day = report.Day;
                Total = report.Total;
            }

            public ModelDataReport(Row cachedRow)
            {
                Country = cachedRow[FieldId.CountryRegion];
                Province = cachedRow[FieldId.ProvinceState];
                County = cachedRow[FieldId.Admin2];
                Day = cachedRow[FieldId.LastUpdate].AsDate("dd-MM-yyyy");
                Total = new Metrics(
                    cachedRow[FieldId.Confirmed].AsLong(),
                    cachedRow[FieldId.Active].AsLong(),
                    cachedRow[FieldId.Recovered].AsLong(),
                    cachedRow[FieldId.Deaths].AsLong());
            }

            public string Country { get; }
            public string County { get; }
            public DateTime Day { get; }
            public string Province { get; }
            public Metrics Total { get; }

            #region IFormattableReport

            IEnumerable<string> IFormattableReport<int, string>.Name { get; } = new[] { "data" };

            IEnumerable<string> IFormattableReport<int, string>.Properties { get; } = FormattableReportProperties;

            ReportType IFormattableReport<int, string>.ReportType { get; } = ReportType.Model;
            IEnumerable<int> IFormattableReport<int, string>.RowIds { get; } = new[] { 0 };

            object IFormattableReport<int, string>.GetValue(string property, int key) => property switch
            {
                "Day" => Day,
                "Country" => Country,
                "Province" => Province,
                "County" => County,
                "Total" => Total,
                _ => throw new ArgumentOutOfRangeException(nameof(property), property, null)
            };

            #endregion IFormattableReport
        }

        private class ModelMetadataReport : IFormattableReport<int, string>
        {
            private static readonly string[] FormattableReportProperties =
            {
                "Country",
                "Province",
                "County",
                "Continent",
                "Population",
                "StatsName"
            };

            public ModelMetadataReport(string countryName, StatsInfoReport report)
            {
                Country = countryName;

                if (report.Parent == string.Empty)
                {
                    Province = County = string.Empty;
                }
                else if (report.Parent == countryName)
                {
                    Province = report.Name;
                    County = string.Empty;
                }
                else
                {
                    Province = report.Parent;
                    County = report.Name;
                }

                Continent = report.Continent;
                Population = report.Population;
                StatsName = report.StatsName;
            }

            public ModelMetadataReport(Row cachedRow)
            {
                Country = cachedRow[FieldId.CountryRegion];
                Province = cachedRow[FieldId.ProvinceState];
                County = cachedRow[FieldId.Admin2];
                Continent = cachedRow[FieldId.ContinentName];
                Population = cachedRow[FieldId.Population].AsLong();
                StatsName = cachedRow[FieldId.CombinedKey];
            }

            public string Continent { get; }
            public string Country { get; }
            public string County { get; }
            public long Population { get; }
            public string Province { get; }
            public string StatsName { get; }

            #region IFormattableReport

            IEnumerable<string> IFormattableReport<int, string>.Name => new[] { "metadata" };

            IEnumerable<string> IFormattableReport<int, string>.Properties => FormattableReportProperties;

            ReportType IFormattableReport<int, string>.ReportType => ReportType.Model;
            IEnumerable<int> IFormattableReport<int, string>.RowIds => new[] { 0 };

            object IFormattableReport<int, string>.GetValue(string property, int key) => property switch
            {
                "Country" => Country,
                "Province" => Province,
                "County" => County,
                "Continent" => Continent,
                "Population" => Population,
                "StatsName" => StatsName,
                _ => throw new ArgumentOutOfRangeException(nameof(property), property, null)
            };

            #endregion IFormattableReport
        }
    }
}
