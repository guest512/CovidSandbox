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
        public CountryReport(string name, LinkedReport head) : base(head, name)
        {
            RegionReports =
                (head.Children.Any(child => child.Name != Consts.MainCountryRegion)
                    ? head.Children.Where(child => child.Name != Consts.MainCountryRegion)
                    : head.Children)
                .Select(child => new RegionReport(child.Name, child))
                .ToArray();
        }

        /// <summary>
        /// Gets a collection of the country's regions reports.
        /// </summary>
        public IEnumerable<RegionReport> RegionReports { get; }
    }
}