using System;
using System.Collections.Generic;
using ReportsGenerator.Data;

namespace ReportsGenerator.Model.Reports
{
    /// <summary>
    /// Represents a statistical report for the country or its part.
    /// </summary>
    public record StatsInfoReport(
        string Country = "",
        string Name = "",
        string Parent = "",
        string Continent = "",
        long Population = 0,
        string StatsName = "") : IFormattableReport<string, string>
    {
        private static readonly string[] FormattableReportProperties =
        {
            "Name",
            "Continent",
            "Population"
        };

        /// <summary>
        /// Gets an empty (with default values) instance of <see cref="StatsInfoReport"/>.
        /// </summary>
        public static StatsInfoReport Empty { get; } = new();

        #region IFormattableReport

        IEnumerable<string> IFormattableReport<string, string>.Name
        {
            get
            {
                if (string.IsNullOrEmpty(Parent))
                    return new[] { Name };

                return Parent == Country ? new[] { Parent, Name } : new[] { Country, Parent, Name };
            }
        }

        IEnumerable<string> IFormattableReport<string, string>.Properties { get; } = FormattableReportProperties;

        IEnumerable<string> IFormattableReport<string, string>.RowIds => new[]
        {
            Name
        };

        ReportType IFormattableReport<string, string>.ReportType { get; } = ReportType.Stats;

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