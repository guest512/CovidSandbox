using System;

namespace ReportsGenerator.Model.Reports.Intermediate
{
    /// <summary>
    /// Represents an abstraction for intermediate report without any contexts and relations.
    /// </summary>
    public record BasicReport
    {
        /// <summary>
        /// Gets a day for which report contains data.
        /// </summary>
        public DateTime Day { get; init; } = DateTime.MinValue;

        /// <summary>
        /// Gets an id. Name of geographical object.
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Gets a parent id.
        /// </summary>
        public string Parent { get; init; } = string.Empty;

        /// <summary>
        /// Gets a report data.
        /// </summary>
        public Metrics Total { get; init; } = Metrics.Empty;

        public static BasicReport Empty { get; } = new BasicReport();

        /// <summary>
        /// Returns the string representation of the report.
        /// </summary>
        /// <returns>The string representation of the report.</returns>
        public override string ToString() => $"{Name}, {Day}: Total({Total})";
    }
}