using CovidSandbox.Model.Reports.Intermediate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CovidSandbox.Model.Reports
{
    public class CountryReport
    {
        private readonly RegionReport? _wholeCountryReport;
        private readonly Dictionary<DateTime, Metrics> _dayByDayMetrics = new Dictionary<DateTime, Metrics>();

        public CountryReport(string name, IEnumerable<IntermediateReport> reports)
        {
            Name = name;
            reports = reports as IntermediateReport[] ?? reports.ToArray();
            RegionReports = reports
                .OfType<CountryWithRegionsIntermediateReport>()
                .SelectMany(_ => _.RegionReports)
                .GroupBy(_ => _.Name)
                .Select(_ => new RegionReport(_.Key, _))
                .ToArray();

            if (reports.Any(_ => !(_ is CountryWithRegionsIntermediateReport)))
                _wholeCountryReport = new RegionReport(string.Empty, reports.Where(_ => !(_ is CountryWithRegionsIntermediateReport)));

            AvailableDates = Utils.GetContinuousDateRange((_wholeCountryReport?
                    .AvailableDates ?? Enumerable.Empty<DateTime>())
                    .Union(RegionReports.SelectMany(_ => _.AvailableDates)))
                .ToArray();
        }

        public string Name { get; }

        public IEnumerable<RegionReport> RegionReports { get; }

        public IEnumerable<DateTime> AvailableDates { get; }

        public Metrics GetDayChange(DateTime day)
        {
            var currentEntry = GetDayTotal(day);
            var prevEntry = GetDayTotal(day.AddDays(-1).Date);

            return currentEntry - prevEntry;
        }

        public Metrics GetDayTotal(DateTime day)
        {
            day = day.Date;
            var dayMetrics = Metrics.Empty;
            if (_dayByDayMetrics.ContainsKey(day))
            {
                dayMetrics = _dayByDayMetrics[day];
            }
            else
            {
                var testDay = day.AddDays(0);

                while (dayMetrics == Metrics.Empty && testDay.Date > Utils.PandemicStart)
                {
                    var dayMetricsCountry = Metrics.Empty;
                    if (_wholeCountryReport != null)
                    {
                        dayMetricsCountry = _wholeCountryReport.GetDayTotal(testDay);
                    }

                    var dayMetricsRegion = RegionReports
                        .Select(_ => _.GetDayTotal(testDay))
                        .Aggregate(Metrics.Empty, (sum, elem) => sum + elem);

                    dayMetrics = dayMetricsCountry.Confirmed > dayMetricsRegion.Confirmed ? dayMetricsCountry : dayMetricsRegion;

                    if (_dayByDayMetrics.ContainsKey(testDay))
                    {
                        dayMetrics = _dayByDayMetrics[testDay];
                        break;
                    }

                    testDay = testDay.AddDays(-1);
                }

                testDay = testDay.AddDays(1);
                while (testDay <= day)
                {
                    _dayByDayMetrics.Add(testDay, dayMetrics);
                    testDay = testDay.AddDays(1);
                }
            }

            return dayMetrics;
        }
    }
}