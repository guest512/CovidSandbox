using System;
using System.Collections.Generic;
using System.Linq;
using ReportsGenerator.Model.Reports.Intermediate;

namespace ReportsGenerator.Model.Reports
{
    public abstract class BaseCountryReport
    {
        protected readonly LinkedReport Head;

        private Dictionary<DateTime, double>? _timeToResolve;

        protected BaseCountryReport(LinkedReport head, string name)
        {
            Head = head;
            Name = name;
            AvailableDates = Head.GetAvailableDates().GetContinuousDateRange()
                .ToArray();
        }

        public string Name { get; }

        public IEnumerable<DateTime> AvailableDates { get; }

        private Dictionary<DateTime, double> TimeToResolve => _timeToResolve ??= new Dictionary<DateTime, double>(GetTimeToResolveCollection());

        public Metrics GetDayChange(DateTime day)
        {
            var position = GetDayReport(day);

            return GetDayChange(position);
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

            return previousDays > 0 ? lastDays / (double)previousDays : 0;
        }

        public double GetTimeToResolve(DateTime day)
        {
            return TimeToResolve[day];
        }

        private static Metrics GetDayChange(LinkedReport report) => report.Total - report.Previous.Total;

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

        private IEnumerable<KeyValuePair<DateTime, double>> GetTimeToResolveCollection()
        {
            static Metrics CalcResolution(long confirmed, Metrics resolved)
            {
                var (_, _, recovered, deaths) = resolved;

                if (deaths > confirmed)
                {
                    deaths -= confirmed;
                    return new Metrics(0, 0, recovered, deaths);
                }

                confirmed -= deaths;

                if (recovered > confirmed)
                {
                    recovered -= confirmed;
                    return new Metrics(0, 0, recovered, 0);
                }

                confirmed -= recovered;

                return new Metrics(confirmed, 0, 0, 0);
            }

            var positionConfirmed = Head;
            var positionResolved = Head;
            var remainigs = Metrics.Empty;

            while (positionConfirmed != LinkedReport.Empty)
            {
                var day = positionConfirmed.Day;
                var dayChange = GetDayChange(positionConfirmed);
                var confirmed = dayChange.Confirmed;
                var days = 0D;

                while (confirmed > 0)
                {
                    long recovered;
                    long deaths;

                    if (remainigs != Metrics.Empty)
                    {
                        (confirmed, _, recovered, deaths) = CalcResolution(confirmed, remainigs);

                        if (confirmed == 0)
                        {
                            remainigs = new Metrics(0, 0, recovered, deaths);
                            days = (positionResolved.Day - positionConfirmed.Day).Days - 1;
                            break;
                        }

                        remainigs = Metrics.Empty;
                    }

                    if (positionResolved == LinkedReport.Empty)
                    {
                        days = double.PositiveInfinity;
                        break;
                    }

                    (confirmed, _, recovered, deaths) = CalcResolution(confirmed, GetDayChange(positionResolved));

                    remainigs = new Metrics(0, 0, recovered, deaths);
                    days = (positionResolved.Day - positionConfirmed.Day).Days;
                    positionResolved = positionResolved.Next;
                }

                if (days < 0)
                    days = 0;

                yield return new KeyValuePair<DateTime, double>(day, days);
                positionConfirmed = positionConfirmed.Next;
            }
        }
    }
}