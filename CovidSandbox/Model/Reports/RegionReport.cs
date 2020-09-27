using CovidSandbox.Model.Reports.Intermediate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CovidSandbox.Model.Reports
{
    public class RegionReport
    {
        private readonly LinkedReport _head;

        public RegionReport(string name, LinkedReport head)
        {
            Name = name;
            _head = head;
            AvailableDates = Utils.GetContinuousDateRange(_head.GetAvailableDates()).ToArray();
        }

        public IEnumerable<DateTime> AvailableDates { get; }

        public string Name { get; }

        public Metrics GetDayChange(DateTime day)
        {
            var currentEntry = GetDayTotal(day);
            var prevEntry = GetDayTotal(day.AddDays(-1).Date);

            return currentEntry - prevEntry;
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