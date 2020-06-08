using CovidSandbox.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CovidSandbox.Model.Reports
{
    public class RegionReport
    {
        private readonly IEnumerable<Entry> _entries;
        private readonly Dictionary<DateTime, Metrics> _dayByDayMetrics = new Dictionary<DateTime, Metrics>();

        public RegionReport(string name, IEnumerable<Entry> entries)
        {
            Name = name;
            _entries = entries.ToArray();
            AvailableDates = _entries.Where(_ => !string.IsNullOrEmpty(_.ProvinceState)).Select(_ => _.LastUpdate).Distinct().OrderBy(_=>_).ToArray();
        }

        public IEnumerable<DateTime> AvailableDates { get; }

        public string Name { get; }

        public Metrics GetDiffByDay(DateTime day)
        {
            var currentEntry = GetTotalByDay(day);
            var prevEntry = GetTotalByDay(day.AddDays(-1).Date);

            return currentEntry - prevEntry;
        }

        public Metrics GetTotalByDay(DateTime day)
        {
            var dayEntries = Enumerable.Empty<Entry>().ToArray();
            var i = 0;

            while (!dayEntries.Any() && day.AddDays(i).Date > Utils.PandemicStart)
            {
                var testDay = i;
                dayEntries = _entries.Where(_ => _.LastUpdate == day.AddDays(testDay).Date).ToArray();
                i--;
            }

            return dayEntries.Select(Metrics.FromEntry).Aggregate(Metrics.Empty, (sum, elem) => sum + elem);
        }
    }
}