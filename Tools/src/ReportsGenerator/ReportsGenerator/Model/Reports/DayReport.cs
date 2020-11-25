using System;
using System.Collections.Generic;
using System.Linq;
using ReportsGenerator.Model.Reports.Intermediate;

namespace ReportsGenerator.Model.Reports
{
    /// <summary>
    /// Represents an abstraction for day report.
    /// Allows you to look Total and DayChange <see cref="Metrics"/> for each known country.
    /// </summary>
    public class DayReport
    {
        private readonly IDictionary<string, BasicReportsWalker> _reports;

        /// <summary>
        /// Initializes a new instance of the <see cref="DayReport"/> class.
        /// </summary>
        /// <param name="day">Day for which report creates.</param>
        /// <param name="reportWalkers">A <see cref="IDictionary{TKey,TValue}"/> of <see cref="LinkedReport"/> and its names, to create the report. </param>
        public DayReport(in DateTime day, IDictionary<string, BasicReportsWalker> reportWalkers)
        {
            Day = day;
            _reports = reportWalkers;
            AvailableCountries = _reports.Keys;
        }

        /// <summary>
        /// Gets a collection of countries represented in this report.
        /// </summary>
        public IEnumerable<string> AvailableCountries { get; }

        /// <summary>
        /// Gets a day for which report was generated.
        /// </summary>
        public DateTime Day { get; }

        /// <summary>
        /// Returns a day-change <see cref="Metrics"/> for the country.
        /// </summary>
        /// <param name="countryName">Country to search for.</param>
        /// <returns>A <see cref="Metrics"/> for the day for the country.</returns>
        public Metrics GetCountryChange(string countryName) => _reports[countryName].GetCountryChangeForPeriod(Day, 1);

        /// <summary>
        /// Returns a total for this day for the country.
        /// </summary>
        /// <param name="countryName">Country to search for.</param>
        /// <returns>A <see cref="Metrics"/> for the day for the country.</returns>
        public Metrics GetCountryTotal(string countryName) => _reports[countryName].GetCountryTotalByDay(Day);
    }
}