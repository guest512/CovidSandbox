using System.Collections.Generic;
using System.Linq;

namespace ReportsGenerator.Model.Reports
{
    /// <summary>
    /// Represents a graph of <see cref="StatsReportNode"/> objects.
    /// Shows a relationship between provinces and counties in the particular country.
    /// </summary>
    public class StatsReport
    {
        private readonly Dictionary<string, List<string>> _namesCache = new();
        private readonly IStatsProvider _statsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatsReport"/> class.
        /// </summary>
        /// <param name="name">Country name.</param>
        /// <param name="statsName">Country name to lookup in <see cref="IStatsProvider"/>.</param>
        /// <param name="statsProvider"><see cref="IStatsProvider"/> instance to collect additional information about geographical objects.</param>
        public StatsReport(string name, string statsName, IStatsProvider statsProvider)
        {
            _statsProvider = statsProvider;
            Root = new StatsReportNode(name, _statsProvider.LookupContinentName(statsName),
                _statsProvider.LookupPopulation(statsName));
        }

        /// <summary>
        /// Gets a pointer to the country <see cref="StatsReportNode"/> node.
        /// </summary>
        public StatsReportNode Root { get; }

        /// <summary>
        /// Adds county, if needed, to the structure.
        /// </summary>
        /// <param name="name">County name.</param>
        /// <param name="statsName">County name to lookup in <see cref="IStatsProvider"/>.</param>
        /// <param name="province">Province where county is belongs to.</param>
        public void AddCounty(string name, string statsName, string province)
        {
            AddProvince(province, statsName[(statsName.IndexOf(',') + 2)..]);

            if (_namesCache[province].Contains(name))
                return;

            var provinceNode = Root.Children.First(node => node.Name == province);

            var countyNode = new StatsReportNode(name,
                _statsProvider.LookupContinentName(statsName),
                _statsProvider.LookupPopulation(statsName),
                provinceNode);

            provinceNode.Children.Add(countyNode);
            _namesCache[province].Add(name);
        }

        /// <summary>
        /// Adds province, if needed, to the structure.
        /// </summary>
        /// <param name="name">Province name.</param>
        /// <param name="statsName">Province name to lookup in <see cref="IStatsProvider"/>.</param>
        public void AddProvince(string name, string statsName)
        {
            if (_namesCache.ContainsKey(name))
                return;

            Root.Children.Add(new StatsReportNode(name,
                _statsProvider.LookupContinentName(statsName),
                _statsProvider.LookupPopulation(statsName),
                Root));

            _namesCache.Add(name, new List<string>());
        }

        /// <summary>
        /// Returns all counties names in the province.
        /// </summary>
        /// <param name="province">Province name.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="string"/>.</returns>
        public IEnumerable<string> GetCounties(string province) =>
            Root.Children.First(c => c.Name == province).Children.Select(c => c.Name);

        /// <summary>
        /// Returns all provinces names in the country.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="string"/>.</returns>
        public IEnumerable<string> GetProvinces() =>
            Root.Children.Select(c => c.Name);
    }
}