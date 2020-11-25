using System;
using System.Collections.Generic;
using ReportsGenerator.Data;

namespace ReportsGenerator.Model.Reports
{
    /// <summary>
    /// Represents a statistical report <see cref="StatsReport"/> node.
    /// </summary>
    public class StatsReportNode : IFormattableReport<string, string>
    {
        private static readonly string[] FormattableReportProperties = {
            "Name",
            "Continent",
            "Population"
        };

        /// <summary>
        /// Initializes a new instance of the top-level <see cref="StatsReportNode"/> class.
        /// </summary>
        /// <param name="name">Name of geographical object.</param>
        /// <param name="continent">Geographical object's continent.</param>
        /// <param name="population">Geographical object's population.</param>
        public StatsReportNode(string name, string continent, in long population) : this(name, continent, population,
            Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatsReportNode"/> class.
        /// </summary>
        /// <param name="name">Name of geographical object.</param>
        /// <param name="continent">Geographical object's continent.</param>
        /// <param name="population">Geographical object's population.</param>
        /// <param name="parent">Geographical object's parent object.</param>
        public StatsReportNode(string name, string continent, in long population, StatsReportNode parent)
        {
            Name = name;
            Parent = parent;
            Continent = continent;
            Population = population;
            Children = new List<StatsReportNode>();
        }

        /// <summary>
        /// Gets an empty <see cref="StatsReportNode"/>. Useful for comparison.
        /// </summary>
        public static StatsReportNode Empty { get; } = new(string.Empty, string.Empty, 0);

        /// <summary>
        /// Gets a collection of child <see cref="StatsReportNode"/> objects.
        /// </summary>
        public ICollection<StatsReportNode> Children { get; }

        /// <summary>
        /// Gets a geographical object continent name.
        /// </summary>
        public string Continent { get; }

        /// <summary>
        /// Gets a geographical object name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a geographical object parent <see cref="StatsReportNode"/>.
        /// </summary>
        public StatsReportNode Parent { get; }

        /// <summary>
        /// Gets a geographical object population.
        /// </summary>
        public long Population { get; }

        #region IFormattableReport

        IEnumerable<string> IFormattableReport<string, string>.Name
        {
            get
            {
                var report = this;
                var name = new List<string>();
                while (report != Empty)
                {
                    name.Add(report.Name);
                    report = report.Parent;
                }

                name.Reverse();

                return name;
            }
        }

        IEnumerable<string> IFormattableReport<string, string>.Properties => FormattableReportProperties;

        ReportType IFormattableReport<string, string>.ReportType => ReportType.Stats;

        IEnumerable<string> IFormattableReport<string, string>.RowIds => new[]
                {
            Name
        };

        object IFormattableReport<string, string>.GetValue(string property, string key) => property switch
        {
            "Name" => Name,
            "Continent" => Continent,
            "Population" => Population,
            _ => throw new ArgumentOutOfRangeException(nameof(property), property, null)
        };

        #endregion IFormattableReport
    }
}