using System;
using System.Collections.Generic;
using System.Linq;

namespace ReportsGenerator.Model.Reports
{
    public class StatsInfoReportWalker
    {
        private readonly List<StatsInfoReport> _provinces = new ();
        private StatsInfoReport? _country;
        private readonly string _countryName;
        private readonly Dictionary<string, List<StatsInfoReport>> _counties = new();

        public StatsInfoReportWalker(string countryName)
        {
            _countryName = countryName;
        }

        public void AddReport(StatsInfoReport report)
        {
            if (string.IsNullOrEmpty(report.Parent))
            {
                if (_country != null) 
                    return;

                if (report.Name != _countryName)
                {
                    throw new InvalidOperationException(
                        $"Wrong country report in this walker. Report = {report.Name}, Walker = {_countryName}");
                }

                _country = report;
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

        public StatsInfoReport Country => _country ?? StatsInfoReport.Empty;

        public IEnumerable<StatsInfoReport> Provinces => _provinces;

        /// <summary>
        /// Returns all counties names in the province.
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