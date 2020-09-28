using CovidSandbox.Model.Reports.Intermediate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CovidSandbox.Model.Reports
{
    public abstract class BaseReport
    {
        protected readonly LinkedReport Head;

        protected BaseReport(LinkedReport head, string name)
        {
            Head = head;
            Name = name;
        }

        public string Name { get; }

        public Metrics GetDayChange(DateTime day)
        {
            var position = GetDayReport(day);

            return position.Total - position.Previous.Total;
        }

        public Metrics GetDayTotal(DateTime day)
        {
            var position = GetDayReport(day);

            return position.Total;
        }

        public double GetRt(DateTime day)
        {
            if (Head.Day.AddDays(7) > day)
                return 0;

            var position = GetDayReport(day);

            static long GetCumulativeConfirmedChange(IEnumerable<LinkedReport> reports) =>
                reports.Aggregate(0L, (sum, rep) => sum + (rep.Total - rep.Previous.Total).Confirmed);

            var lastDays = GetCumulativeConfirmedChange(GetReports(position, 4, false));
            var previousDays = GetCumulativeConfirmedChange(GetReports(GetDayReport(day.AddDays(-4)), 4, false));

            return previousDays > 0 ? lastDays / (double)previousDays  : 0;
        }

        private static IEnumerable<LinkedReport> GetReports(LinkedReport start, int count, bool lookForward = true)
        {
            var position = start;
            for (var i = 0; i < count; i++)
            {
                yield return position;
                position = lookForward ? position.Next : position.Previous;
            }
        }

        private LinkedReport GetDayReport(DateTime day)
        {
            var position = Head;

            while (position.Next.Day <= day && position.Next != LinkedReport.Empty)
            {
                position = position.Next;
            }

            return position;
        }
    }
}