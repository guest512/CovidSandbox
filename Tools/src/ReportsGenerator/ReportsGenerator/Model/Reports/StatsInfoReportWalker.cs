using System;
using System.Collections.Generic;
using System.Linq;

namespace ReportsGenerator.Model.Reports
{
    /// <summary>
    /// Represents a class to convert a bunch of <see cref="StatsInfoReport"/> to its tree view.
    /// </summary>
    public class StatsInfoReportWalker
    {
        private readonly List<StatsInfoReport> _provinces = new ();
        private StatsInfoReport? _country;
        private readonly string _countryName;
        private readonly Dictionary<string, List<StatsInfoReport>> _counties = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatsInfoReportWalker"/> class.
        /// </summary>
        /// <param name="countryName">Country name.</param>
        public StatsInfoReportWalker(string countryName)
        {
            _countryName = countryName;
        }

        /// <summary>
        /// Adds <see cref="StatsInfoReport"/> to the structure.
        /// </summary>
        /// <param name="report">Report to add.</param>
        /// <exception cref="InvalidOperationException">Throws if, <see cref="StatsInfoReport.Country"/> not the same as country name.</exception>
        public void AddReport(StatsInfoReport report)
        {
            if (report.Country != _countryName)
            {
                throw new InvalidOperationException(
                    $"Wrong report in this walker. Report = {report.Country} - {report.Name}, Walker = {_countryName}");
            }

            if (string.IsNullOrEmpty(report.Parent))
            {
                _country ??= report;
            }
            else if (report.Parent == _countryName)
            {
                _provinces.Add(report);
            }
            else
            {
               
                if (!_counties.ContainsKey(report.Parent))
                    _counties[report.Parent] = new List<StatsInfoReport>();

                _counties[report.Parent].Add(report);
            }
        }

        /// <summary>
        /// Gets a root node (Country node).
        /// </summary>
        public StatsInfoReport Country => _country ?? StatsInfoReport.Empty;

        /// <summary>
        /// Gets a collection of provinces nodes.
        /// </summary>
        public IEnumerable<StatsInfoReport> Provinces => _provinces;

        /// <summary>
        /// Returns all counties reports in the province.
        /// </summary>
        /// <param name="province">Province name.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="StatsInfoReport"/>.</returns>
        public IEnumerable<StatsInfoReport> GetCounties(string province)
        {
            return _counties.TryGetValue(province, out var counties)
                ? counties
                : Enumerable.Empty<StatsInfoReport>();
        }
    }
}