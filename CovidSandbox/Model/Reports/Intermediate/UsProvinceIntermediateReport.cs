using System;
using System.Collections.Generic;
using System.Linq;

namespace CovidSandbox.Model.Reports.Intermediate
{
    public class UsProvinceIntermediateReport : ProvinceIntermediateReport
    {
        public IEnumerable<UsCountyIntermidiateReport> CountyReports { get; }

        public override Metrics Total { get; }

        public override Metrics Change { get; }

        public UsProvinceIntermediateReport(string provinceName, string countryName, DateTime day,
            IEnumerable<UsCountyIntermidiateReport> countyReports) :
            base(provinceName, countryName, day)
        {
            CountyReports = countyReports.ToArray();
            Total = CountyReports.Aggregate(Metrics.Empty, (sum, elem) => sum + elem.Total);
            Change = CountyReports.Aggregate(Metrics.Empty, (sum, elem) => sum + elem.Change);
        }

        public UsProvinceIntermediateReport(string provinceName, string countryName, in DateTime day, List<UsCountyIntermidiateReport> countyReports, in Metrics previousMetrics) : this(provinceName, countryName, day, countyReports)
        {
            Change = Total - previousMetrics;
        }

        public UsProvinceIntermediateReport(string provinceName, string countryName, in DateTime day, in Metrics total,
            in Metrics change) : base(provinceName, countryName, day, total, change)
        {
            CountyReports = Enumerable.Empty<UsCountyIntermidiateReport>();
        }
    }
}