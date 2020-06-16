using CovidSandbox.Model.Reports.Intermediate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CovidSandbox.Model.Reports
{
    public class RegionReport
    {
        private readonly IEnumerable<IntermediateReport> _reports;
        private readonly Dictionary<DateTime, Metrics> _dayByDayMetrics = new Dictionary<DateTime, Metrics>();

        public RegionReport(string name, IEnumerable<IntermediateReport> reports)
        {
            Name = name;
            _reports = reports.ToArray();
            AvailableDates = Utils.GetContinuousDateRange(_reports.Select(_ => _.Day)).ToArray();
        }

        public IEnumerable<DateTime> AvailableDates { get; }

        public string Name { get; }

        public Metrics GetDayChange(DateTime day)
        {
            var currentEntry = GetDayTotal(day);
            var prevEntry = GetDayTotal(day.AddDays(-1).Date);

            return currentEntry - prevEntry;
        }

        public Metrics GetDayTotal(DateTime day)
        {
            day = day.Date;
            Metrics dayMetrics;
            if (_dayByDayMetrics.ContainsKey(day))
            {
                dayMetrics = _dayByDayMetrics[day];
            }
            else
            {
                var testDay = day;
                var dayReports = Enumerable.Empty<IntermediateReport>().ToArray();

                while (!dayReports.Any() && testDay > Utils.PandemicStart && !_dayByDayMetrics.ContainsKey(testDay))
                {
                    dayReports = _reports.Where(_ => _.Day == testDay).ToArray();
                    testDay = testDay.AddDays(-1);
                }

                dayMetrics = !dayReports.Any() && _dayByDayMetrics.ContainsKey(testDay)
                    ? _dayByDayMetrics[testDay]
                    : dayReports.Select(_ => _.Total).Aggregate(Metrics.Empty, (sum, elem) => sum + elem);

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