using System;
using System.Collections.Generic;
using System.Linq;
using ReportsGenerator.Model.Reports.Intermediate;

namespace ReportsGenerator.Model.Reports
{
    /// <summary>
    /// Represents a base abstraction fro the country and region report.
    /// Implements the logic to retrieve different metrics and data.
    /// </summary>
    public abstract class BaseCountryReport
    {
        private readonly LinkedReport _head;
        private Dictionary<DateTime, double>? _timeToResolve;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCountryReport"/> class.
        /// </summary>
        /// <param name="name">Report name.</param>
        /// <param name="head">Pointer to the earliest <see cref="LinkedReport"/> for the geographical object.</param>
        protected BaseCountryReport(LinkedReport head, string name)
        {
            _head = head;
            Name = name;
            AvailableDates = _head.GetAvailableDates().GetContinuousDateRange()
                .ToArray();
        }

        /// <summary>
        /// Gets a collection of known dates included in this report.
        /// </summary>
        public IEnumerable<DateTime> AvailableDates { get; }

        /// <summary>
        /// Gets a geographical object name for which report was built.
        /// </summary>
        public string Name { get; }

        private Dictionary<DateTime, double> TimeToResolve => _timeToResolve ??= new Dictionary<DateTime, double>(GetTimeToResolveCollection());

        /// <summary>
        /// Calculates a day-change <see cref="Metrics"/> for this country.
        /// </summary>
        /// <param name="day">A day for which metrics should be calculated.</param>
        /// <returns>A <see cref="Metrics"/> for the day for the country.</returns>
        public Metrics GetDayChange(DateTime day)
        {
            var position = GetDayReport(day);

            return GetDayChange(position);
        }

        /// <summary>
        /// Calculates a total <see cref="Metrics"/> for the day for this country.
        /// </summary>
        /// <param name="day">A day for which metrics should be calculated.</param>
        /// <returns>A <see cref="Metrics"/> for the day for the country.</returns>
        public Metrics GetDayTotal(DateTime day)
        {
            var position = GetDayReport(day);

            return position.Total;
        }

        /// <summary>
        /// Calculates the R(t) coefficient for the day for this country
        /// </summary>
        /// <remarks>
        /// The R(t) coefficient is calulcated with the following formula:
        /// 
        /// R(i) = (cc[i] + cc[i-1] + cc[i-2] + cc[i-3]) / (cc[i-4] + cc[i-5] + cc[i-6] + cc[i-7]) ,
        ///
        /// where cc[i] is a 'Confirmed' change for the i-day.
        /// 
        /// </remarks>
        /// <param name="day">A day for which metrics should be calculated.</param>
        /// <returns>A R(t) coefficient value for the day for the country.</returns>
        public double GetRt(DateTime day)
        {
            if (_head.Day.AddDays(7) > day)
                return 0;

            var position = GetDayReport(day);

            static long GetCumulativeConfirmedChange(IEnumerable<LinkedReport> reports) =>
                reports.Aggregate(0L, (sum, rep) => sum + (rep.Total - rep.Previous.Total).Confirmed);

            var lastDays = GetCumulativeConfirmedChange(GetReports(position, 4, false));
            var previousDays = GetCumulativeConfirmedChange(GetReports(GetDayReport(day.AddDays(-4)), 4, false));

            return previousDays > 0 ? lastDays / (double)previousDays : 0;
        }

        /// <summary>
        /// Calculates a time-to-resolve(TTR) metrics for the day for this country.
        /// </summary>
        /// <remarks>
        /// TTR - represents a number of days needed to resolve all new confirmed cases for the requested day.
        /// Resolution includes both recovering and death. NOTE: this coefficient is very sensitive to the
        /// provided data about cases, and could be very misleading for some countries. For instance, Spain,
        /// doesn't provide accurate information about recovered cases at all, and makes this coefficient useless.
        /// </remarks>
        /// <param name="day">A day for which metrics should be calculated.</param>
        /// <returns>A TTR coefficient value for the day for the country.</returns>
        public double GetTimeToResolve(DateTime day)
        {
            return TimeToResolve[day];
        }

        private static (long, long) CalcResolution(long confirmed, long resolved)
        {
            if (resolved <= confirmed)
                return (confirmed - resolved, 0);

            resolved -= confirmed;
            return (0, resolved);
        }

        private static (long, long) CalcResolution(long confirmed, Metrics resolved) =>
            CalcResolution(confirmed, resolved.Recovered + resolved.Deaths);

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
            var position = _head;

            while (position.Next.Day <= day && position.Next != LinkedReport.Empty)
            {
                position = position.Next;
            }

            return position;
        }

        private IEnumerable<KeyValuePair<DateTime, double>> GetTimeToResolveCollection()
        {
            var positionConfirmed = _head;
            var positionResolved = _head;
            var resolved = 0L;
            var prevDays = 0D;

            while (positionConfirmed != LinkedReport.Empty)
            {
                var day = positionConfirmed.Day;
                var dayChange = GetDayChange(positionConfirmed);
                var confirmed = dayChange.Confirmed;
                var days = 0D;

                while (confirmed > 0)
                {
                    if (resolved > 0)
                    {
                        (confirmed, resolved) = CalcResolution(confirmed, resolved);

                        if (confirmed == 0)
                        {
                            days = (positionResolved.Day - positionConfirmed.Day).Days - 1;
                            break;
                        }
                    }

                    if (positionResolved == LinkedReport.Empty)
                    {
                        days = double.PositiveInfinity;
                        break;
                    }

                    (confirmed, resolved) = CalcResolution(confirmed, GetDayChange(positionResolved));

                    days = (positionResolved.Day - positionConfirmed.Day).Days;
                    positionResolved = positionResolved.Next;
                }

                if (days < 0)
                    days = 0;

                if (double.IsPositiveInfinity(prevDays))
                    days = double.PositiveInfinity;

                prevDays = days;
                yield return new KeyValuePair<DateTime, double>(day, days);
                positionConfirmed = positionConfirmed.Next;
            }
        }
    }
}