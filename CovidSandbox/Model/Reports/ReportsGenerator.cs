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
            var countriesList = uniqueEntries.Select(_ => _.CountryRegion).Distinct().ToArray();
            var dates = uniqueEntries.Select(_ => _.LastUpdate.Date).Distinct().OrderBy(_ => _).ToArray();
            ConcurrentDictionary<string, IntermediateReport> lastReport =
                new ConcurrentDictionary<string, IntermediateReport>();

            Parallel.ForEach(countriesList, (country) =>
            //foreach (var country in countriesList)
            {
                if (_reports.Any(_ => _.Name == country))
                    return;

                foreach (var day in dates)
                {
                    if (_reports.Any(_ => _.Day == day && _.Name == country))
                        continue;

                    var dayCountryReports = uniqueEntries
                        .Where(_ => _.LastUpdate.Date == day && _.CountryRegion == country)
                        .ToArray();
                    IntermediateReport report;

                    switch (dayCountryReports.Length)
                    {
                        case 0:
                            continue;
                        case 1 when string.IsNullOrEmpty(dayCountryReports[0].ProvinceState) ||
                                    dayCountryReports[0].ProvinceState == dayCountryReports[0].CountryRegion:
                            {
                                report = CreateCountryReport(lastReport, country, dayCountryReports[0], day);
                                break;
                            }
                        default:
                            {
                                report = dayCountryReports.All(_ => string.IsNullOrEmpty(_.ProvinceState)) ||
                                         dayCountryReports.All(_ => _.ProvinceState == country)
                                    ? CreateCountryReport(lastReport, country, dayCountryReports[0], day)
                                    : CreateCountryWithRegionsReport(lastReport, country, dayCountryReports.Where(_ => !(string.IsNullOrEmpty(_.ProvinceState) || _.ProvinceState == country)), day);

                                break;
                            }
                    }

                    if (!lastReport.TryAdd(country, report))
                        Console.WriteLine($"!!!POSSIBLE DUPLICATE OR WRONG DATA!!! {report}");
                    _reports.Add(report);
                }
            });
        }

        private static IntermediateReport CreateCountryWithRegionsReport(ConcurrentDictionary<string, IntermediateReport> lastReport, string country,
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
                    lastReport.TryRemove(dayProvinceReports.Key, out var previousReport)
                        ? previousReport.Total
                        : Metrics.Empty;

                var currentMetrics = dayProvinceReports.Select(Metrics.FromEntry)
                    .Aggregate((sum, elem) => sum + elem);
                provinceReports.Add(new ProvinceIntermediateReport(dayProvinceReports.Key,
                    country, day,
                    currentMetrics, currentMetrics - previousMetrics));
            }

            foreach (var provinceReport in provinceReports.Where(provinceReport =>
                !lastReport.TryAdd(provinceReport.Name, provinceReport)))
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

            IntermediateReport report = lastReport.TryRemove(country, out var previousCountryReport) &&
                                        !(previousCountryReport is CountryWithRegionsIntermediateReport)
                ? new CountryWithRegionsIntermediateReport(country, day, provinceReports,
                    previousCountryReport.Total)
                : new CountryWithRegionsIntermediateReport(country, day, provinceReports);
            return report;
        }

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