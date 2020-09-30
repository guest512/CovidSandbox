using System.Collections.Generic;
using System.Linq;

namespace ReportsGenerator.Model.Reports.Intermediate
{
    public class ReportsGraphStructure
    {
        private readonly ReportGraphStructureNode _root;

        public ReportsGraphStructure(string name)
        {
            Name = name;
            _root = new ReportGraphStructureNode(name);
        }

        public string Name { get; }

        public void AddCounty(string name, string province)
        {
            AddProvince(province);

            var provinceNode = _root.Children.First(node => node.Name == province);
            var countyNode = new ReportGraphStructureNode(name, provinceNode);

            if (provinceNode.Children.All(child => child.Name != name))
                provinceNode.Children.Add(countyNode);
        }

        public void AddProvince(string name)
        {
            var provinceNode = new ReportGraphStructureNode(name, _root);
            if (_root.Children.All(child => child.Name != name))
                _root.Children.Add(provinceNode);
        }

        public IEnumerable<string> GetAllCounties() => _root.Children.SelectMany(c => c.Children.Select(cc => cc.Name));

        public IEnumerable<string> GetCounties(string province) => _root.Children.First(c => c.Name == province).Children.Select(c => c.Name);

        public IEnumerable<string> GetProvinces() => _root.Children.Select(c => c.Name);
    }
}