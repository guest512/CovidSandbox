using System;
using System.Collections.Generic;
using System.Linq;

namespace CovidSandbox.Model.Reports
{
    public class CountryReport
    {
        public CountryReport(string name, IEnumerable<Entry> entries)
        {
            Name = name;
            RegionReports = entries.GroupBy(_ => _.ProvinceState).Select(_ => new RegionReport(_.Key, _)).ToArray();
        }

        public string Name { get; }

        public IEnumerable<RegionReport> RegionReports { get; }

        public static CountryReport FromData(string name, IEnumerable<Entry> allEntries)
        {
            return new CountryReport(name, allEntries.GroupBy(_ => _.CountryRegion).First(_ => _.Key == name));
        }

        public IEnumerable<DateTime> GetAvailableDates() =>
            RegionReports.SelectMany(_ => _.GetAvailableDates()).Distinct().OrderBy(_ => _);

        public Metrics GetDiffByDay(DateTime day) =>
            RegionReports.Select(_ => _.GetDiffByDay(day)).Aggregate(Metrics.Empty, (sum, elem) => sum + elem);

        public Metrics GetTotalByDay(DateTime day) =>
                            RegionReports.Select(_ => _.GetTotalByDay(day)).Aggregate(Metrics.Empty, (sum, elem) => sum + elem);
    }
}