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
        private readonly ConcurrentBag<IntermidiateReport> _reports = new ConcurrentBag<IntermidiateReport>();
        private readonly Dictionary<string, CountryReport> _countryReports = new Dictionary<string, CountryReport>();
        private readonly Dictionary<DateTime, DayReport> _dayReports = new Dictionary<DateTime, DayReport>();

        public IEnumerable<string> AvailableCountries => _reports.Select(_ => _.Name).Distinct();
        public IEnumerable<DateTime> AvailableDates => _reports.Select(_ => _.Day).Distinct();

        private void GenerateReports()
        {
            var countriesList = _data.Select(_ => _.CountryRegion).Distinct().ToArray();
            var dates = _data.Select(_ => _.LastUpdate.Date).Distinct().OrderBy(_ => _).ToArray();

            Parallel.ForEach(countriesList, (country) =>
                //foreach (var country in countriesList)
            {
                if (_reports.Any(_ => _.Name == country))
                    return;

                foreach (var day in dates)
                {
                    if (_reports.Any(_ => _.Day == day && _.Name == country))
                        continue;

                    var dayCountryReports = _data.Where(_ => _.LastUpdate.Date == day && _.CountryRegion == country)
                        .ToArray();
                    IntermidiateReport report;

                    switch (dayCountryReports.Length)
                    {
                        case 0:
                            continue;
                        case 1 when string.IsNullOrEmpty(dayCountryReports[0].ProvinceState):
                        {
                            var previousMetrics = _reports
                                .Where(_ =>_.Name == country)
                                .OrderBy(_ => _.Day).LastOrDefault()?
                                .Total ?? Metrics.Empty;

                            var currentMetrics = Metrics.FromEntry(dayCountryReports[0]);
                            report = new IntermidiateReport(country, day, currentMetrics,
                                currentMetrics - previousMetrics);
                            break;
                        }
                        default:
                        {
                            var dayProvinceReports = _reports
                                .OfType<CountryWithRegionsIntermidiateReport>()
                                .Where(_ => _.Name == country)
                                .SelectMany(_ => _.RegionReports)
                                .ToArray();

                            List<IntermidiateReport> provinceReports = new List<IntermidiateReport>();
                            foreach (var dayProvinceReport in dayCountryReports)
                            {
                                var previousMetrics =
                                    dayProvinceReports.Where(_ => _.Name == dayProvinceReport.ProvinceState)
                                        .OrderBy(_ => _.Day)
                                        .LastOrDefault()
                                        ?.Total ?? Metrics.Empty;

                                var currentMetrics = Metrics.FromEntry(dayProvinceReport);
                                provinceReports.Add(new IntermidiateReport(dayProvinceReport.ProvinceState, day,
                                    currentMetrics, currentMetrics - previousMetrics));
                            }

                            var additionalProvinceReports = dayProvinceReports.Where(_ =>
                                    dayCountryReports.Select(__ => __.ProvinceState).All(__ => __ != _.Name))
                                .GroupBy(_ => _.Name);

                            provinceReports.AddRange(
                                additionalProvinceReports
                                    .Select(provinceReport =>
                                        new IntermidiateReport(
                                            provinceReport.Key,
                                            day,
                                            provinceReport.OrderBy(_ => _.Day).Last().Total,
                                            Metrics.Empty)));

                            report = new CountryWithRegionsIntermidiateReport(country, day, provinceReports);
                            break;
                        }
                    }

                    _reports.Add(report);
                }
            });
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