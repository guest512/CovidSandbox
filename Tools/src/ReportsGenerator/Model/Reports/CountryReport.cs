using System;
using System.Collections.Generic;
using System.Linq;
using ReportsGenerator.Model.Reports.Intermediate;

namespace ReportsGenerator.Model.Reports
{
    public class CountryReport : BaseCountryReport
    {
        public CountryReport(string name, LinkedReport head) : base(head, name)
        {
            RegionReports =
                (head.Children.Any(child => child.Name != Consts.MainCountryRegion)
                    ? head.Children.Where(child => child.Name != Consts.MainCountryRegion)
                    : head.Children)
                .Select(child => new RegionReport(child.Name, child))
                .ToArray();
        }

        public IEnumerable<RegionReport> RegionReports { get; }
    }
}