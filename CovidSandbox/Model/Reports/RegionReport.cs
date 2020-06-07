using System;
using System.Collections.Generic;
using System.Linq;

namespace CovidSandbox.Model.Reports
{
    public class RegionReport
    {
        private static readonly DateTime PandemicStart = new DateTime(2020,1,1);

        private readonly IEnumerable<Entry> _entries;
        public RegionReport(string name, IEnumerable<Entry> entries)
        {
            Name = name;
            _entries = entries.ToArray();
        }

        public string Name {get; }

        public Metrics GetTotalByDay(DateTime day){
            Entry dayEntry = null;
            var i =0;


            while(dayEntry == null && day.AddDays(i).Date > PandemicStart)
            {
                dayEntry = _entries.FirstOrDefault(_ => _.LastUpdate == day.AddDays(i).Date);
                i--;
            }

            return Metrics.FromEntry(dayEntry);
        }

        public Metrics GetDiffByDay(DateTime day)
        {
            var currentEntry = GetTotalByDay(day);
            var prevEntry = GetTotalByDay(day.AddDays(-1).Date);

            return currentEntry - prevEntry;
        }

        public IEnumerable<DateTime> GetAvailableDates() =>
            _entries.Select(_ => _.LastUpdate).Distinct();

    }
}
