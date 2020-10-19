using System.Collections.Generic;

namespace ReportsGenerator.Model.Reports
{
    public class StatsReportNode
    {
        public StatsReportNode(string name, string continent, in long population) : this(name, continent, population, Empty)
        {
        }

        public StatsReportNode(string name, string continent, in long population, StatsReportNode parent)
        {
            Name = name;
            Parent = parent;
            Continent = continent;
            Population = population;
            Children = new List<StatsReportNode>();
        }

        public static StatsReportNode Empty { get; } =
            new StatsReportNode(string.Empty, string.Empty, 0);

        public ICollection<StatsReportNode> Children { get; }
        public string Continent { get; }
        public string Name { get; }
        public StatsReportNode Parent { get; }
        public long Population { get; }
    }
}