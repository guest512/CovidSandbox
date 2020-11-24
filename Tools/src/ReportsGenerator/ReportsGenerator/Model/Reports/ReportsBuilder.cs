using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReportsGenerator.Model.Reports.Intermediate;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Model.Reports
{
    /// <summary>
    /// Builds reports from bunch of <see cref="Entry"/> objects.
    /// </summary>
    public class ReportsBuilder
    {
        private readonly Dictionary<string, CountryReport> _countryReports = new Dictionary<string, CountryReport>();
        private readonly Dictionary<DateTime, DayReport> _dayReports = new Dictionary<DateTime, DayReport>();
        private readonly ConcurrentDictionary<string, StatsReport> _graphStructures = new ConcurrentDictionary<string, StatsReport>();
        private readonly ConcurrentDictionary<string, BasicReportsWalker> _linkedReports = new ConcurrentDictionary<string, BasicReportsWalker>();
        private readonly ILogger _logger;
        private readonly List<Entry> _entries = new List<Entry>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportsBuilder"/> class.
        /// </summary>
        /// <param name="logger">A <see cref="ILogger"/> instance.</param>
        public ReportsBuilder(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of countries for which reports could be build.
        /// </summary>
        public IEnumerable<string> AvailableCountries => _linkedReports.Keys;

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of dates for which reports could be build.
        /// </summary>
        public IEnumerable<DateTime> AvailableDates => _linkedReports.SelectMany(lr => lr.Value.GetAvailableDates()).Distinct();

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
                _linkedReports.TryRemove(country, out _);
            }

            foreach (var day in newRows.Select(_ => _.LastUpdate.Date))
            {
                _dayReports.Remove(day);
                _linkedReports.Clear();
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
            var countriesList = uniqueEntries.Select(x => x.CountryRegion).Distinct();

            Parallel.ForEach(countriesList, country =>
            {
                if (country == "Others")
                    return;

                _logger.WriteInfo($"--Processing {country}...");

                var countryEntries = uniqueEntries.Where(x => x.CountryRegion == country);
                var basicReports = new List<BasicReport>();
                var graphStructure = new StatsReport(country, statsProvider.GetCountryStatsName(country), statsProvider);

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
                            switch (dayCountryEntry.IsoLevel)
                            {
                                case IsoLevel.ProvinceState:
                                    graphStructure.AddProvince(dayCountryEntry.ProvinceState, dayCountryEntry.StatsName);
                                    break;

                                case IsoLevel.County:
                                    graphStructure.AddCounty(dayCountryEntry.County, dayCountryEntry.StatsName,
                                        dayCountryEntry.ProvinceState);
                                    break;
                            }

                            basicReports.Add(CreateBasicReport(dayCountryEntry));
                        }
                    }
                }
                else
                {
                    foreach (var countryEntry in countryEntries)
                    {
                        switch (countryEntry.IsoLevel)
                        {
                            case IsoLevel.ProvinceState:
                                graphStructure.AddProvince(countryEntry.ProvinceState, countryEntry.StatsName);
                                break;

                            case IsoLevel.County:
                                graphStructure.AddCounty(countryEntry.County, countryEntry.StatsName, countryEntry.ProvinceState);
                                break;
                        }

                        basicReports.Add(CreateBasicReport(countryEntry));
                    }
                }

                _linkedReports.TryAdd(country, new BasicReportsWalker(basicReports, graphStructure));
                _graphStructures.TryAdd(country, graphStructure);
            });
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

            var countyrReport = new CountryReport(countryName, _linkedReports[countryName], GetCountryStats(countryName));
            _countryReports.Add(countryName, countyrReport);

            return _countryReports[countryName];
        }

        /// <summary>
        /// Returns the <see cref="StatsReport"/> for the particular country name.
        /// </summary>
        /// <param name="countryName">Country name to retrieve.</param>
        /// <returns>The <see cref="StatsReport"/> for the particular country name.</returns>
        public StatsReport GetCountryStats(string countryName) => _graphStructures[countryName];

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

            var dayReport = new DayReport(day, _linkedReports);
            _dayReports.Add(day, dayReport);

            return _dayReports[day];
        }

        private static BasicReport CreateBasicReport(Entry entry) => entry.IsoLevel switch
        {
            IsoLevel.CountryRegion => new BasicReport
            {
                Name = entry.CountryRegion,
                Day = entry.LastUpdate,
                Total = Metrics.FromEntry(entry)
            },

            IsoLevel.ProvinceState => new BasicReport
            {
                Name = entry.ProvinceState,
                Parent = entry.CountryRegion,
                Day = entry.LastUpdate,
                Total = Metrics.FromEntry(entry)
            },

            IsoLevel.County => new BasicReport
            {
                Name = entry.County,
                Parent = entry.ProvinceState,
                Day = entry.LastUpdate,
                Total = Metrics.FromEntry(entry)
            },

            _ => throw new ArgumentOutOfRangeException(nameof(entry.IsoLevel), $"Unknown ISO level of {entry}")
        };
    }
}