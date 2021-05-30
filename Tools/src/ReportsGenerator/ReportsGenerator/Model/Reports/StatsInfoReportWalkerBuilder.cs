using System.Collections.Generic;
using System.Linq;

namespace ReportsGenerator.Model.Reports
{
    /// <summary>
    /// Helper class to build <see cref="StatsInfoReportWalker"/> from strings.
    /// </summary>
    public class StatsInfoReportWalkerBuilder
    {
        private readonly StatsInfoStructureNode _countryName;
        private readonly IStatsProvider _statsProvider;
        private readonly Dictionary<StatsInfoStructureNode, List<StatsInfoStructureNode>> _countryStructure = new();
        private readonly Dictionary<string, List<string>> _namesCache = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="StatsInfoReportWalkerBuilder"/> class.
        /// </summary>
        /// <param name="country">Country name.</param>
        /// <param name="statsProvider"><see cref="IStatsProvider"/> instance to collect additional information about geographical objects.</param>
        public StatsInfoReportWalkerBuilder(string country, IStatsProvider statsProvider)
        {
            _statsProvider = statsProvider;
            _countryName = new StatsInfoStructureNode(country, country, statsProvider.GetCountryStatsName(country));
        }

        /// <summary>
        /// Adds province, if needed, to the structure.
        /// </summary>
        /// <param name="provinceName">Province name.</param>
        /// <param name="provinceStatsName">Province name to lookup in <see cref="IStatsProvider"/>.</param>
        public void AddProvince(string provinceName, string provinceStatsName)
        {
            AddProvinceNode(provinceName, provinceStatsName);
        }

        private StatsInfoStructureNode AddProvinceNode(string provinceName, string provinceStatsName)
        {
            if (_namesCache.ContainsKey(provinceName))
                return _countryStructure.Keys.First(k => k.Name == provinceName);

            var provinceNode = new StatsInfoStructureNode(_countryName.Name, provinceName, provinceStatsName);

            _countryStructure.Add(provinceNode, new List<StatsInfoStructureNode>());
            _namesCache.Add(provinceName, new List<string>());

            return provinceNode;
        }

        /// <summary>
        /// Adds county, if needed, to the structure.
        /// </summary>
        /// <param name="countyName">County name.</param>
        /// <param name="countyStatsName">County name to lookup in <see cref="IStatsProvider"/>.</param>
        /// <param name="provinceName">Province where county is belonged to.</param>
        public void AddCounty(string provinceName, string countyName, string countyStatsName)
        {
            StatsInfoStructureNode provinceNode;

            if (_namesCache.ContainsKey(provinceName))
            {
                if (_namesCache[provinceName].Contains(countyName))
                    return;

                provinceNode = _countryStructure.Keys.First(k => k.Name == provinceName);
            }
            else
            {
                provinceNode = AddProvinceNode(provinceName, countyStatsName[(countyStatsName.IndexOf(',') + 2)..]);
            }

            _countryStructure[provinceNode].Add(new StatsInfoStructureNode(_countryName.Name, countyName, countyStatsName));
            _namesCache[provinceName].Add(countyName);
        }

        /// <summary>
        /// Builds <see cref="StatsInfoReportWalker"/> from the current state.
        /// </summary>
        /// <returns>A new instance of the <see cref="StatsInfoReportWalker"/>.</returns>
        public StatsInfoReportWalker Build()
        {
            var walker = new StatsInfoReportWalker(_countryName.Name);

            walker.AddReport(NodeToReport(_countryName));

            foreach (var (province, counties) in _countryStructure)
            {
                walker.AddReport(NodeToReport(province, _countryName.Name));

                foreach (var county in counties)
                {
                    walker.AddReport(NodeToReport(county, province.Name));
                }
            }

            return walker;
        }

        private StatsInfoReport NodeToReport(StatsInfoStructureNode node, string parentName = "") =>
            new()
            {
                Country = node.CountryName,
                Parent = parentName,
                Name = node.Name,
                Continent = _statsProvider.LookupContinentName(node.StatsName),
                Population = _statsProvider.LookupPopulation(node.StatsName),
                StatsName = node.StatsName
            };

        private record StatsInfoStructureNode(string CountryName, string Name, string StatsName);
    }
}