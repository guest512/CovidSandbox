using System;
using System.Collections.Generic;
using System.Linq;
using ReportsGenerator.Model.Reports.Intermediate;

namespace ReportsGenerator.Model.Reports
{
    /// <summary>
    /// Represents an abstraction for country report.
    /// Allows you to look Total and DayChange <see cref="Metrics"/> for each known day,
    /// also provides information about R(t) coefficient and Time-To-Resolve for each day.
    /// </summary>
    public class CountryReport : BaseCountryReport
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegionReport"/> class.
        /// </summary>
        /// <param name="name">Country name.</param>
        /// <param name="head">Pointer to the earliest <see cref="LinkedReport"/> for the country.</param>
        public CountryReport(string name, BasicReportsWalker walker, StatsReport structure) : base(walker, name)
        {
            RegionReports = structure.GetProvinces().Select(province => new RegionReport(province, walker));
        }

        /// <summary>
        /// Gets a collection of the country's regions reports.
        /// </summary>
        public IEnumerable<RegionReport> RegionReports { get; }

        protected override Metrics GetDaysMetrics(DateTime startDay, int days) => Walker.GetCountryMetricsForPeriod(startDay, days);
    }
}