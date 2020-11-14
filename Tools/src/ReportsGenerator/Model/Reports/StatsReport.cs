using System.Collections.Generic;
using System.Linq;

namespace ReportsGenerator.Model.Reports
{
    public class StatsReport
    {
        private readonly IStatsProvider _statsProvider;
        private readonly Dictionary<string, List<string>> _namesCache = new Dictionary<string, List<string>>();
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

        public void AddProvince(string name, string statsName)
        {
            if(_namesCache.ContainsKey(name))
                return;

            Root.Children.Add(new StatsReportNode(name,
                _statsProvider.LookupContinentName(statsName),
                _statsProvider.LookupPopulation(statsName),
                Root));

            _namesCache.Add(name, new List<string>());
        }

        public IEnumerable<string> GetAllCounties() =>
            Root.Children.SelectMany(c => c.Children.Select(cc => cc.Name));

        public IEnumerable<string> GetCounties(string province) =>
            Root.Children.First(c => c.Name == province).Children.Select(c => c.Name);

        public IEnumerable<string> GetProvinces() =>
            Root.Children.Select(c => c.Name);
    }
}