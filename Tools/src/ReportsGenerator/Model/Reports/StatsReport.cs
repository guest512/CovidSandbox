using System.Collections.Generic;
using System.Linq;

namespace ReportsGenerator.Model.Reports
{
    public class StatsReport
    {
        private readonly IStatsProvider _statsProvider;
        public StatsReportNode Root { get; }

        public StatsReport(string name, string statsName, IStatsProvider statsProvider)
        {
            _statsProvider = statsProvider;
            Root = new StatsReportNode(name, _statsProvider.LookupContinentName(statsName),
                _statsProvider.LookupPopulation(statsName));
        }

        public void AddCounty(string name, string statsName, string province)
        {
            AddProvince(province, statsName[(statsName.IndexOf(',') + 2)..]);
            var provinceNode = Root.Children.First(node => node.Name == province);

            if (provinceNode.Children.Any(child => child.Name == name))
                return;

            var countyNode = new StatsReportNode(name,
                _statsProvider.LookupContinentName(statsName),
                _statsProvider.LookupPopulation(statsName),
                provinceNode);

            provinceNode.Children.Add(countyNode);
        }

        public void AddProvince(string name, string statsName)
        {
            if (Root!.Children.All(child => child.Name != name))
                Root.Children.Add(new StatsReportNode(name,
                    _statsProvider.LookupContinentName(statsName),
                    _statsProvider.LookupPopulation(statsName),
                    Root));
        }

        public IEnumerable<string> GetAllCounties() =>
            Root.Children.SelectMany(c => c.Children.Select(cc => cc.Name));

        public IEnumerable<string> GetCounties(string province) =>
            Root.Children.First(c => c.Name == province).Children.Select(c => c.Name);

        public IEnumerable<string> GetProvinces() =>
            Root.Children.Select(c => c.Name);
    }
}