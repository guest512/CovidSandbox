using System.Collections.Generic;

namespace ReportsGenerator.Model.Reports.Intermediate
{
    public class ReportGraphStructureNode
    {
        public ReportGraphStructureNode(string name) : this(name, Empty)
        {
        }

        public ReportGraphStructureNode(string name, ReportGraphStructureNode parent)
        {
            Name = name;
            Parent = parent;
            Children = new List<ReportGraphStructureNode>();
        }

        public static ReportGraphStructureNode Empty { get; } = new ReportGraphStructureNode(string.Empty);
        public ICollection<ReportGraphStructureNode> Children { get; }
        public string Name { get; }

        public ReportGraphStructureNode Parent { get; }
    }
}