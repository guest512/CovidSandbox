using System;
using System.Collections.Generic;
using ReportsGenerator.Model.Reports.Intermediate;

namespace ReportsGenerator.Model.Reports
{
    /// <summary>
    /// Represents an abstraction for region/province report.
    /// Allows you to look Total and DayChange <see cref="Metrics"/> for each known day,
    /// also provides information about R(t) coefficient and Time-To-Resolve for each day.
    /// </summary>
    public class RegionReport : BaseCountryReport

    {
        private readonly string _countryName;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegionReport"/> class.
        /// </summary>]
        /// <param name="countryName">Region country name.</param>"/>
        /// <param name="name">Region/province name.</param>
        /// <param name="walker">A <see cref="BasicReportsWalker"/> instance for retrieving the data for the region/province.</param>
        public RegionReport(string countryName, string name, BasicReportsWalker walker) : base(walker, name)
        {
            _countryName = countryName;
        }

        /// <inheritdoc />
        protected override Metrics GetDayTotalMetrics(DateTime day) => Walker.GetProvinceTotalByDay(Name, day);

        /// <inheritdoc />
        protected override IEnumerable<string> GetNames() => new[]
        {
            _countryName,
            Name
        };

        /// <inheritdoc />
        protected override Metrics GetDaysChangeMetrics(DateTime startDay, int days) => Walker.GetProvinceChangeForPeriod(Name, startDay, days);
    }
}