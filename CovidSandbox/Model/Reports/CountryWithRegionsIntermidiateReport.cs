using System;
using System.Collections.Generic;
using System.Linq;

namespace CovidSandbox.Model.Reports
{
    public sealed class CountryWithRegionsIntermidiateReport : IntermidiateReport
    {
        public IEnumerable<IntermidiateReport> RegionReports { get; }

        public override Metrics Total => RegionReports.Aggregate(Metrics.Empty, (sum, elem) => sum + elem.Total);

        public override Metrics Change => RegionReports.Aggregate(Metrics.Empty, (sum, elem) => sum + elem.Change);

        public CountryWithRegionsIntermidiateReport(string coutnryProvinceName, DateTime day, IEnumerable<IntermidiateReport> regionReports) : base(coutnryProvinceName, day)
        {
            RegionReports = regionReports.ToArray();
        }
    }
}