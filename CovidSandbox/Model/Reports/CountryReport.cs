using CovidSandbox.Model.Reports.Intermediate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CovidSandbox.Model.Reports
{
    public class CountryReport
    {
        private readonly LinkedReport _head;
        private readonly RegionReport? _wholeCountryReport;
        private readonly Dictionary<DateTime, Metrics> _dayByDayMetrics = new Dictionary<DateTime, Metrics>();

        public CountryReport(string name, LinkedReport head)
        {
            Name = name;
            _head = head;

            RegionReports = head.Children
                .Where(child => child.Name != Consts.MainCountryRegion)
                .Select(child => new RegionReport(child.Name, child))
                .ToArray();

            if (head.Children.Any(child => child.Name == Consts.MainCountryRegion))
                _wholeCountryReport = new RegionReport(string.Empty, head.Children.First(child => child.Name == Consts.MainCountryRegion));

            AvailableDates = Utils.GetContinuousDateRange(head.GetAvailableDates())
                .ToArray();
        }

        public string Name { get; }

        public IEnumerable<RegionReport> RegionReports { get; }

        public IEnumerable<DateTime> AvailableDates { get; }

        public Metrics GetDayChange(DateTime day)
        {
            var position = _head;

            while (position.Next.Day <= day && position.Next != LinkedReport.Empty)
            {
                position = position.Next;
            }

            return position.Change;
        }

        public Metrics GetDayTotal(DateTime day)
        {
            var position = _head;

            while (position.Next.Day <= day && position.Next != LinkedReport.Empty)
            {
                position = position.Next;
            }

            return position.Total;
        }
    }
}