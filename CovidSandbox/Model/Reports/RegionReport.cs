using System;
using System.Collections.Generic;
using System.Linq;

namespace CovidSandbox.Model.Reports
{
    public class RegionReport
    {
        private static readonly DateTime PandemicStart = new DateTime(2020, 1, 1);

        private readonly IEnumerable<Entry> _entries;

        public RegionReport(string name, IEnumerable<Entry> entries)
        {
            Name = name;
            _entries = entries.ToArray();
        }

        public string Name { get; }

        public IEnumerable<DateTime> GetAvailableDates() =>
            _entries.Where(_ => !string.IsNullOrEmpty(_.ProvinceState)).Select(_ => _.LastUpdate).Distinct();

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

            while (!dayEntries.Any() && day.AddDays(i).Date > PandemicStart)
            {
                var testDay = i;
                dayEntries = _entries.Where(_ => _.LastUpdate == day.AddDays(testDay).Date).ToArray();
                i--;
            }

            return dayEntries.Select(Metrics.FromEntry).Aggregate(Metrics.Empty, (sum, elem) => sum + elem);
        }
    }
}