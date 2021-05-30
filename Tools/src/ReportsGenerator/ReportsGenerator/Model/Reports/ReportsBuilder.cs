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
        private readonly ReportsBuilderCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportsBuilder"/> class.
        /// </summary>
        /// <param name="statsProvider">A <see cref="IStatsProvider"/> object to lookup for countries information.</param>
        /// <param name="logger">A <see cref="ILogger"/> instance.</param>
        public ReportsBuilder(IStatsProvider statsProvider, ILogger logger)
        {
            _cache = new ReportsBuilderCache(statsProvider, logger);
            _logger = logger;
        }

        /// <summary>
        /// Initializes cache from provided cache data source.
        /// </summary>
        /// <param name="cacheDataSource">Cache data source.</param>
        /// <param name="lastDay">Gets a last day found in <paramref name="cacheDataSource"/>.</param>
        public void InitializeCache(ModelCacheDataSource cacheDataSource, out DateTime lastDay)
        {
            _cache.Initialize(cacheDataSource);
            lastDay = _cache.LastDay;
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
        public void Build()
        {
            var uniqueEntries = _entries.ToArray();
            var countriesList = uniqueEntries.Select(x => x.CountryRegion).Concat(_cache.Countries).Distinct();

            Parallel.ForEach(countriesList, country =>
            {
                if (country == "Others")
                    return;

                _logger.WriteInfo($"--Processing {country}...");

                var countryEntries = uniqueEntries.Where(x => x.CountryRegion == country);
                var basicReports = new List<BasicReport>();
                var statsInfoWalkerBuilder = new StatsInfoReportWalkerBuilder(country, _cache.StatsProvider);

                if (_cache.Countries.Contains(country))
                {
                    basicReports.AddRange(_cache.GetCachedDataReports(country));

                    foreach (var (_, province, county, statsName) in _cache.GetCachedMetadataReports(country)
                        .Where(r => !string.IsNullOrEmpty(r.Province)))
                    {
                        if (string.IsNullOrEmpty(county))
                            statsInfoWalkerBuilder.AddProvince(province, statsName);
                        else
                            statsInfoWalkerBuilder.AddCounty(province, county, statsName);
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
                            ProcessEntry(dayCountryEntry, basicReports, statsInfoWalkerBuilder, _cache);
                        }
                    }
                }
                else
                {
                    foreach (var countryEntry in countryEntries)
                    {
                        ProcessEntry(countryEntry, basicReports, statsInfoWalkerBuilder, _cache);
                    }
                }

                var statsInfoWalker = statsInfoWalkerBuilder.Build();
                _reportsWalkers.TryAdd(country, new BasicReportsWalker(basicReports, statsInfoWalker));
                _statsInfoWalkers.TryAdd(country, statsInfoWalker);
            });
        }

        private static void ProcessEntry(Entry entry, ICollection<BasicReport> dataCollection,
            StatsInfoReportWalkerBuilder metadataBuilder, ReportsBuilderCache builderCache)
        {
            var dataReport = CreateBasicReport(entry);
            dataCollection.Add(dataReport);
            builderCache.AddNewDataReport(entry.CountryRegion, dataReport);

            switch (entry.IsoLevel)
            {
                case IsoLevel.ProvinceState:
                    metadataBuilder.AddProvince(entry.ProvinceState, entry.StatsName);
                    builderCache.AddProvinceMetadata(entry.CountryRegion, entry.ProvinceState, entry.StatsName);
                    break;

                case IsoLevel.County:
                    metadataBuilder.AddCounty(entry.ProvinceState, entry.County, entry.StatsName);
                    builderCache.AddCountyMetadata(entry.CountryRegion, entry.ProvinceState, entry.County,
                        entry.StatsName);

                    break;
            }
        }

        /// <summary>
        /// Gets all data reports as <see cref="IFormattableReport{TRow,TName}"/> objects used to build reports.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="IFormattableReport{TRow,TName}"/>.</returns>
        public IEnumerable<IFormattableReport<int, string>> DumpModelData(bool newOnly = true) =>
            _cache.DumpData(newOnly);

        /// <summary>
        /// Gets all metadata reports as <see cref="IFormattableReport{TRow,TName}"/> objects used to build reports.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="IFormattableReport{TRow,TName}"/>.</returns>
        public IEnumerable<IFormattableReport<int, string>> DumpModelMetadata(bool newOnly = true) =>
            _cache.DumpMetadata(newOnly);

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
    }
}