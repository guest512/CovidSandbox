﻿using System;
using System.Collections.Generic;
using ReportsGenerator.Data;
using ReportsGenerator.Model.Reports.Intermediate;

namespace ReportsGenerator.Model.Reports
{
    /// <summary>
    /// Represents a base abstraction fro the country and region report.
    /// Implements the logic to retrieve different metrics and data.
    /// </summary>
    public abstract class BaseCountryReport : IFormattableReport<DateTime, string>
    {
        /// <summary>
        /// A <see cref="BasicReportsWalker"/> instance for retrieving the data for the geographical object.
        /// </summary>
        protected readonly BasicReportsWalker Walker;

        private static readonly string[] FormattableReportProperties = {
            "Date",
            "Total",
            "Change",
            "Rt",
            "TTR"
        };

        private Dictionary<DateTime, double>? _timeToResolve;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCountryReport"/> class.
        /// </summary>
        /// <param name="name">Report name.</param>
        /// <param name="walker">A <see cref="BasicReportsWalker"/> instance for retrieving the data for the geographical object.</param>
        protected BaseCountryReport(BasicReportsWalker walker, string name)
        {
            Walker = walker;
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

        /// <summary>
        /// Returns a change metrics for the specified period.
        /// </summary>
        /// <param name="startDay">Start day of the period to lookup.</param>
        /// <param name="days">Period length in days.</param>
        /// <returns>A <see cref="Metrics"/> that contains change values for the specified period.</returns>
        protected abstract Metrics GetDaysChangeMetrics(DateTime startDay, int days);

        /// <summary>
        /// Returns a total metrics for the specified day.
        /// </summary>
        /// <param name="day">Day to lookup total metrics.</param>
        /// <returns>A <see cref="Metrics"/> that contains total values for the specified day.</returns>
        protected abstract Metrics GetDayTotalMetrics(DateTime day);

        /// <summary>
        /// Helper method to implement <see cref="IFormattableReport{TRow,TName}"/> interface.
        /// </summary>
        /// <returns>See results of <see cref="IFormattableReport{TRow,TName}.Name"/> property.</returns>
        protected abstract IEnumerable<string> GetNames();

        private static (long, long) CalcResolution(long confirmed, long resolved)
        {
            if (resolved <= confirmed)
                return (confirmed - resolved, 0);

            resolved -= confirmed;
            return (0, resolved);
        }

        private static (long, long) CalcResolution(long confirmed, Metrics resolved) =>
            CalcResolution(confirmed, resolved.Recovered + resolved.Deaths);

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

        #region IFormattableReport

        IEnumerable<string> IFormattableReport<DateTime, string>.Name => GetNames();
        IEnumerable<string> IFormattableReport<DateTime, string>.Properties => FormattableReportProperties;

        ReportType IFormattableReport<DateTime, string>.ReportType => ReportType.Country;
        IEnumerable<DateTime> IFormattableReport<DateTime, string>.RowIds => AvailableDates;

        object IFormattableReport<DateTime, string>.GetValue(string property, DateTime key) => property switch
        {
            "Date" => key,
            "Total" => GetDayTotal(key),
            "Change" => GetDayChange(key),
            "Rt" => GetRt(key),
            "TTR" => GetTimeToResolve(key),
            _ => throw new ArgumentOutOfRangeException(nameof(property), property, null)
        };

        #endregion IFormattableReport
    }
}