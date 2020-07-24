using System;
using System.Collections.Generic;
using System.Linq;

namespace CovidSandbox.Model.Reports.Intermediate
{
    public sealed class CountryWithRegionsIntermediateReport : IntermediateReport
    {
        public IEnumerable<ProvinceIntermediateReport> RegionReports { get; }
        
        public CountryWithRegionsIntermediateReport(string countryName, DateTime day, IEnumerable<ProvinceIntermediateReport> regionReports) : base(countryName, day)
        {
            RegionReports = regionReports.ToArray();
            Total = RegionReports.Aggregate(Metrics.Empty, (sum, elem) => sum + elem.Total);
            Change = RegionReports.Aggregate(Metrics.Empty, (sum, elem) => sum + elem.Change);
        }

        public CountryWithRegionsIntermediateReport(string countryName, DateTime day, IEnumerable<ProvinceIntermediateReport> regionReports, Metrics previousMetrics) : this(countryName, day, regionReports)
        {
            Change = Total - previousMetrics;
        }
    }
}