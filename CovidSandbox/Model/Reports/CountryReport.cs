using CovidSandbox.Model.Reports.Intermediate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CovidSandbox.Model.Reports
{
    public class CountryReport : BaseReport
    {
        private readonly RegionReport? _wholeCountryReport;

        public CountryReport(string name, LinkedReport head) : base(head, name)
        {
            RegionReports = head.Children
                .Where(child => child.Name != Consts.MainCountryRegion)
                .Select(child => new RegionReport(child.Name, child))
                .ToArray();

            if (head.Children.Any(child => child.Name == Consts.MainCountryRegion))
                _wholeCountryReport = new RegionReport(string.Empty, head.Children.First(child => child.Name == Consts.MainCountryRegion));

            AvailableDates = Utils.GetContinuousDateRange(Head.GetAvailableDates())
                .ToArray();
        }

        public IEnumerable<DateTime> AvailableDates { get; }
        public IEnumerable<RegionReport> RegionReports { get; }
    }
}