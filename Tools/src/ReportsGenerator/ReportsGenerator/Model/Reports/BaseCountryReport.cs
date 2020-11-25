using System;
using System.Collections.Generic;
using ReportsGenerator.Model.Reports.Intermediate;

namespace ReportsGenerator.Model.Reports
{
    /// <summary>
    /// Represents a base abstraction fro the country and region report.
    /// Implements the logic to retrieve different metrics and data.
    /// </summary>
    public abstract class BaseCountryReport
    {
        protected readonly BasicReportsWalker Walker;
        private Dictionary<DateTime, double>? _timeToResolve;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCountryReport"/> class.
        /// </summary>
        /// <param name="name">Report name.</param>
        /// <param name="head">Pointer to the earliest <see cref="LinkedReport"/> for the geographical object.</param>
        protected BaseCountryReport(BasicReportsWalker head, string name)
        {
            Walker = head;
            Name = name;
            AvailableDates = new[] { Walker.StartDay, Walker.LastDay }.GetContinuousDateRange();
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
        public Metrics GetDayChange(DateTime day) => GetDaysChangeMetrics(day, 1);

        /// <summary>
        /// Calculates a total <see cref="Metrics"/> for the day for this country.
        /// </summary>
        /// <param name="day">A day for which metrics should be calculated.</param>
        /// <returns>A <see cref="Metrics"/> for the day for the country.</returns>
        public Metrics GetDayTotal(DateTime day) => GetDayTotalMetrics(day);

        /// <summary>
        /// Calculates the R(t) coefficient for the day for this country
        /// </summary>
        /// <remarks>
        /// The R(t) coefficient is calculated with the following formula:
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
            var lastDays = GetDaysChangeMetrics(day.AddDays(-3), 4).Confirmed;
            var previousDays = GetDaysChangeMetrics(day.AddDays(-7), 4).Confirmed;

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

        protected abstract Metrics GetDayTotalMetrics(DateTime day);

        protected abstract Metrics GetDaysChangeMetrics(DateTime startDay, int days);

        private IEnumerable<KeyValuePair<DateTime, double>> GetTimeToResolveCollection()
        {
            var positionConfirmed = Walker.StartDay;
            var positionResolved = Walker.StartDay;
            var resolved = 0L;
            var prevDays = 0D;

            while (positionConfirmed <= Walker.LastDay)
            {
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
                            days = (positionResolved - positionConfirmed).Days - 1;
                            break;
                        }
                    }

                    if (positionResolved == Walker.LastDay)
                    {
                        days = double.PositiveInfinity;
                        break;
                    }

                    (confirmed, resolved) = CalcResolution(confirmed, GetDayChange(positionResolved));

                    days = (positionResolved - positionConfirmed).Days;
                    positionResolved = positionResolved.AddDays(1);
                }

                if (days < 0)
                    days = 0;

                if (double.IsPositiveInfinity(prevDays))
                    days = double.PositiveInfinity;

                prevDays = days;
                yield return new KeyValuePair<DateTime, double>(positionConfirmed, days);
                positionConfirmed = positionConfirmed.AddDays(1);
            }
        }
    }
}