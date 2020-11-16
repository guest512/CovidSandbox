using System;

namespace ReportsGenerator.Model.Reports.Intermediate
{
    /// <summary>
    /// Represents an abstraction for intermediate report without any contexts and relations.
    /// </summary>
    public readonly struct BasicReport
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BasicReport"/> class.
        /// </summary>
        /// <param name="name">Id. Name of geographical object.</param>
        /// <param name="parent">Parent Id.</param>
        /// <param name="day">Day for which report contains data.</param>
        /// <param name="total">Report's data.</param>
        public BasicReport(string name, string parent, DateTime day, Metrics total)
        {
            Name = name;
            Parent = parent;
            Day = day;
            Total = total;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicReport"/> class without parent.
        /// </summary>
        /// <param name="name">Id. Name of geographical object.</param>
        /// <param name="day">Day for which report contains data.</param>
        /// <param name="total">Report's data.</param>
        public BasicReport(string name, DateTime day, Metrics total) : this(name, string.Empty, day, total)
        {
        }

        /// <summary>
        /// Gets a day for which report contains data.
        /// </summary>
        public DateTime Day { get; }

        /// <summary>
        /// Gets an id. Name of geographical object.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a parent id.
        /// </summary>
        public string Parent { get; }

        /// <summary>
        /// Gets a report data.
        /// </summary>
        public Metrics Total { get; }

        /// <summary>
        /// Returns the string representation of the report.
        /// </summary>
        /// <returns>The string representation of the report.</returns>
        public override string ToString() => $"{Name}, {Day}: Total({Total})";
    }
}