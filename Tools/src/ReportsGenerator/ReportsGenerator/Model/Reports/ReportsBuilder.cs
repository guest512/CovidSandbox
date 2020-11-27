using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReportsGenerator.Data;
using ReportsGenerator.Data.DataSources;
using ReportsGenerator.Model.Reports.Intermediate;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Model.Reports
{
    /// <summary>
    /// Builds reports from bunch of <see cref="Entry"/> objects.
    /// </summary>
    public class ReportsBuilder
    {
        private readonly Dictionary<string, CountryReport> _countryReports = new();
        private readonly Dictionary<DateTime, DayReport> _dayReports = new();
        private readonly List<Entry> _entries = new();
        private readonly ConcurrentDictionary<string, StatsInfoReportWalker> _statsInfoWalkers = new();
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, BasicReportsWalker> _reportsWalkers = new();
        private readonly Dictionary<string, List<ModelDataReport>> _cachedDataReports = new();
        private readonly Dictionary<string, List<ModelMetadataReport>> _cachedStatsReports = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportsBuilder"/> class.
        /// </summary>
        /// <param name="logger">A <see cref="ILogger"/> instance.</param>
        public ReportsBuilder(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Initializes cache from provided cache data source.
        /// </summary>
        /// <param name="cacheDataSource">Cache data source.</param>
        /// <param name="lastDay">Gets a last day found in <paramref name="cacheDataSource"/>.</param>
        public void InitializeCache(ModelCacheDataSource cacheDataSource, out DateTime lastDay)
        {
            lastDay = DateTime.MinValue;
            foreach (var row in cacheDataSource.GetReader().GetRows())
            {
                var country = row[FieldId.CountryRegion];
                if (!_cachedDataReports.ContainsKey(country))
                {
                    _cachedDataReports.Add(country, new List<ModelDataReport>());
                    _cachedStatsReports.Add(country, new List<ModelMetadataReport>());
                }
                switch (row.Version)
                {
                    case RowVersion.ModelCacheData:
                        var dataReport = new ModelDataReport(row);
                        if (dataReport.Day > lastDay)
                            lastDay = dataReport.Day;

                        _cachedDataReports[country].Add(dataReport);
                        break;

                    case RowVersion.ModelCacheMetaData:
                        _cachedStatsReports[country].Add(new ModelMetadataReport(row));
                        break;
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of countries for which reports could be build.
        /// </summary>
        public IEnumerable<string> AvailableCountries => _reportsWalkers.Keys;

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of dates for which reports could be build.
        /// </summary>
        public IEnumerable<DateTime> AvailableDates => _reportsWalkers
            .SelectMany(lr => new[] { lr.Value.StartDay, lr.Value.LastDay }).GetContinuousDateRange();

        /// <summary>
        /// Adds <see cref="Entry"/> objects to the build that should be processed during the <see cref="Build"/> function call.
        /// </summary>
        /// <remarks>
        /// This method also removes all previously generated reports for each country and day that mentioned in new entries.
        /// </remarks>
        /// <param name="rows">Entries to add.</param>
        public void AddEntries(IEnumerable<Entry> rows)
        {
            var newRows = rows as Entry[] ?? rows.ToArray();

            foreach (var country in newRows.Select(_ => _.CountryRegion))
            {
                _countryReports.Remove(country);
                _reportsWalkers.TryRemove(country, out _);
            }

            foreach (var day in newRows.Select(_ => _.LastUpdate.Date))
            {
                _dayReports.Remove(day);
                _reportsWalkers.Clear();
            }

            _entries.AddRange(newRows);
        }

        /// <summary>
        /// Converts <see cref="Entry"/> to internal intermediate reports.
        /// Results can be extracted via properties: <see cref="AvailableDates"/> and <see cref="AvailableCountries"/>.
        /// Also they are available through functions: <see cref="GetCountryReport"/>, <see cref="GetCountryStats"/>, and <see cref="GetDayReport"/>.
        /// </summary>
        /// <param name="statsProvider"></param>
        public void Build(IStatsProvider statsProvider)
        {
            var uniqueEntries = _entries.ToArray();
            var countriesList = uniqueEntries.Select(x => x.CountryRegion).Concat(_cachedDataReports.Keys).Distinct();

            Parallel.ForEach(countriesList, country =>
            {
                if (country == "Others")
                    return;

                _logger.WriteInfo($"--Processing {country}...");

                var countryEntries = uniqueEntries.Where(x => x.CountryRegion == country);
                var basicReports = new List<BasicReport>();
                var statsInfoWalkerBuilder = new StatsInfoReportWalkerBuilder(country, statsProvider);

                if (_cachedDataReports.ContainsKey(country))
                {
                    basicReports.AddRange(_cachedDataReports[country].Select(CreateBasicReport));

                    foreach (var cachedMetadata in _cachedStatsReports[country]
                        .Where(r => !string.IsNullOrEmpty(r.Province)))
                    {
                        if (string.IsNullOrEmpty(cachedMetadata.County))
                            statsInfoWalkerBuilder.AddProvince(cachedMetadata.Province, cachedMetadata.StatsName);
                        else
                            statsInfoWalkerBuilder.AddCounty(cachedMetadata.Province, cachedMetadata.County,
                                cachedMetadata.StatsName);
                    }
                }

                if (country == "Russia")
                {
                    countryEntries = countryEntries.ToArray();
                    var dates = uniqueEntries.Select(x => x.LastUpdate.Date).Distinct().OrderBy(_ => _);
                    foreach (var day in dates)
                    {
                        var dayCountryEntries = countryEntries.Where(x => x.LastUpdate.Date == day).ToArray();

                        if (dayCountryEntries.Any(x => x.Origin == Origin.Yandex))
                        {
                            dayCountryEntries = dayCountryEntries.Where(x => x.Origin == Origin.Yandex).ToArray();
                        }

                        foreach (var dayCountryEntry in dayCountryEntries)
                        {
                            ProcessEntry(dayCountryEntry, basicReports, statsInfoWalkerBuilder);
                        }
                    }
                }
                else
                {
                    foreach (var countryEntry in countryEntries)
                    {
                        ProcessEntry(countryEntry, basicReports, statsInfoWalkerBuilder);
                    }
                }

                var statsInfoWalker = statsInfoWalkerBuilder.Build();
                _reportsWalkers.TryAdd(country, new BasicReportsWalker(basicReports, statsInfoWalker));
                _statsInfoWalkers.TryAdd(country, statsInfoWalker);
            });
        }

        private static void ProcessEntry(Entry entry, ICollection<BasicReport> dataCollection,
            StatsInfoReportWalkerBuilder metadataBuilder)
        {
            dataCollection.Add(CreateBasicReport(entry));

            switch (entry.IsoLevel)
            {
                case IsoLevel.ProvinceState:
                    metadataBuilder.AddProvince(entry.ProvinceState, entry.StatsName);
                    break;

                case IsoLevel.County:
                    metadataBuilder.AddCounty(entry.ProvinceState, entry.County, entry.StatsName);
                    break;
            }
        }

        /// <summary>
        /// Gets all data reports as <see cref="IFormattableReport{TRow,TName}"/> objects used to build reports.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="IFormattableReport{TRow,TName}"/>.</returns>
        public IEnumerable<IFormattableReport<int, string>> DumpModelData() =>
            _reportsWalkers.SelectMany(
                rw => rw.Value.DumpReports().Select(br => new ModelDataReport(rw.Key, br)));

        /// <summary>
        /// Gets all metadata reports as <see cref="IFormattableReport{TRow,TName}"/> objects used to build reports.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="IFormattableReport{TRow,TName}"/>.</returns>
        public IEnumerable<IFormattableReport<int, string>> DumpModelMetadata()
        {
            foreach (var (countryName, walker) in _statsInfoWalkers)
            {
                yield return new ModelMetadataReport(countryName, walker.Country);
                foreach (var provinceRoot in walker.Provinces)
                {
                    yield return new ModelMetadataReport(countryName, provinceRoot);
                    foreach (var countyRoot in walker.GetCounties(provinceRoot.Name))
                    {
                        yield return new ModelMetadataReport(countryName, countyRoot);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the <see cref="CountryReport"/> for the particular country name.
        /// </summary>
        /// <param name="countryName">Country name to retrieve.</param>
        /// <returns>The <see cref="CountryReport"/> for the particular country name.</returns>
        public CountryReport GetCountryReport(string countryName)
        {
            if (_countryReports.ContainsKey(countryName))
                return _countryReports[countryName];

            var countyrReport = new CountryReport(countryName, _reportsWalkers[countryName], GetCountryStats(countryName));
            _countryReports.Add(countryName, countyrReport);

            return _countryReports[countryName];
        }

        /// <summary>
        /// Returns the <see cref="StatsInfoReportWalker"/> for the particular country name.
        /// </summary>
        /// <param name="countryName">Country name to retrieve.</param>
        /// <returns>The <see cref="StatsInfoReportWalker"/> for the particular country name.</returns>
        public StatsInfoReportWalker GetCountryStats(string countryName) => _statsInfoWalkers[countryName];

        /// <summary>
        /// Returns the <see cref="DayReport"/> for the particular day.
        /// </summary>
        /// <param name="day">Day to retrieve.</param>
        /// <returns>The <see cref="DayReport"/> for the particular day.</returns>
        public DayReport GetDayReport(DateTime day)
        {
            day = day.Date;
            if (_dayReports.ContainsKey(day))
                return _dayReports[day];

            var dayReport = new DayReport(day, _reportsWalkers);
            _dayReports.Add(day, dayReport);

            return _dayReports[day];
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

        private static BasicReport CreateBasicReport(Entry entry)
        {
            var template = new BasicReport
            {
                Day = entry.LastUpdate,
                Total = new Metrics(entry.Confirmed, entry.Active, entry.Recovered, entry.Deaths)
            };

            return entry.IsoLevel switch
            {
                IsoLevel.CountryRegion => template with { Name = entry.CountryRegion },
                IsoLevel.ProvinceState => template with { Name = entry.ProvinceState, Parent = entry.CountryRegion },
                IsoLevel.County => template with { Name = entry.County, Parent = entry.ProvinceState },

                _ => throw new ArgumentOutOfRangeException(nameof(entry.IsoLevel), $"Unknown ISO level of {entry}")
            };
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

            public DateTime Day { get; }
            public string Country { get; }
            public string Province { get; }
            public string County { get; }
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

            public string Country { get; }
            public string Province { get; }
            public string County { get; }
            public string Continent { get; }
            public long Population { get; }
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