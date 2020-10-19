using ReportsGenerator.Model.Reports.Intermediate;
using ReportsGenerator.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportsGenerator.Model.Reports
{
    public class ReportsBuilder
    {
        private readonly Dictionary<string, CountryReport> _countryReports = new Dictionary<string, CountryReport>();
        private readonly Dictionary<DateTime, DayReport> _dayReports = new Dictionary<DateTime, DayReport>();
        private readonly ConcurrentDictionary<string, StatsReport> _graphStructures = new ConcurrentDictionary<string, StatsReport>();
        private readonly ConcurrentDictionary<string, LinkedReport> _linkedReports = new ConcurrentDictionary<string, LinkedReport>();
        private readonly ILogger _logger;
        private readonly List<Entry> entries = new List<Entry>();

        public ReportsBuilder(ILogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<string> AvailableCountries => _linkedReports.Select(lr => lr.Value.Name).Distinct();
        public IEnumerable<DateTime> AvailableDates => _linkedReports.SelectMany(lr => lr.Value.GetAvailableDates()).Distinct();

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

            entries.AddRange(newRows);
        }

        public void Build(IStatsProvider statsProvider)
        {
            var uniqueEntries = entries.Distinct().ToArray();

            var countriesList = uniqueEntries.Select(x => x.CountryRegion).Distinct().ToArray();
            var dates = uniqueEntries.Select(x => x.LastUpdate.Date).Distinct().OrderBy(_ => _).ToArray();

            Parallel.ForEach(countriesList, country =>
            {
                if (country == "Others")
                    return;

                _logger.WriteInfo($"--Processing {country}...");

                var countryEntries = uniqueEntries.Where(x => x.CountryRegion == country).ToArray();
                var graphBuilder = new ReportsGraphBuilder(country, _logger);
                var graphStructure = new StatsReport(country, statsProvider.GetCountryStatsName(country), statsProvider);

                if (country == "Russia")
                {
                    foreach (var day in dates)
                    {
                        var dayCountryEntries = countryEntries.Where(x => x.LastUpdate.Date == day).ToArray();

                        if (dayCountryEntries.Any(x => x.Origin == Origin.JHopkins) &&
                            dayCountryEntries.Any(x => x.Origin == Origin.Yandex))
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

                            graphBuilder.Reports.Add(CreateBasicReport(dayCountryEntry));
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

                        graphBuilder.Reports.Add(CreateBasicReport(countryEntry));
                    }
                }

                _linkedReports.TryAdd(country, graphBuilder.Build(graphStructure));
                _graphStructures.TryAdd(country, graphStructure);
            });
        }

        public CountryReport GetCountryReport(string countryName)
        {
            if (_countryReports.ContainsKey(countryName))
                return _countryReports[countryName];

            var countyrReport = new CountryReport(countryName, _linkedReports[countryName]);
            _countryReports.Add(countryName, countyrReport);

            return _countryReports[countryName];
        }

        public StatsReport GetCountryStats(string countryName) => _graphStructures[countryName];

        public DayReport GetDayReport(DateTime day)
        {
            day = day.Date;
            if (_dayReports.ContainsKey(day))
                return _dayReports[day];

            var dayReport = new DayReport(day, _linkedReports);
            _dayReports.Add(day, dayReport);

            return _dayReports[day];
        }

        private static BasicReport CreateBasicReport(Entry entry)
        {
            return entry.IsoLevel switch
            {
                IsoLevel.CountryRegion => new BasicReport(entry.CountryRegion,
                    entry.LastUpdate,
                    Metrics.FromEntry(entry)),

                IsoLevel.ProvinceState => new BasicReport(entry.ProvinceState,
                    entry.CountryRegion,
                    entry.LastUpdate,
                    Metrics.FromEntry(entry)),

                IsoLevel.County => new BasicReport(entry.County,
                    entry.ProvinceState, entry.LastUpdate,
                    Metrics.FromEntry(entry)),

                _ => throw new ArgumentOutOfRangeException(nameof(entry.IsoLevel), $"Unknown ISO level of {entry}")
            };
        }
    }
}