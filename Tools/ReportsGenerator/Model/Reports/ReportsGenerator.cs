using CovidSandbox.Model.Reports.Intermediate;
using CovidSandbox.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CovidSandbox.Model.Reports
{
    public class ReportsGenerator
    {
        private readonly Dictionary<string, CountryReport> _countryReports = new Dictionary<string, CountryReport>();
        private readonly List<Entry> _data = new List<Entry>();
        private readonly Dictionary<DateTime, DayReport> _dayReports = new Dictionary<DateTime, DayReport>();
        private readonly ConcurrentDictionary<string, LinkedReport> _linkedReports = new ConcurrentDictionary<string, LinkedReport>();
        private readonly ILogger _logger;

        public ReportsGenerator(ILogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<string> AvailableCountries => _linkedReports.Select(lr => lr.Value.Name).Distinct();
        public IEnumerable<DateTime> AvailableDates => _linkedReports.SelectMany(lr => lr.Value.GetAvailableDates()).Distinct();

        public void AddEntries(IEnumerable<Entry> rows)
        {
            var newRows = rows as Entry[] ?? rows.ToArray();
            _data.AddRange(newRows);

            foreach (var country in newRows.Select(_ => _.CountryRegion))
            {
                _countryReports.Remove(country);
                //_reports.RemoveAll(_ => _.Name == country);
            }

            foreach (var day in newRows.Select(_ => _.LastUpdate.Date))
            {
                _dayReports.Remove(day);
                //_reports.RemoveAll(_ => _.Day == day);
            }

            GenerateReports();
        }

        public CountryReport GetCountryReport(string countryName)
        {
            if (_countryReports.ContainsKey(countryName))
                return _countryReports[countryName];

            var countyrReport = new CountryReport(countryName, _linkedReports[countryName]);
            _countryReports.Add(countryName, countyrReport);

            return _countryReports[countryName];
        }

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

        private void GenerateReports()
        {
            var uniqueEntries = _data.Distinct().ToArray();

            var countriesList = uniqueEntries.Select(x => x.CountryRegion).Distinct().ToArray();
            var dates = uniqueEntries.Select(x => x.LastUpdate.Date).Distinct().OrderBy(_ => _).ToArray();

            Parallel.ForEach(countriesList, country =>
            {
                if (country == "Others")
                    return;

                _logger.WriteInfo($"--Processing {country}...");

                var countryEntries = uniqueEntries.Where(x => x.CountryRegion == country).ToArray();
                var graphBuilder = new ReportsGraphBuilder(country, _logger);
                var graphStructure = new ReportsGraphStructure(country);

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
                                    graphStructure.AddProvince(dayCountryEntry.ProvinceState);
                                    break;

                                case IsoLevel.County:
                                    graphStructure.AddCounty(dayCountryEntry.County, dayCountryEntry.ProvinceState);
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
                                graphStructure.AddProvince(countryEntry.ProvinceState);
                                break;

                            case IsoLevel.County:
                                graphStructure.AddCounty(countryEntry.County, countryEntry.ProvinceState);
                                break;
                        }

                        graphBuilder.Reports.Add(CreateBasicReport(countryEntry));
                    }
                }

                _linkedReports.TryAdd(country, graphBuilder.Build(graphStructure));
            });

            _data.Clear();
        }
    }
}