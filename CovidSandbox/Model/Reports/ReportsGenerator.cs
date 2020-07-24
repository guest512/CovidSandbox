using CovidSandbox.Model.Reports.Intermediate;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CovidSandbox.Model.Reports
{
    public class ReportsGenerator
    {
        private readonly List<Entry> _data = new List<Entry>();
        private readonly ConcurrentBag<IntermediateReport> _reports = new ConcurrentBag<IntermediateReport>();
        private readonly Dictionary<string, CountryReport> _countryReports = new Dictionary<string, CountryReport>();
        private readonly Dictionary<DateTime, DayReport> _dayReports = new Dictionary<DateTime, DayReport>();

        public IEnumerable<string> AvailableCountries => _reports.Select(_ => _.Name).Distinct();
        public IEnumerable<DateTime> AvailableDates => _reports.Select(_ => _.Day).Distinct();

        private void GenerateReports()
        {
            var uniqueEntries = _data.Distinct().ToArray();
            var countriesList = uniqueEntries.Select(x => x.CountryRegion).Distinct().ToArray();
            var dates = uniqueEntries.Select(x => x.LastUpdate.Date).Distinct().OrderBy(_ => _).ToArray();
            var lastReport = new ConcurrentDictionary<string, IntermediateReport>();

            Parallel.ForEach(countriesList, (country) =>
            //foreach (var country in countriesList)
            {
                if (_reports.Any(_ => _.Name == country))
                    return;

                var countryReports = uniqueEntries.Where(x => x.CountryRegion == country).ToArray();

                foreach (var day in dates)
                {
                    if (_reports.Any(x => x.Day == day && x.Name == country))
                        continue;

                    var dayCountryReports = countryReports
                        .Where(x => x.LastUpdate.Date == day)
                        .ToArray();

                    if (country == "Russia" &&
                        dayCountryReports.Any(x => x.Origin == Origin.JHopkins) &&
                        dayCountryReports.Any(x => x.Origin == Origin.Yandex))
                    {
                        dayCountryReports = dayCountryReports.Where(x => x.Origin == Origin.Yandex).ToArray();
                    }

                    IntermediateReport report;

                    switch (dayCountryReports.Length)
                    {
                        case 0:
                            continue;
                        case 1 when string.IsNullOrEmpty(dayCountryReports[0].ProvinceState) ||
                                    dayCountryReports[0].ProvinceState == dayCountryReports[0].CountryRegion ||
                                    dayCountryReports[0].ProvinceState == "Main territory":
                            {
                                report = CreateCountryReport(lastReport, country, dayCountryReports[0], day);
                                break;
                            }
                        default:
                            {
                                report = dayCountryReports.All(_ => string.IsNullOrEmpty(_.ProvinceState)) ||
                                         dayCountryReports.All(_ => _.ProvinceState == country)
                                    ? CreateCountryReport(lastReport, country, dayCountryReports[0], day)
                                    : (country == "US"
                                        ? CreateUsCountryWithRegionsReport(lastReport, country,
                                            dayCountryReports.Where(_ =>
                                                !(string.IsNullOrEmpty(_.ProvinceState) || _.ProvinceState == country)),
                                            day)
                                        : CreateCountryWithRegionsReport(lastReport, country,
                                            dayCountryReports.Where(_ =>
                                                !(string.IsNullOrEmpty(_.ProvinceState) || _.ProvinceState == country)),
                                            day));

                                break;
                            }
                    }

                    if (!lastReport.TryAdd(country, report))
                        Console.WriteLine($"!!!POSSIBLE DUPLICATE OR WRONG DATA!!! {report}");
                    _reports.Add(report);
                }
            });

            _data.Clear();
        }

        private static CountryWithRegionsIntermediateReport CreateUsCountryWithRegionsReport(
            ConcurrentDictionary<string, IntermediateReport> lastReport, string country, IEnumerable<Entry> dayCountryReports,
            DateTime day)
        {
            dayCountryReports = dayCountryReports as Entry[] ?? dayCountryReports.ToArray();
            if (dayCountryReports.All(_ => string.IsNullOrEmpty(_.County)) ||
                dayCountryReports.All(_ => _.FIPS == 0))
            {
                return CreateCountryWithRegionsReport(lastReport, country, dayCountryReports, day);
            }

            var lastProvinceReports = lastReport.Values
                .OfType<UsProvinceIntermediateReport>()
                .Where(_ => _.Country == country)
                .ToArray();

            var todayProvinces = new List<string>();
            var provinceReports =
                new List<UsProvinceIntermediateReport>();
            foreach (var dayProvinceReports in dayCountryReports.GroupBy(_ => _.ProvinceState))
            {
                string CountyKey(uint fips, string name, string provinceName) => $"{provinceName}-{name}({fips})";
                var lastCountyReports = lastReport.Values
                    .OfType<UsCountyIntermidiateReport>()
                    .Where(_ => _.Name == dayProvinceReports.Key)
                    .ToArray();
                todayProvinces.Add(dayProvinceReports.Key);

                var todayCounties = new List<uint>();
                var countyReports = new List<UsCountyIntermidiateReport>();
                foreach (var dayCountyReport in dayProvinceReports)
                {
                    todayCounties.Add(dayCountyReport.FIPS);
                    var previousCountyMetrics = lastReport.TryRemove(
                        CountyKey(dayCountyReport.FIPS, dayCountyReport.County, dayCountyReport.ProvinceState), out var previousCountyReport)
                        ? previousCountyReport.Total
                        : Metrics.Empty;
                    var currentCountyMetrics = Metrics.FromEntry(dayCountyReport);
                    countyReports.Add(new UsCountyIntermidiateReport(dayCountyReport.FIPS, dayCountyReport.County,
                        dayProvinceReports.Key, country, day, currentCountyMetrics,
                        currentCountyMetrics - previousCountyMetrics));
                }

                foreach (var countyReport in countyReports.Where(_ => !lastReport.TryAdd(CountyKey(_.FIPS, _.Name, _.Province), _)))
                {
                    Console.WriteLine($"!!!POSSIBLE DUPLICATE OR WRONG DATA!!! {countyReport}");
                }

                var additionalCountyReports = lastCountyReports.Where(_ =>
                    todayCounties.All(__ => __ != _.FIPS));

                countyReports.AddRange(additionalCountyReports.Select(_ =>
                    new UsCountyIntermidiateReport(_.FIPS, _.Name, dayProvinceReports.Key, country, day, _.Total,
                        Metrics.Empty)));

                var provinceReport = lastReport.TryRemove(ProvinceKey(dayProvinceReports.Key, country), out var previousProvinceReport) &&
                                     !(previousProvinceReport is UsProvinceIntermediateReport)
                    ? new UsProvinceIntermediateReport(dayProvinceReports.Key, country, day, countyReports,
                        previousProvinceReport.Total)
                    : new UsProvinceIntermediateReport(dayProvinceReports.Key, country, day, countyReports);

                provinceReports.Add(provinceReport);
            }

            foreach (var provinceReport in provinceReports.Where(provinceReport =>
                !lastReport.TryAdd(ProvinceKey(provinceReport.Name, country), provinceReport)))
            {
                Console.WriteLine($"!!!POSSIBLE DUPLICATE OR WRONG DATA!!! {provinceReport}");
            }

            var additionalProvinceReports = lastProvinceReports.Where(_ =>
                todayProvinces.All(__ => __ != _.Name));

            provinceReports.AddRange(
                additionalProvinceReports
                    .Select(_ =>
                        new UsProvinceIntermediateReport(
                            _.Name,
                            country,
                            day,
                            _.Total,
                            Metrics.Empty)));

            var report = lastReport.TryRemove(country, out var previousCountryReport) &&
                     !(previousCountryReport is CountryWithRegionsIntermediateReport)
                ? new CountryWithRegionsIntermediateReport(country, day, provinceReports,
                    previousCountryReport.Total)
                : new CountryWithRegionsIntermediateReport(country, day, provinceReports);

            return report;
        }

        private static CountryWithRegionsIntermediateReport CreateCountryWithRegionsReport(ConcurrentDictionary<string, IntermediateReport> lastReport, string country,
            IEnumerable<Entry> dayCountryReports, DateTime day)
        {
            var lastProvinceReports = lastReport.Values
                .OfType<ProvinceIntermediateReport>()
                .Where(_ => _.Country == country)
                .ToArray();

            var todayProvinces = new List<string>();
            var provinceReports =
                new List<ProvinceIntermediateReport>();
            foreach (var dayProvinceReports in dayCountryReports.GroupBy(_ => _.ProvinceState))
            {
                todayProvinces.Add(dayProvinceReports.Key);
                var previousMetrics =
                    lastReport.TryRemove(ProvinceKey(dayProvinceReports.Key, country), out var previousReport)
                        ? previousReport.Total
                        : Metrics.Empty;

                var currentMetrics = dayProvinceReports.Select(Metrics.FromEntry)
                    .Aggregate((sum, elem) => sum + elem);
                provinceReports.Add(new ProvinceIntermediateReport(dayProvinceReports.Key,
                    country, day,
                    currentMetrics, currentMetrics - previousMetrics));
            }

            foreach (var provinceReport in provinceReports.Where(provinceReport =>
                !lastReport.TryAdd(ProvinceKey(provinceReport.Name, provinceReport.Country), provinceReport)))
            {
                Console.WriteLine($"!!!POSSIBLE DUPLICATE OR WRONG DATA!!! {provinceReport}");
            }

            var additionalProvinceReports = lastProvinceReports.Where(_ =>
                todayProvinces.All(__ => __ != _.Name));

            provinceReports.AddRange(
                additionalProvinceReports
                    .Select(_ =>
                        new ProvinceIntermediateReport(
                            _.Name,
                            country,
                            day,
                            _.Total,
                            Metrics.Empty)));

            var report = lastReport.TryRemove(country, out var previousCountryReport) &&
                                        !(previousCountryReport is CountryWithRegionsIntermediateReport)
                ? new CountryWithRegionsIntermediateReport(country, day, provinceReports,
                    previousCountryReport.Total)
                : new CountryWithRegionsIntermediateReport(country, day, provinceReports);
            return report;
        }

        private static string ProvinceKey(string name, string countryName) => $"{countryName}-{name}";

        private static IntermediateReport CreateCountryReport(ConcurrentDictionary<string, IntermediateReport> lastReport, string country,
            Entry dayCountryReport, DateTime day)
        {
            var previousMetrics = lastReport.TryRemove(country, out var previousReport)
                ? previousReport.Total
                : Metrics.Empty;

            if (previousReport is ProvinceIntermediateReport provinceReport)
            {
                Console.WriteLine($"!!!COUNTRY IS ALSO A REGION!!! {country} part of {provinceReport.Country}");
            }

            var currentMetrics = Metrics.FromEntry(dayCountryReport);
            var report = new IntermediateReport(country, day, currentMetrics,
                currentMetrics - previousMetrics);
            return report;
        }

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

            var dayReport = new CountryReport(countryName, _reports.Where(_ => _.Name == countryName));
            _countryReports.Add(countryName, dayReport);

            return _countryReports[countryName];
        }

        public DayReport GetDayReport(DateTime day)
        {
            day = day.Date;
            if (_dayReports.ContainsKey(day))
                return _dayReports[day];

            var dayReport = new DayReport(day, _reports.Where(_ => _.Day == day));
            _dayReports.Add(day, dayReport);

            return _dayReports[day];
        }
    }
}